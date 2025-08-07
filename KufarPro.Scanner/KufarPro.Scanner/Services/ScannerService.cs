using KufarPro.Scanner.HttpClients.Interfaces;
using KufarPro.Scanner.Processors;
using KufarPro.Shared.Models.Search;

namespace KufarPro.Scanner.Services
{
    public class ScannerService : BackgroundService
    {
        private readonly List<SearchFilter> _searchFilters;

        private readonly KufarProcessor _kufarProcessor;
        private readonly IGetSearchFiltersApiClient _searchFiltersApiClient;
        private readonly ILogger<ScannerService> _logger;

        public ScannerService(KufarProcessor kufarProcessor, IGetSearchFiltersApiClient searchFiltersApiClient, ILogger<ScannerService> logger)
        {
            _kufarProcessor = kufarProcessor;
            _searchFilters = searchFiltersApiClient.GetAll().Result.ToList();
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
                        var newAds = await _kufarProcessor.ScanForNewAds(searchFilter);
                        //await Task.WhenAll(newAds.Select(ad => NotifyUsers(ad, searchFilter.ChatIds)));
                    }

                    await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Some error occured: {ex.GetType()}::{ex.Message}\nContinue scanning..");
                }
            }
        }
    }
}
