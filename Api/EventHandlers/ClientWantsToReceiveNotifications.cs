using System.Text.Json;
using Api.Dtos;
using Api.Filters;
using Api.State;
using Core.Exceptions;
using Core.Interfaces;
using Fleck;
using lib;

namespace Api.EventHandlers;

[RequireAuthentication]
public class ClientWantsToReceiveNotifications : BaseEventHandler<ClientWantsToReceiveNotificationsDto>
{
    private readonly ICarControlService _notificationService;
    private readonly IWebSocketConnectionManager _webSocketConnectionManager;
    private readonly ILogger<ClientWantsToReceiveNotifications> _logger;

    public ClientWantsToReceiveNotifications(ICarControlService notificationService, IWebSocketConnectionManager webSocketConnectionManager, ILogger<ClientWantsToReceiveNotifications> logger)
    {
        _notificationService = notificationService;
        _webSocketConnectionManager = webSocketConnectionManager;
        _logger = logger;
        _notificationService.OnNotificationReceived += OnNotificationReceived;
    }

    private void OnNotificationReceived(string topic, string message)
    {
        foreach (var socket in _webSocketConnectionManager.GetAllConnections())
        {
            try
            {
                _logger.LogInformation("Sending notification to client {ClientId} on topic {Topic}.", socket.Connection.ConnectionInfo.Id, topic);
                socket.Connection.Send($"Notification on '{topic}': {message}");
            }
            catch (AppException ex)
            {
                _logger.LogError(ex, "AppException occurred while sending notification to client {ClientId}.", socket.Connection.ConnectionInfo.Id);

                socket.Connection.Send(JsonSerializer.Serialize(new ServerSendsErrorMessageToClientDto
                {
                    ErrorMessage = ex.Message
                }));
            }
            catch (Exception ex)
            {
                var errorMessage = "An unexpected error occurred. Please try again later.";

                _logger.LogError(ex, "Unexpected error occurred while sending notification to client {ClientId}.", socket.Connection.ConnectionInfo.Id);

                socket.Connection.Send(JsonSerializer.Serialize(new ServerSendsErrorMessageToClientDto
                {
                    ErrorMessage = errorMessage
                }));
            }
        }
    }

    public override async Task Handle(ClientWantsToReceiveNotificationsDto dto, IWebSocketConnection socket)
    {
        _logger.LogInformation("Client {ClientId} requested to receive notifications.", socket.ConnectionInfo.Id);
        await Task.CompletedTask;
    }
}
