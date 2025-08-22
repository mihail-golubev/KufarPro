using KufarPro.Bot.Helpers;
using KufarPro.Bot.Models.Kufar.API;
using KufarPro.Bot.Models.Settings;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace KufarPro.Bot
{
    public class BotService : BackgroundService
    {
        private readonly ITelegramBotClient _botClient;
        private readonly BotSettings _botSettings;
        private readonly ILogger<BotService> _logger;

        public BotService(
            ITelegramBotClient botClient,
            IOptions<BotSettings> botOptions,
            ILogger<BotService> logger)
        {
            _botClient = botClient;
            _botSettings = botOptions.Value;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _botClient.StartReceiving(HandleUpdateAsync, HandleErrorAsync, cancellationToken: stoppingToken);
            _logger.LogInformation($"{_botSettings.BotType} KufarPro Bot 3.0.0 has been started.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Some error occured: {ex.GetType()}::{ex.Message}\nContinue scanning..");
                }
            }
        }

        private async Task HandleUpdateAsync(ITelegramBotClient bot, Update update, CancellationToken token)
        {
        }

        private Task HandleErrorAsync(ITelegramBotClient bot, Exception exception, CancellationToken token)
        {
            _logger.LogError($"Exception type: {exception.GetType().Name}. Error message: {exception.Message}");
            return Task.CompletedTask;
        }

        private async Task NotifyUsers(Ad ad, List<long> chatIds)
        {
            string message = ad switch
            {
                AutoAd autoAd => AppHelper.GetNotifyMessage(autoAd),
                BicycleAd bicycleAd => AppHelper.GetNotifyMessage(bicycleAd),
                _ => AppHelper.GetNotifyMessage(ad)
            };

            foreach (var userId in chatIds)
            {
                if (ad.Images.Count > 0)
                {
                    var media = ad.Images.Select((img, index) =>
                        new InputMediaPhoto(img.Link) { Caption = index == 0 ? message : null }).ToArray();

                    await _botClient.SendMediaGroup(userId, media);
                }
                else
                {
                    await _botClient.SendMessage(userId, message);
                }
            }

            _logger.LogInformation($"{_searchFilters.Count} user(s) have been notified about {ad.Subject} listed in {ad.ListTime:HH:mm dd.MM.yyyy}.");
        }
    }
}
