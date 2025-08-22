using KufarPro.Shared.Models.Ads;
using KufarPro.Shared.Models.Settings;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Telegram.Bot;
using Telegram.Bot.Types;

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
            string message = GetNotifyMessage(ad);

            foreach (var userId in chatIds)
            {
                if (ad.Images.ToList().Count > 0)
                {
                    var media = ad.Images.Select((image, index) =>
                        new InputMediaPhoto(image) { Caption = index == 0 ? message : null }).ToArray();

                    await _botClient.SendMediaGroup(userId, media);
                }
                else
                {
                    await _botClient.SendMessage(userId, message);
                }
            }

            _logger.LogInformation($"{chatIds} user(s) have been notified about {ad.Subject} listed in {ad.ListTime:HH:mm dd.MM.yyyy}.");
        }

        public static string GetNotifyMessage(Ad ad)
        {
            string message = $"Вышло новое объявление в {ad.ListTime:HH:mm dd/MM/yyyy}!" +
                $"\n\nНазвание объявления: {ad.Subject}" +
                $"\nЦена: {ad.Price} {ad.Currency}" +
                $"\nЦена: {ad.Price} {ad.Currency}" +
                $"\nСсылка на объявление: {ad.Url}";

            return message;
        }
    }
}
