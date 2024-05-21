using System.Text.Json;
using Api.Dtos;
using Api.Filters;
using Core.Exceptions;
using Core.Interfaces;
using Fleck;
using lib;

namespace Api.EventHandlers;

[RequireAuthentication]
public class ClientWantsToControlCar : BaseEventHandler<ClientWantsToControlCarDto>
{
    private readonly ICarControlService _carControlService;
    private readonly ILogger<ClientWantsToControlCar> _logger;

    public ClientWantsToControlCar(ICarControlService carControlService, ILogger<ClientWantsToControlCar> logger)
    {
        _carControlService = carControlService;
        _logger = logger;
    }

    public override async Task Handle(ClientWantsToControlCarDto dto, IWebSocketConnection socket)
    {
        try
        {
            _logger.LogInformation("Client {ClientId} requested car control with command {Command} on topic {Topic}.", socket.ConnectionInfo.Id, dto.Command, dto.Topic);

            await _carControlService.CarControl(socket.ConnectionInfo.Id, dto.Topic, dto.Command);
            var successMessage = $"Command '{dto.Command}' sent to topic '{dto.Topic}'.";
            _logger.LogInformation(successMessage);

            await socket.Send(successMessage);
        }
        catch (AppException ex)
        {
            var errorMessage = ex.Message;
            _logger.LogError(ex, "An application error occurred while processing the car control request for client {ClientId}.", socket.ConnectionInfo.Id);

            socket.Send(JsonSerializer.Serialize(new ServerSendsErrorMessageToClientDto
            {
                ErrorMessage = errorMessage
            }));
        }
        catch (Exception ex)
        {
            var errorMessage = "An unexpected error occurred. Please try again later.";
            _logger.LogError(ex, errorMessage);

            socket.Send(JsonSerializer.Serialize(new ServerSendsErrorMessageToClientDto
            {
                ErrorMessage = errorMessage
            }));
        }
    }
}
