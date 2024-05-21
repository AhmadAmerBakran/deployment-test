using Api.Dtos;
using Fleck;
using lib;

namespace Api.EventHandlers;

public class ErrorMessageHandler : BaseEventHandler<ServerSendsErrorMessageToClientDto>
{
    public override async Task Handle(ServerSendsErrorMessageToClientDto dto, IWebSocketConnection socket)
    {
        await socket.Send($"Error: {dto.ErrorMessage}");
        await Task.CompletedTask;
    }
}