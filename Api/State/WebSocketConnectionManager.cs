using api.State;
using Core.Exceptions;
using Core.Interfaces;
using Fleck;

namespace Api.State;

public class WebSocketConnectionManager : IWebSocketConnectionManager
{
    private readonly Dictionary<Guid, WebSocketWithMetaData> _connections = new();
    private readonly ILogger<WebSocketConnectionManager> _logger;
    private readonly ILoggerFactory _loggerFactory;
    private readonly ICarControlService _carControlService;

    
    public WebSocketConnectionManager(ILogger<WebSocketConnectionManager> logger, ILoggerFactory loggerFactory, ICarControlService carControlService)
    {
        _logger = logger;
        _loggerFactory = loggerFactory;
        _carControlService = carControlService;

    }

    public void AddConnection(Guid id, IWebSocketConnection socket)
    {
        try
        {
            var logger = _loggerFactory.CreateLogger<WebSocketWithMetaData>();

            if (!_connections.ContainsKey(id))
            {
                _connections[id] = new WebSocketWithMetaData(socket, logger);
                _logger.LogInformation($"New connection added with GUID: {id}");
            }
            else
            {
                _logger.LogInformation($"Connection with GUID: {id} already exists. Updating socket reference.");
                _connections[id].Connection = socket;
            }
        }
        catch (Exception ex)
        {
            throw new AppException("An error occurred while establishing the connection. Please try again later.");
        }
    }

    public void RemoveConnection(Guid id)
    {
        try
        {
            if (_connections.ContainsKey(id))
            {
                _connections[id].Connection.Close();
                _connections.Remove(id);
                _logger.LogInformation($"Connection and associated metadata removed: {id}");
            }
            else
            {
                _logger.LogWarning($"Attempted to remove non-existent connection with GUID: {id}");
            }
        }
        catch (Exception ex)
        {
            throw new AppException("An error occurred while disconnecting. Please try again later or close the app.");
        }
    }
    
    public WebSocketWithMetaData GetConnection(Guid id)
    {
        try
        {
            _connections.TryGetValue(id, out var metaData);
            return metaData;
        }
        catch (Exception ex)
        {
            throw new AppException("An error occurred while retrieving a connection. Please try again later.");
        }
    }

    public IEnumerable<WebSocketWithMetaData> GetAllConnections()
    {
        try
        {
            return _connections.Values;
        }
        catch (Exception ex)
        {
            throw new AppException("An error occurred while retrieving all connections. Please try again later.");
        }
    }
    
    public bool IsAuthenticated(IWebSocketConnection socket)
    {
        try
        {
            if (_connections.TryGetValue(socket.ConnectionInfo.Id, out WebSocketWithMetaData metaData))
            {
                return !string.IsNullOrEmpty(metaData.Username);
            }
            return false;
        }
        catch (Exception ex)
        {
            throw new AppException("An error occurred while checking authentication. Please try again later.");
        }
    }
    
    public bool HasMetadata(Guid id)
    {
        try
        {
            return _connections.ContainsKey(id);
        }
        catch (Exception ex)
        {
            throw new AppException("An error occurred while checking metadata. Please try again later.");
        }
    }
    
    public void ResetConnection(Guid id)
    {
        if (_connections.ContainsKey(id))
        {
            _connections[id].Username = null; // Reset the username or other metadata
            _connections[id].StopDisconnectTimer();
            _logger.LogInformation($"Connection metadata reset for GUID: {id}");
        }
        else
        {
            _logger.LogWarning($"Attempted to reset metadata for non-existent connection with GUID: {id}");
        }
    }
    
    public void StartDisconnectTimer(Guid id, Action disconnectAction, int timeoutMilliseconds)
    {
        if (_connections.ContainsKey(id))
        {
            _connections[id].StartDisconnectTimer(disconnectAction, timeoutMilliseconds);
        }
    }
    
    public void StopDisconnectTimer(Guid id)
    {
        if (_connections.ContainsKey(id))
        {
            _connections[id].StopDisconnectTimer();
        }
    }
    
    public async Task ResetCarStateToDefault(Guid connectionId)
    {
        try
        {
            await _carControlService.CarControl(connectionId, "cam/flash", "off");
            await _carControlService.CarControl(connectionId, "car/led/control", "off");
            await _carControlService.CarControl(connectionId, "car/control", "manual mode");
            await _carControlService.CarControl(connectionId, "cam/control", "stop");
            _logger.LogInformation($"Car state reset to default for connection: {connectionId}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to reset car state for connection: {connectionId}");
        }
    }

}
