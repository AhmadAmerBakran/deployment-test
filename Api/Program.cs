using System.Reflection;
using System.Text.Json;
using Api.Dtos;
using Api.EventHandlers;
using Api.Filters;
using Core.Interfaces;
using Fleck;
using Infrastructure;
using MQTTClient;
using Service;
using Api.State;
using Core.Exceptions;
using Infrastructure.Repositories;
using lib;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddSingleton<IMQTTClientManager, MQTTClientManager>();
builder.Services.AddNpgsqlDataSource(Utilities.ProperlyFormattedConnectionString, dataSourceBuilder => dataSourceBuilder.EnableParameterLogging());
builder.Services.AddSingleton<ICarControlService, CarControlService>();
builder.Services.AddSingleton<IWebSocketConnectionManager, WebSocketConnectionManager>();
builder.Services.AddSingleton<ICarLogRepository, CarLogRepository>();

// Register filters with transient lifetime to avoid conflicts.
builder.Services.AddTransient<ValidateDataAnnotations>(); 
builder.Services.AddTransient<RequireAuthenticationAttribute>();

// Register event handlers.
builder.Services.AddSingleton<BaseEventHandler<ClientWantsToControlCarDto>, ClientWantsToControlCar>();
builder.Services.AddSingleton<BaseEventHandler<ClientWantsToSignInDto>, ClientWantsToSignIn>();
builder.Services.AddSingleton<BaseEventHandler<ClientWantsToSignOutDto>, ClientWantsToSignOut>();
builder.Services.AddSingleton<BaseEventHandler<ClientWantsToGetCarLogDto>, ClientWantsToGetCarLog>();
builder.Services.AddSingleton<BaseEventHandler<ClientWantsToReceiveNotificationsDto>, ClientWantsToReceiveNotifications>();

// Configure logging.
builder.Services.AddLogging(config =>
{
    config.AddConsole();
    config.AddDebug();
});

var clientEventHandlers = builder.FindAndInjectClientEventHandlers(Assembly.GetExecutingAssembly());

var app = builder.Build();

var connectionManager = app.Services.GetRequiredService<IWebSocketConnectionManager>();
var logger = app.Services.GetRequiredService<ILogger<Program>>();

var userConnectionId = Guid.Empty;
var espConnectionId = Guid.Empty;

builder.WebHost.UseUrls("http://*:9999");
var port = Environment.GetEnvironmentVariable("PORT") ?? "8181";
var server = new WebSocketServer("ws://0.0.0.0:" + port);

ServiceLocator.ServiceProvider = app.Services;

server.Start(socket =>
{
    socket.OnOpen = () =>
    {
        try
        {
            var connectionPool = connectionManager.GetAllConnections();
            connectionManager.AddConnection(socket.ConnectionInfo.Id, socket);
            logger.LogInformation($"New connection added with GUID: {socket.ConnectionInfo.Id}");
        }
        catch (AppException ex)
        {
            socket.Send(ex.Message);
            logger.LogError(ex, $"AppException: {ex.Message}");
        }
        catch (Exception ex)
        {
            var errorMessage = "An unexpected error occurred during the connection process. Please try again later.";
            socket.Send(errorMessage);
            logger.LogError(ex, $"Exception: {ex.Message}");
        }
    };

    socket.OnMessage = async message =>
    {
        try
        {
            if (message == "ESP32-CAM")
            {
                // Handle ESP32-CAM connection
                if (espConnectionId != Guid.Empty && espConnectionId != socket.ConnectionInfo.Id)
                {
                    var existingSocket = connectionManager.GetConnection(espConnectionId)?.Connection;
                    existingSocket?.Close();
                }
                espConnectionId = socket.ConnectionInfo.Id;
                logger.LogInformation($"ESP32-CAM connected with ID: {espConnectionId}");
            }
            else
            {
                // Handle User connection
                if (userConnectionId == Guid.Empty || userConnectionId == socket.ConnectionInfo.Id)
                {
                    userConnectionId = socket.ConnectionInfo.Id;
                    logger.LogInformation($"User connected with ID: {userConnectionId}");
                    await app.InvokeClientEventHandler(clientEventHandlers, socket, message);
                }
                else
                {
                    socket.Send(JsonSerializer.Serialize(new ServerSendsErrorMessageToClientDto()
                    {
                        ErrorMessage = "The car is in use right now, please try again later"
                    }));
                    socket.Close();
                }
            }
        }
        catch (AppException ex)
        {
            socket.Send(ex.Message);
            logger.LogError(ex, $"AppException: {ex.Message}");
        }
        catch (Exception ex)
        {
            socket.Send(JsonSerializer.Serialize(new ServerSendsErrorMessageToClientDto()
            {
                ErrorMessage = ex.Message
            }));
            logger.LogError(ex, $"Exception: {ex.Message}");
        }
    };

    socket.OnClose = async () =>
    {
        try
        {
            logger.LogInformation("Connection closed.");

            await connectionManager.ResetCarStateToDefault(socket.ConnectionInfo.Id);
            
            connectionManager.RemoveConnection(socket.ConnectionInfo.Id);
            if (!connectionManager.HasMetadata(socket.ConnectionInfo.Id))
            {
                logger.LogInformation($"Metadata successfully removed for GUID: {socket.ConnectionInfo.Id}");
            }
            else
            {
                logger.LogWarning($"Failed to remove metadata for GUID: {socket.ConnectionInfo.Id}");
            }

            if (userConnectionId == socket.ConnectionInfo.Id)
            {
                userConnectionId = Guid.Empty;
            }
            if (espConnectionId == socket.ConnectionInfo.Id)
            {
                espConnectionId = Guid.Empty;
            }
        }
        catch (AppException ex)
        {
            socket.Send(ex.Message);
            logger.LogError(ex, $"AppException: {ex.Message}");
        }
        catch (Exception ex)
        {
            var errorMessage = "An unexpected error occurred while closing the connection. Please try again later.";
            socket.Send(errorMessage);
            logger.LogError(ex, $"Exception: {ex.Message}");
        }
    };

    socket.OnBinary = (data) =>
    {
        try
        {
            var connections = connectionManager.GetAllConnections();
            foreach (var conn in connections)
            {
                conn.Connection.Send(data);
                logger.LogInformation($"Sent frame with length: {data.Length} to connection");
            }
        }
        catch (AppException ex)
        {
            socket.Send(ex.Message);
            logger.LogError(ex, $"AppException: {ex.Message}");
        }
        catch (Exception ex)
        {
            var errorMessage = "An unexpected error occurred while processing binary data. Please try again later.";
            socket.Send(errorMessage);
            logger.LogError(ex, $"Exception: {ex.Message}");
        }
    };
});

app.Run();
