using KufarPro.Shared.Messaging;
using KufarPro.Shared.Models.Settings;
using Microsoft.Extensions.Options;

namespace KufarPro.Api.Messaging
{
    public class FiltersApiMessageQueueService : MessageQueueService
    {
        public FiltersApiMessageQueueService(IOptions<MessageQueueSettings> options, ILogger<MessageQueueService> logger) : base(options, logger)
        {
        }

        public override async Task InitializeAsync()
        {
            _connection = await _factory.CreateConnectionAsync();
            _channel = await _connection.CreateChannelAsync();

            await _channel.QueueDeclareAsync(_settings.NewFiltersQueueName, durable: true, exclusive: false, autoDelete: false);

            _logger.LogInformation($"RabbitMQ connected. Queue: {_settings.NewFiltersQueueName}");
        }
    }
}
