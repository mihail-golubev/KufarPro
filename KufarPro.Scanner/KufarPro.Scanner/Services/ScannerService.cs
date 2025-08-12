using KufarPro.Scanner.Helpers;
using KufarPro.Scanner.HttpClients.Interfaces;
using KufarPro.Scanner.Processors;
using KufarPro.Scanner.Services.Interfaces;
using KufarPro.Shared.Models.HelperModels;
using KufarPro.Shared.Models.Search;
using KufarPro.Shared.Models.Settings;
using Microsoft.Extensions.Options;

namespace KufarPro.Scanner.Services
{
    public class ScannerService : BackgroundService
    {
        private readonly List<SearchFilter> _searchFilters;

        private readonly KufarProcessor _kufarProcessor;
        private readonly IGetSearchFiltersApiClient _searchFiltersApiClient;
        private readonly IMessageQueueService _messageQueueService;
        private readonly ILogger<ScannerService> _logger;
        private readonly MessageQueueSettings _messageQueueSettings;

        public ScannerService(KufarProcessor kufarProcessor,
            IGetSearchFiltersApiClient searchFiltersApiClient,
            IMessageQueueService messageQueueService,
            IOptions<MessageQueueSettings> messageQueueSettings,
            ILogger<ScannerService> logger)
        {
            _kufarProcessor = kufarProcessor;
            _searchFilters = searchFiltersApiClient.GetAll().Result.ToList();
            _messageQueueService = messageQueueService;
            _messageQueueSettings = messageQueueSettings.Value;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    _logger.LogInformation($"There are {_searchFilters.Count} search filters in database. Looking for new ads..");

                    foreach (var searchFilter in _searchFilters)
                    {
                        BotType? botType = null;
                        var newAds = await _kufarProcessor.ScanForNewAds(searchFilter);

                        foreach (var ad in newAds)
                        {
                            botType ??= AppHelper.GetBotType(searchFilter.UrlQuery);

                            var newAd = new AdQueueModel()
                            {
                                BotType = AppHelper.GetBotType(searchFilter.UrlQuery),
                                Ad = ad
                            };

                            await _messageQueueService.PublishAsync(_messageQueueSettings.NewAdsQueueName, newAd);
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
            await _messageQueueService.ConsumeAsync<SearchFilter>(_messageQueueSettings.NewFiltersQueueName, async filter =>
            {
                _searchFilters.Add(filter);
                _logger.LogInformation($"New filter added from queue: {filter.UrlQuery}");
                await Task.CompletedTask;
            });

            await base.StartAsync(cancellationToken);
        }
    }
}
