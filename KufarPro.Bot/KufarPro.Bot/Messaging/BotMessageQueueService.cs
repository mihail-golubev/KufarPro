using KufarPro.Shared.Messaging;
using KufarPro.Shared.Models.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace KufarPro.Bot.Messaging
{
    public class BotMessageQueueService : MessageQueueService
    {
        public BotMessageQueueService(IOptions<MessageQueueSettings> messageQueueOptions, ILogger<BotMessageQueueService> logger) : base(messageQueueOptions, logger)
        {
        }

        public override async Task InitializeAsync()
        {
            _connection = await _factory.CreateConnectionAsync();
            _channel = await _connection.CreateChannelAsync();

            await _channel.QueueDeclareAsync(_settings.NewAdsQueueName, durable: true, exclusive: false, autoDelete: false);

            _logger.LogInformation($"RabbitMQ connected. Queue: {_settings.NewAdsQueueName}");
        }
    }
}
