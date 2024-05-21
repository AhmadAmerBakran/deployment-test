using System.Text.Json;
using Api.Dtos;
using Api.State;
using Core.Exceptions;
using Core.Interfaces;
using Fleck;
using lib;

namespace Api.EventHandlers;

public class ClientWantsToSignOut : BaseEventHandler<ClientWantsToSignOutDto>
{
    private readonly IWebSocketConnectionManager _webSocketConnectionManager;
    private readonly ICarControlService _carControlService;
    private readonly ILogger<ClientWantsToSignOut> _logger;

    public ClientWantsToSignOut(IWebSocketConnectionManager webSocketConnectionManager, ICarControlService carControlService, ILogger<ClientWantsToSignOut> logger)
    {
        _webSocketConnectionManager = webSocketConnectionManager;
        _carControlService = carControlService;
        _logger = logger;
    }

    public override async Task Handle(ClientWantsToSignOutDto dto, IWebSocketConnection socket)
    {
        try
        {
            var connectionId = socket.ConnectionInfo.Id;

            // Reset car state to default
            _webSocketConnectionManager.ResetConnection(connectionId);

            _webSocketConnectionManager.ResetConnection(connectionId); // Reset metadata
            _webSocketConnectionManager.StartDisconnectTimer(connectionId, () =>
            {
                _webSocketConnectionManager.RemoveConnection(connectionId);
                socket.Close();
            }, 30000);

            _logger.LogInformation("User signed out, but connection remains open. Car state reset to default.");
        }
        catch (AppException ex)
        {
            socket.Send(JsonSerializer.Serialize(new ServerSendsErrorMessageToClientDto
            {
                ErrorMessage = ex.Message
            }));
            _logger.LogError(ex, "AppException occurred during sign out.");
        }
        catch (Exception ex)
        {
            var errorMessage = "An unexpected error occurred. Please try again later.";
            socket.Send(JsonSerializer.Serialize(new ServerSendsErrorMessageToClientDto
            {
                ErrorMessage = errorMessage
            }));
            _logger.LogError(ex, "Exception occurred during sign out.");
        }

        await Task.CompletedTask;
    }
}
