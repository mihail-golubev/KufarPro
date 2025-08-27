using KufarPro.Scanner.HttpClients.Interfaces;
using KufarPro.Scanner.Processors;
using KufarPro.Shared.Helpers;
using KufarPro.Shared.Mappers;
using KufarPro.Shared.Messaging.Interfaces;
using KufarPro.Shared.Messaging.Models;
using KufarPro.Shared.Models.Search;
using KufarPro.Shared.Models.Settings;
using Microsoft.Extensions.Options;

namespace KufarPro.Scanner.Services
{
    public class ScannerService : BackgroundService
    {
        private const int SyncCooldown = 60; // minutes

        private DateTime _lastSyncTime;
        private readonly List<SearchFilter> _searchFilters = new List<SearchFilter>();

        private readonly KufarProcessor _kufarProcessor;
        private readonly IGetSearchFiltersApiClient _searchFiltersApiClient;
        private readonly IMessageQueueService _messageQueueService;
        private readonly MessageQueueSettings _messageQueueSettings;
        private readonly ILogger<ScannerService> _logger;

        public ScannerService(KufarProcessor kufarProcessor,
            IGetSearchFiltersApiClient searchFiltersApiClient,
            IMessageQueueService messageQueueService,
            IOptions<MessageQueueSettings> messageQueueSettings,
            ILogger<ScannerService> logger)
        {
            _kufarProcessor = kufarProcessor;
            _messageQueueService = messageQueueService;
            _messageQueueSettings = messageQueueSettings.Value;
            _searchFiltersApiClient = searchFiltersApiClient;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    if ((DateTime.UtcNow - _lastSyncTime) > TimeSpan.FromMinutes(SyncCooldown))
                    {
                        await SyncFilters(stoppingToken);
                        _lastSyncTime = DateTime.UtcNow;
                    }

                    _logger.LogInformation($"There are {_searchFilters.Count} search filters in database. Looking for new ads..");

                    foreach (var searchFilter in _searchFilters)
                    {
                        var newAds = await _kufarProcessor.ScanForNewAds(searchFilter);

                        if (newAds.Count > 0)
                        {
                            var botType = AdTypeHelper.GetBotType(newAds.FirstOrDefault().Type);

                            var newAdsDto = new NewAdsQueueModel()
                            {
                                BotType = AdTypeHelper.GetBotType(newAds.FirstOrDefault().Type),
                                Ads = newAds.Select(ad => ad.MapToNewAd()).ToList(),
                                ChatIds = searchFilter.ChatIds
                            };

                            await _messageQueueService.PublishAsync(_messageQueueSettings.NewAdsQueueName, newAdsDto);
                        }
                    }

                    await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Some error occured: {ex.GetType()}::{ex.Message}\nContinue scanning..");
                }
            }
        }

        public override async Task StartAsync(CancellationToken cancellationToken)
        {
            await _messageQueueService.InitializeAsync();
            await _messageQueueService.ConsumeAsync<NewFilter>(_messageQueueSettings.NewFiltersQueueName, async filter =>
            {
                _searchFilters.AddOrUpdate(filter);
                _logger.LogInformation($"New filter added from queue: {filter.UrlQuery}");
                await Task.CompletedTask;
            });

            await base.StartAsync(cancellationToken);
        }

        private async Task SyncFilters(CancellationToken stoppingToken)
        {
            try
            {
                var filters = await _searchFiltersApiClient.GetAll();
                if (filters != null && filters.Any())
                {
                    _searchFilters.Clear();
                    _searchFilters.AddRange(filters);
                    _logger.LogInformation($"Filters sync... {_searchFilters.Count} filters received.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Could not retrieve filters from API.");
            }
        }
    }
}
