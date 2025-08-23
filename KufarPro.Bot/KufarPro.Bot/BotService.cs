using KufarPro.Shared.Messaging.Interfaces;
using KufarPro.Shared.Models.DTOs;
using KufarPro.Shared.Models.HelperModels;
using KufarPro.Shared.Models.Settings;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;

namespace KufarPro.Bot
{
    public class BotService : BackgroundService
    {
        private readonly ITelegramBotClient _botClient;
        private readonly BotSettings _botSettings;
        private readonly IMessageQueueService _messageQueueService;
        private readonly MessageQueueSettings _messageQueueSettings;
        private readonly ILogger<BotService> _logger;

        public BotService(
            ITelegramBotClient botClient,
            IOptions<BotSettings> botOptions,
            IMessageQueueService messageQueueService,
            IOptions<MessageQueueSettings> messageQueueOptions,
            ILogger<BotService> logger)
        {
            _botClient = botClient;
            _botSettings = botOptions.Value;
            _messageQueueService = messageQueueService;
            _messageQueueSettings = messageQueueOptions.Value;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await _messageQueueService.InitializeAsync();

            _botClient.StartReceiving(HandleUpdateAsync, HandleErrorAsync, cancellationToken: stoppingToken);
            _logger.LogInformation($"{_botSettings.BotType} KufarPro Bot 3.0.0 has been started.");

            await _messageQueueService.ConsumeAsync<NewAdsQueueModel>(_messageQueueSettings.NewAdsQueueName, async message =>
            {
                try
                {
                    if (message.BotType != _botSettings.BotType)
                    {
                        return;
                    }

                    await NotifyUsers(message.Ads, message.ChatIds);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error while processing NewAdsQueueModel.");
                }
            });
        }

        private async Task HandleUpdateAsync(ITelegramBotClient bot, Update update, CancellationToken token)
        {
            if (update.Message.Text.StartsWith("/status", StringComparison.CurrentCultureIgnoreCase))
            {
                var message = $"{_botSettings.BotType} Kufar Pro запущен и ожидает новых объявлений.";
                await _botClient.SendMessage(update.Message.Chat.Id, message, cancellationToken: token);
            }
        }

        private Task HandleErrorAsync(ITelegramBotClient bot, Exception exception, CancellationToken token)
        {
            _logger.LogError($"Exception type: {exception.GetType().Name}. Error message: {exception.Message}");
            return Task.CompletedTask;
        }

        private async Task NotifyUsers(IEnumerable<NewAd> ads, List<long> chatIds)
        {
            foreach (var ad in ads)
            {
                InputMediaPhoto[] media = null;
                string message = GetNotifyMessage(ad);

                if (ad.Images.Count > 0)
                {
                    media = ad.Images.Select((image, index) => new InputMediaPhoto(image) { Caption = index == 0 ? message : null }).ToArray();
                }

                foreach (var chatId in chatIds)
                {
                    try
                    {
                        if (media == null)
                        {
                            await _botClient.SendMessage(chatId, message);
                        }
                        else
                        {
                            await _botClient.SendMediaGroup(chatId, media);
                        }

                        _logger.LogInformation($"{chatIds.Count} user(s) have been notified about {ad.Subject} listed in {ad.ListTime:HH:mm dd.MM.yyyy}.");
                    }
                    catch (ApiRequestException ex)
                    {
                        _logger.LogError(ex, $"Something went wrong with sending message to Telegram.");
                    }
                }
            }
        }

        private static string GetNotifyMessage(NewAd ad)
        {
            string message = $"Вышло новое объявление в {ad.ListTime:HH:mm dd/MM/yyyy}!" +
                $"\n\nНазвание объявления: {ad.Subject}" +
                $"\nРазместил: {ad.Author.Name} [id:{ad.Author.Id}]" +
                $"\nЕсть ли телефон: {GetBooleanAsString(!ad.IsPhoneHidden)}\n\n";

            ad.Parameters.ForEach(param => message += $"{param}\n");

            message += $"\nМестонахождение: {ad.City}, {ad.Region}" +
                $"\nЦена: {ad.Price} {ad.Currency}" +
                $"\nСсылка на объявление: {ad.Url}";

            return message;
        }
         
        private static string GetBooleanAsString(bool value)
        {
            return value ? "Да" : "Нет";
        }
    }
}
