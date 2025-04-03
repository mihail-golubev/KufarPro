using AvKufarCarParser.DataAccess;
using AvKufarCarParser.Kufar;
using AvKufarCarParser.Models.Database;
using AvKufarCarParser.Models.Kufar;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace AvKufarCarParser
{
    public class BotService : BackgroundService
    {
        private List<SearchFilter> _searchFilters;

        private readonly ITelegramBotClient _botClient;
        private readonly KufarProcessor _kufarProcessor;
        private readonly DatabaseService _databaseService;
        private readonly ILogger<BotService> _logger;

        public BotService(ITelegramBotClient botClient, KufarProcessor kufarProcessor, DatabaseService databaseService, ILogger<BotService> logger)
        {
            _botClient = botClient;
            _kufarProcessor = kufarProcessor;
            _databaseService = databaseService;
            _logger = logger;

            _searchFilters = _databaseService.GetAllFiltersAsync().Result;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _botClient.StartReceiving(HandleUpdateAsync, HandleErrorAsync, cancellationToken: stoppingToken);
            _logger.LogInformation("AvKufarCarParser Bot 1.1.0 has been started.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    _logger.LogInformation($"There are {_searchFilters.Count} search filters in database. Looking for new ads..");

                    foreach (var searchFilter in _searchFilters)
                    {
                        var newAds = await _kufarProcessor.GetNewAds(searchFilter.FilterParameters);
                        await Task.WhenAll(newAds.Select(ad => NotifyUsers(ad, searchFilter.ChatIds)));
                    }

                    await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
                }

                catch (Exception ex)
                {
                    _logger.LogError($"Some error occured: {ex.GetType()}::{ex.Message}. Continue scanning..");
                }
            }
        }

        private async Task HandleUpdateAsync(ITelegramBotClient bot, Update update, CancellationToken token)
        {
            if (update.Type == UpdateType.Message && update.Message!.Text != null)
            {
                long chatId = update.Message.Chat.Id;
                string messageText = update.Message.Text.ToLower();

                if (messageText.StartsWith("/subscribe"))
                {
                    var parameters = ParseFilterParameters(messageText);
                    var result = await _databaseService.AddOrUpdateSubscriptionAsync(chatId, parameters);

                    if (result != null)
                    {
                        await bot.SendMessage(chatId, "Вы подписались на уведомления о новых объявлениях!", cancellationToken: token);
                        _searchFilters.Add(result);
                        _logger.LogInformation($"User {chatId} has subscribed.");
                    }
                    else
                    {
                        await bot.SendMessage(chatId, "Что-то пошло не так. Возможно, вы уже подписаны на уведомления по этому фильтру.", cancellationToken: token);
                        _logger.LogInformation($"User {chatId} has not been subscribed.");
                    }
                }
                else if (messageText.StartsWith("/unsubscribe"))
                {
                    var parameters = ParseFilterParameters(messageText);
                    var result = await _databaseService.RemoveSubscriptionAsync(chatId, parameters);

                    if (result)
                    {
                        await bot.SendMessage(chatId, "Вы отписались от уведомлений о новых объявлениях!", cancellationToken: token);
                        _logger.LogInformation($"User {chatId} has unsubscribed.");
                    }
                    else
                    {
                        await bot.SendMessage(chatId, "Что-то пошло не так. Возможно, вы не подписаны на уведомления по этому фильтру.", cancellationToken: token);
                        _logger.LogInformation($"User {chatId} has not been unsubscribed.");
                    }
                }
            }
        }

        private Task HandleErrorAsync(ITelegramBotClient bot, Exception exception, CancellationToken token)
        {
            _logger.LogError($"Error: {exception.Message}");
            return Task.CompletedTask;
        }

        private async Task NotifyUsers(Ad ad, List<long> chatIds)
        {
            string message = $"Вышло новое объявление в {ad.ListTime:HH:mm dd/MM/yyyy}!" +
                $"\n\nНазвание автомобиля: {ad.CarParams.Brand} {ad.CarParams.Model}" +
                $"\nЦена: ${ad.Price}" +
                $"\nОбласть: {ad.CarParams.Region}" +
                $"\nГород: {ad.CarParams.City}" +
                $"\nСсылка на объявление: {ad.Link}" +
                $"\n\nХарактеристики автомобиля:" +
                $"\nДвигатель: {ad.CarParams.FuelType} {ad.CarParams.EngineCapacity}" +
                $"\nПробег: {ad.CarParams.Mileage} км" +
                $"\nГод выпуска: {ad.CarParams.Year}" +
                $"\nКоробка передач: {ad.CarParams.GearboxType}" +
                $"\nТип кузова: {ad.CarParams.BodyType}" +
                $"\nПривод: {ad.CarParams.DriveType}";

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

            _logger.LogInformation($"{_searchFilters.Count} user(s) have been notified about {ad.CarParams.Brand} {ad.CarParams.Model} listed in {ad.ListTime:HH:mm dd.MM.yyyy}.");
        }

        private List<FilterParameter> ParseFilterParameters(string messageText)
        {
            var parameters = new List<FilterParameter>();
            var parts = messageText.Split(' ').Skip(1);

            foreach (var part in parts)
            {
                var kv = part.Split('=');
                if (kv.Length == 2)
                {
                    parameters.Add(new FilterParameter { QueryName = kv[0], Value = kv[1] });
                }
            }

            return parameters;
        }
    }
}
