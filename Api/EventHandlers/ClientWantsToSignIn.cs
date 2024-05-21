using System.Text.Json;
using Api.Dtos;
using Api.Filters;
using Api.State;
using Core.Exceptions;
using Core.Interfaces;
using Fleck;
using lib;

namespace Api.EventHandlers;

[ValidateDataAnnotations]
public class ClientWantsToSignIn : BaseEventHandler<ClientWantsToSignInDto>
{
    private readonly ICarControlService _carControlService;
    private readonly IWebSocketConnectionManager _webSocketConnectionManager;
    private readonly ILogger<ClientWantsToSignIn> _logger;

    public ClientWantsToSignIn(ICarControlService carControlService, IWebSocketConnectionManager webSocketConnectionManager, ILogger<ClientWantsToSignIn> logger)
    {
        _carControlService = carControlService;
        _webSocketConnectionManager = webSocketConnectionManager;
        _logger = logger;
    }

    public override async Task Handle(ClientWantsToSignInDto dto, IWebSocketConnection socket)
    {
        try
        {
            var metaData = _webSocketConnectionManager.GetConnection(socket.ConnectionInfo.Id);
            if (metaData == null)
            {
                _logger.LogWarning("Failed to sign in: missing connection metadata for client {ClientId}.", socket.ConnectionInfo.Id);

                socket.Send(JsonSerializer.Serialize(new ServerSendsErrorMessageToClientDto
                {
                    ErrorMessage = "Failed to sign in due to missing connection metadata."
                }));
                return;
            }

            _logger.LogInformation("Opening connection for client {ClientId}.", socket.ConnectionInfo.Id);
            await _carControlService.OpenConnection();

            metaData.Username = dto.NickName;
            socket.Send(JsonSerializer.Serialize(new ServerClientSignIn
            {
                Message = "You have connected as " + dto.NickName
            }));
            _logger.LogInformation("Client {ClientId} connected as {NickName}.", socket.ConnectionInfo.Id, dto.NickName);

            await _carControlService.AddUserAsync(socket.ConnectionInfo.Id, dto.NickName);
            _webSocketConnectionManager.StopDisconnectTimer(socket.ConnectionInfo.Id);
        }
        catch (AppException ex)
        {
            _logger.LogError(ex, "AppException occurred while signing in client {ClientId}.", socket.ConnectionInfo.Id);

            socket.Send(JsonSerializer.Serialize(new ServerSendsErrorMessageToClientDto
            {
                ErrorMessage = ex.Message
            }));
        }
        catch (Exception ex)
        {
            var errorMessage = "An unexpected error occurred. Please try again later.";

            _logger.LogError(ex, "Unexpected error occurred while signing in client {ClientId}.", socket.ConnectionInfo.Id);

            socket.Send(JsonSerializer.Serialize(new ServerSendsErrorMessageToClientDto
            {
                ErrorMessage = errorMessage
            }));
        }
    }
}
