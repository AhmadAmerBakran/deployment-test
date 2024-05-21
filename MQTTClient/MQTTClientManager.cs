using System.Collections.Concurrent;
using System.Security.Authentication;
using System.Text;
using Core.Exceptions;
using Core.Interfaces;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Exceptions;
using MQTTnet.Formatter;
using MQTTnet.Protocol;
using Microsoft.Extensions.Logging;

namespace MQTTClient
{
    public class MQTTClientManager : IMQTTClientManager
    {
        private readonly ILogger<MQTTClientManager> _logger;
        private IMqttClient _client;
        private MqttFactory _factory;
        private readonly ConcurrentDictionary<string, (string message, Timer timer)> _debounceTimers;
        private const int ReconnectDelay = 5000; // Reconnect delay in milliseconds
        private const int MaxReconnectAttempts = 10; // Max number of reconnect attempts
        private const int DebounceInterval = 500;
        private int _reconnectAttempts = 0;
        public event Action<string, string> MessageReceived;

        public MQTTClientManager(ILogger<MQTTClientManager> logger)
        {
            _logger = logger;
            _factory = new MqttFactory();
            _client = _factory.CreateMqttClient();
            _debounceTimers = new ConcurrentDictionary<string, (string, Timer)>();
            _client.DisconnectedAsync += OnDisconnectedAsync;
        }

        public void InitializeSubscriptions()
        {
            _client.ApplicationMessageReceivedAsync += HandleApplicationMessageReceivedAsync;
            try
            {
                SubscribeAsync("car/notifications").Wait();
            }
            catch (AggregateException ex)
            {
                var innerEx = ex.InnerException;
                if (innerEx is MqttCommunicationException || innerEx is TimeoutException || innerEx is AuthenticationException || innerEx is Exception)
                {
                    _logger.LogError(innerEx, "A MQTT subscription error occurred.");
                    throw new AppException("A MQTT subscription error occurred. Please try again later.");
                }
            }
        }

        private async Task HandleApplicationMessageReceivedAsync(MqttApplicationMessageReceivedEventArgs e)
        {
            var topic = e.ApplicationMessage.Topic;
            var message = Encoding.UTF8.GetString(e.ApplicationMessage.PayloadSegment);
            _logger.LogInformation("Received on {Topic}: {Message}", topic, message);

            if (_debounceTimers.ContainsKey(topic))
            {
                // Reset the existing timer
                _debounceTimers[topic].timer.Change(DebounceInterval, Timeout.Infinite);
                _debounceTimers[topic] = (message, _debounceTimers[topic].timer);
            }
            else
            {
                // Create a new timer
                var timer = new Timer(DebounceHandler, topic, DebounceInterval, Timeout.Infinite);
                _debounceTimers[topic] = (message, timer);
            }
            await Task.CompletedTask;
        }

        private void DebounceHandler(object state)
        {
            var topic = (string)state;
            if (_debounceTimers.TryRemove(topic, out var value))
            {
                var message = value.message;
                _logger.LogInformation("Debounced message on {Topic}: {Message}", topic, message);
                MessageReceived?.Invoke(topic, message);
            }
        }

        public async Task ConnectAsync()
        {
            if (_client.IsConnected)
            {
                _logger.LogInformation("Already connected to MQTT Broker.");
                return;
            }
            var tlsOptions = new MqttClientOptionsBuilderTlsParameters
            {
                UseTls = true,
                AllowUntrustedCertificates = true,
                IgnoreCertificateChainErrors = true,
                IgnoreCertificateRevocationErrors = true,
                SslProtocol = SslProtocols.Tls12
            };

            var mqttClientOptions = new MqttClientOptionsBuilder()
                .WithClientId(MQTTConfig.ClientId)
                .WithTcpServer(MQTTConfig.Server, MQTTConfig.Port)
                .WithCredentials(MQTTConfig.Username, MQTTConfig.Password)
                .WithCleanSession()
                .WithTls(tlsOptions)
                .WithProtocolVersion(MqttProtocolVersion.V500)
                .WithKeepAlivePeriod(TimeSpan.FromSeconds(60)) // Set the keep-alive period
                .Build();

            try
            {
                await _client.ConnectAsync(mqttClientOptions, CancellationToken.None);
                _logger.LogInformation("Connected to MQTT Broker.");
                _reconnectAttempts = 0; // Reset reconnect attempts after successful connection

            }
            catch (MqttCommunicationException ex)
            {
                _logger.LogError(ex, "An error occurred while connecting to the MQTT broker.");
                throw new AppException("An error occurred while connecting to the MQTT broker. Please try again later.");
            }
            catch (AuthenticationException ex)
            {
                _logger.LogError(ex, "Authentication failed while connecting to the MQTT broker.");
                throw new AppException("Authentication failed while connecting to the MQTT broker. Please check your credentials.");
            }
            catch (TimeoutException ex)
            {
                _logger.LogError(ex, "The connection to the MQTT broker timed out.");
                throw new AppException("The connection to the MQTT broker timed out. Please try again later.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred while connecting to the MQTT broker.");
                throw new AppException("An unexpected error occurred while connecting to the MQTT broker. Please try again later.");
            }
        }
        
        private async Task OnDisconnectedAsync(MqttClientDisconnectedEventArgs e)
        {
            _logger.LogWarning("Disconnected from MQTT Broker. Reason: {Reason}", e.Reason);

            if (e.Exception != null)
            {
                _logger.LogError(e.Exception, "An exception occurred while disconnecting from the MQTT broker.");
            }

            if (_reconnectAttempts < MaxReconnectAttempts)
            {
                _reconnectAttempts++;
                _logger.LogInformation("Attempting to reconnect to MQTT Broker (Attempt {ReconnectAttempt}/{MaxReconnectAttempts})...", _reconnectAttempts, MaxReconnectAttempts);
                await Task.Delay(ReconnectDelay);
                await ConnectAsync();
            }
            else
            {
                _logger.LogError("Max reconnect attempts reached. Could not reconnect to MQTT Broker.");
            }
        }

        public async Task DisconnectAsync()
        {
            try
            {
                await _client.DisconnectAsync();
                _logger.LogInformation("Disconnected from MQTT Broker.");
            }
            catch (MqttCommunicationException ex)
            {
                _logger.LogError(ex, "An error occurred while disconnecting from the MQTT broker.");
                throw new AppException("An error occurred while disconnecting from the MQTT broker. Please try again later.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred while disconnecting from the MQTT broker.");
                throw new AppException("An unexpected error occurred while disconnecting from the MQTT broker. Please try again later.");
            }
        }

        public async Task PublishAsync(string topic, string message)
        {
            var mqttMessage = new MqttApplicationMessageBuilder()
                .WithTopic(topic)
                .WithPayload(Encoding.UTF8.GetBytes(message))
                .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtLeastOnce)
                .WithRetainFlag()
                .Build();

            try
            {
                await _client.PublishAsync(mqttMessage);
                _logger.LogInformation("Published to {Topic}: {Message}", topic, message);
            }
            catch (MqttCommunicationException ex)
            {
                _logger.LogError(ex, "An error occurred while publishing to the MQTT broker.");
                throw new AppException("An error occurred while publishing to the MQTT broker. Please try again later.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred while publishing to the MQTT broker.");
                throw new AppException("An unexpected error occurred while publishing to the MQTT broker. Please try again later.");
            }
        }

        public async Task SubscribeAsync(string topic)
        {
            var subscribeOptions = new MqttClientSubscribeOptionsBuilder()
                .WithTopicFilter(f =>
                    f.WithTopic(topic)
                        .WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce))
                .Build();

            try
            {
                await _client.SubscribeAsync(subscribeOptions);
                _logger.LogInformation("Subscribed to {Topic}", topic);
            }
            catch (MqttCommunicationException ex)
            {
                _logger.LogError(ex, "An error occurred while subscribing to the MQTT broker.");
                throw new AppException("An error occurred while subscribing to the MQTT broker. Please try again later.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred while subscribing to the MQTT broker.");
                throw new AppException("An unexpected error occurred while subscribing to the MQTT broker. Please try again later.");
            }
        }
    }
}
