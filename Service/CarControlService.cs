using System.Collections;
using Core.Exceptions;
using Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace Service;

public class CarControlService : ICarControlService
{
    private readonly IMQTTClientManager _mqttClientManager;
    private readonly ICarLogRepository _carLogRepository;
    private readonly ILogger<CarControlService> _logger;
    private bool _isSubscribed = false;
    private Guid _guid;

    public CarControlService(IMQTTClientManager mqttClientManager, ICarLogRepository carLogRepository, ILogger<CarControlService> logger)
    {
        _mqttClientManager = mqttClientManager;
        _carLogRepository = carLogRepository;
        _logger = logger;
    }

    public void HandleReceivedMessage(string topic, string message)
    {
        OnNotificationReceived?.Invoke(topic, message);
        try
        {
            _carLogRepository.AddNotificationAsync(_guid, topic, null, message).Wait();
            _logger.LogInformation("Handled received message on topic '{Topic}': {Message}", topic, message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while handling the received message.");
            throw new AppException("An error occurred while handling the received message. Please try again later.");
        }
    }

    public async Task CarControl(Guid userId, string topic, string command)
    {
        try
        {
            await _mqttClientManager.PublishAsync(topic, command);
            await _carLogRepository.AddNotificationAsync(userId, null, topic, command);
            _guid = userId;
            _logger.LogInformation("Sent command '{Command}' to topic '{Topic}' for user '{UserId}'", command, topic, userId);
        }
        catch (AppException ex)
        {
            _logger.LogError(ex, "AppException occurred while controlling the car.");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unexpected error occurred while controlling the car.");
            throw new AppException("An unexpected error occurred while controlling the car. Please try again later.");
        }
    }

    public async Task OpenConnection()
    {
        try
        {
            await _mqttClientManager.ConnectAsync();
            _mqttClientManager.InitializeSubscriptions();
            if (!_isSubscribed)
            {
                _mqttClientManager.MessageReceived += HandleReceivedMessage;
                _isSubscribed = true;
            }
            _logger.LogInformation("Opened connection and initialized subscriptions.");
        }
        catch (AppException ex)
        {
            _logger.LogError(ex, "AppException occurred while opening the connection.");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unexpected error occurred while opening the connection.");
            throw new AppException("An unexpected error occurred while opening the connection. Please try again later.");
        }
    }

    public async Task CloseConnection()
    {
        try
        {
            if (_isSubscribed)
            {
                _mqttClientManager.MessageReceived -= HandleReceivedMessage;
                _isSubscribed = false;
            }
            await _mqttClientManager.DisconnectAsync();
            _logger.LogInformation("Closed connection.");
        }
        catch (AppException ex)
        {
            _logger.LogError(ex, "AppException occurred while closing the connection.");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unexpected error occurred while closing the connection.");
            throw new AppException("An unexpected error occurred while closing the connection. Please try again later.");
        }
    }

    public async Task AddUserAsync(Guid userId, string nickname)
    {
        try
        {
            await _carLogRepository.AddUserAsync(userId, nickname);
            _logger.LogInformation("Added user '{UserId}' with nickname '{Nickname}'", userId, nickname);
        }
        catch (AppException ex)
        {
            _logger.LogError(ex, "AppException occurred while adding the user.");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unexpected error occurred while adding the user.");
            throw new AppException("An unexpected error occurred while adding the user. Please try again later.");
        }
    }

    public event Action<string, string> OnNotificationReceived;

    public async Task<IEnumerable> GetCarLog()
    {
        try
        {
            var carLog = await _carLogRepository.GetCarLog();
            _logger.LogInformation("Fetched car log.");
            return carLog;
        }
        catch (AppException ex)
        {
            _logger.LogError(ex, "AppException occurred while fetching the car log.");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unexpected error occurred while fetching the car log.");
            throw new AppException("An unexpected error occurred while fetching the car log. Please try again later.");
        }
    }
}
