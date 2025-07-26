using KufarPro.DataAccess;
using KufarPro.Helpers;
using KufarPro.Models.Database;
using KufarPro.Models.Kufar.API;
using KufarPro.Models.Kufar.HelperModels;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using KufarPro.Models.Settings;
using Microsoft.Extensions.Options;
using KufarPro.Processors;
using KufarPro.Handlers;

namespace KufarPro
{
    public class BotService : BackgroundService
    {
        private readonly List<SearchFilter> _searchFilters;
        private readonly Dictionary<long, UserFilterState> _userFilterStates = new();

        private readonly ITelegramBotClient _botClient;
        private readonly KufarProcessor _kufarProcessor;
        private readonly IDbSubscriptionService _dbService;
        private readonly BotSettings _botSettings;
        private readonly ILogger<BotService> _logger;
        private readonly FilterMessageHandler _filterMessageHandler;

        public BotService(
            ITelegramBotClient botClient,
            KufarProcessor kufarProcessor,
            IDbSubscriptionService dbService,
            IOptions<BotSettings> botOptions,
            ILogger<BotService> logger)
        {
            _botClient = botClient;
            _kufarProcessor = kufarProcessor;
            _dbService = dbService;
            _botSettings = botOptions.Value;
            _logger = logger;
            _searchFilters = _dbService.GetAllFiltersAsync().Result;
            _filterMessageHandler = new FilterMessageHandler(_userFilterStates, _dbService, _searchFilters, _botClient, _logger);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _botClient.StartReceiving(HandleUpdateAsync, HandleErrorAsync, cancellationToken: stoppingToken);
            _logger.LogInformation($"{_botSettings.BotType} KufarPro Bot 2.1.0 has been started.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    _logger.LogInformation($"There are {_searchFilters.Count} search filters in database. Looking for new ads..");

                    foreach (var searchFilter in _searchFilters)
                    {
                        var newAds = await _kufarProcessor.ScanForNewAds(searchFilter);
                        await Task.WhenAll(newAds.Select(ad => NotifyUsers(ad, searchFilter.ChatIds)));
                    }

                    await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Some error occured: {ex.GetType()}::{ex.Message}\nContinue scanning..");
                }
            }
        }

        private async Task HandleUpdateAsync(ITelegramBotClient bot, Update update, CancellationToken token)
        {
            if (update.Type == UpdateType.Message && update.Message?.Text != null)
            {
                await _filterMessageHandler.HandleMessageAsync(update.Message, token);
            }
            else if (update.Type == UpdateType.CallbackQuery && update.CallbackQuery != null)
            {
                await _filterMessageHandler.HandleCallbackQueryAsync(update.CallbackQuery, token);
            }
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
