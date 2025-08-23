using KufarPro.Shared.Messaging.Interfaces;
using KufarPro.Shared.Models.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace KufarPro.Shared.Messaging
{
    public class MessageQueueService : IMessageQueueService, IAsyncDisposable
    {
        protected readonly MessageQueueSettings _settings;
        protected readonly ILogger<MessageQueueService> _logger;

        protected IConnection _connection;
        protected IChannel _channel;
        protected IConnectionFactory _factory;

        public MessageQueueService(IOptions<MessageQueueSettings> options, ILogger<MessageQueueService> logger)
        {
            _settings = options.Value;
            _logger = logger;

            var password = Environment.GetEnvironmentVariable(_settings.PasswordEnvVariableName);
            if (string.IsNullOrEmpty(password))
            {
                throw new InvalidOperationException($"Environment variable '{_settings.PasswordEnvVariableName}' is not set.");
            }

            _factory = new ConnectionFactory
            {
                HostName = _settings.HostName,
                Port = _settings.Port,
                UserName = _settings.Username,
                Password = password
            };
        }

        public virtual async Task InitializeAsync()
        {
            _connection = await _factory.CreateConnectionAsync();
            _channel = await _connection.CreateChannelAsync();

            await _channel.QueueDeclareAsync(_settings.NewFiltersQueueName, durable: true, exclusive: false, autoDelete: false);
            await _channel.QueueDeclareAsync(_settings.NewAdsQueueName, durable: true, exclusive: false, autoDelete: false);

            _logger.LogInformation($"RabbitMQ connected. Queues: {_settings.NewFiltersQueueName}, {_settings.NewAdsQueueName}");
        }

        public async Task PublishAsync<T>(string queueName, T message)
        {
            var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));
            await _channel.BasicPublishAsync(exchange: string.Empty, routingKey: queueName, mandatory: false, body: body);
            _logger.LogInformation($"Message published to {queueName}");
        }

        public async Task ConsumeAsync<T>(string queueName, Func<T, Task> handler)
        {
            var consumer = new AsyncEventingBasicConsumer(_channel);
            consumer.ReceivedAsync += async (model, ea) =>
            {
                var json = Encoding.UTF8.GetString(ea.Body.ToArray());
                try
                {
                    var obj = JsonSerializer.Deserialize<T>(json);
                    if (obj != null)
                    {
                        await handler(obj);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error processing message from {queueName}");
                }
            };

            await _channel.BasicConsumeAsync(queue: queueName, autoAck: true, consumer: consumer);
        }

        public async ValueTask DisposeAsync()
        {
            if (_channel != null) await _channel.CloseAsync();
            if (_connection != null) await _connection.CloseAsync();
        }
    }
}
