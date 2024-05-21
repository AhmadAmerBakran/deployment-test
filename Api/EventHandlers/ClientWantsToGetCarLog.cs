using System.Text.Json;
using Api.Dtos;
using Api.Filters;
using Core.Exceptions;
using Core.Interfaces;
using Fleck;
using lib;

namespace Api.EventHandlers;

[RequireAuthentication]
public class ClientWantsToGetCarLog : BaseEventHandler<ClientWantsToGetCarLogDto>
{
    private readonly ICarControlService _carControlService;
    private readonly ILogger<ClientWantsToGetCarLog> _logger;

    public ClientWantsToGetCarLog(ICarControlService carControlService, ILogger<ClientWantsToGetCarLog> logger)
    {
        _carControlService = carControlService;
        _logger = logger;
    }

    public override Task Handle(ClientWantsToGetCarLogDto dto, IWebSocketConnection socket)
    {
        try
        {
            _logger.LogInformation("Client {ClientId} requested car log.", socket.ConnectionInfo.Id);

            var notifications = _carControlService.GetCarLog().Result;
            foreach (var not in notifications)
            {
                socket.Send(JsonSerializer.Serialize(not));
            }

            _logger.LogInformation("Sent car log to client {ClientId}.", socket.ConnectionInfo.Id);
        }
        catch (AppException ex)
        {
            _logger.LogError(ex, "An AppException occurred while fetching the car log for client {ClientId}.", socket.ConnectionInfo.Id);

            socket.Send(JsonSerializer.Serialize(new ServerSendsErrorMessageToClientDto
            {
                ErrorMessage = ex.Message
            }));
        }
        catch (Exception ex)
        {
            var errorMessage = "An unexpected error occurred. Please try again later.";

            _logger.LogError(ex, "An unexpected error occurred while fetching the car log for client {ClientId}.", socket.ConnectionInfo.Id);

            socket.Send(JsonSerializer.Serialize(new ServerSendsErrorMessageToClientDto
            {
                ErrorMessage = errorMessage
            }));
        }
        return Task.CompletedTask;
    }
}
