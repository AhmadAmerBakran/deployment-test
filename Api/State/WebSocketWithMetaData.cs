using Fleck;

namespace api.State;

public class WebSocketWithMetaData
{
    private readonly ILogger<WebSocketWithMetaData> _logger;

    public IWebSocketConnection Connection { get; set; }
    public string Username { get; set; }
    public Timer DisconnectTimer { get; set; }

    public WebSocketWithMetaData(IWebSocketConnection connection, ILogger<WebSocketWithMetaData> logger)
    {
        Connection = connection;
        Username = string.Empty;
        DisconnectTimer = null;
        _logger = logger;

    }

    public void StartDisconnectTimer(Action disconnectAction, int timeoutMilliseconds)
    {
        DisconnectTimer?.Dispose();
        DisconnectTimer = new Timer(_ => disconnectAction(), null, timeoutMilliseconds, Timeout.Infinite);
        _logger.LogInformation($"Stopped disconnect timer for connection: {Connection.ConnectionInfo.Id}");

    }

    public void StopDisconnectTimer()
    {
        DisconnectTimer?.Dispose();
        DisconnectTimer = null;
        _logger.LogInformation($"Stopped disconnect timer for connection: {Connection.ConnectionInfo.Id}");

    }
}