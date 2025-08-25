using KufarPro.Shared.Messaging.Interfaces;

namespace KufarPro.Api.Messaging
{
    public class MessageQueueInitializer : IHostedService
    {
        private readonly IMessageQueueService _messageQueueService;
        private readonly ILogger<MessageQueueInitializer> _logger;

        public MessageQueueInitializer(IMessageQueueService messageQueueService, ILogger<MessageQueueInitializer> logger)
        {
            _messageQueueService = messageQueueService;
            _logger = logger;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            try
            {
                await _messageQueueService.InitializeAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "RabbitMQ initialization failed.");
                throw;
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("MessageQueueInitializer stopped.");
            return Task.CompletedTask;
        }
    }
}
