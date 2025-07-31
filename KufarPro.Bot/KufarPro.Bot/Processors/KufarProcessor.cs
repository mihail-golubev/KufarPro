using KufarPro.Bot.Converters;
using KufarPro.Bot.DataAccess;
using KufarPro.Bot.Helpers;
using KufarPro.Bot.Models.Database;
using KufarPro.Bot.Models.Kufar.API;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace KufarPro.Bot.Processors
{
    public class KufarProcessor
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<KufarProcessor> _logger;
        private readonly IDbUpdaterService _dbUpdaterService;

        public KufarProcessor(HttpClient httpClient, IDbUpdaterService dbUpdaterService, ILogger<KufarProcessor> logger)
        {
            _httpClient = httpClient;
            _dbUpdaterService = dbUpdaterService;
            _logger = logger;
        }

        public async Task<List<Ad>> ScanForNewAds(SearchFilter searchFilter)
        {
            var url = $"{AppHelper.BaseKufarGetLink}&{searchFilter.UrlQuery}";
            var searchResult = await GetSearchResult(url);

            return GetNewAds(searchResult, searchFilter);
        }

        private List<Ad> GetNewAds(SearchResult searchResult, SearchFilter searchFilter)
        {
            var result = new List<Ad>();
            var adsIds = searchResult.Ads.Select(x => x.Id).ToHashSet();

            if (searchFilter.LatestAdsIds == null || searchFilter.LatestAdsIds.Count == 0)
            {
                searchFilter.LatestAdsIds = adsIds;
                _dbUpdaterService.UpdateSearchFilter(searchFilter);

                _logger.LogInformation("Initial ads list has been saved.");

                return result;
            }
            _logger.LogInformation($"There are {searchResult.Total} ads in total.");

            if (!searchFilter.LatestAdsIds.SequenceEqual(adsIds))
            {
                int latestAdId = searchFilter.LatestAdsIds.FirstOrDefault();

                if (adsIds.Contains(latestAdId))
                {
                    result = searchResult.Ads.Take(adsIds.ToList().IndexOf(latestAdId)).ToList();
                }
                else
                {
                    result = searchResult.Ads;
                }

                searchFilter.LatestAdsIds = adsIds;
                _dbUpdaterService.UpdateSearchFilter(searchFilter);
            }

            _logger.LogInformation($"{result.Count} new ad(s) detected.");

            result.ForEach(x => x.Images = x.Images?.Take(10).ToList());

            return result;
        }

        private async Task<SearchResult> GetSearchResult(string url)
        {
            var adType = AppHelper.GetAdType(url);
            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();
            var responseString = await response.Content.ReadAsStringAsync();

            var options = new JsonSerializerOptions();
            options.Converters.Add(new SearchResultConverter(adType));
            options.PropertyNameCaseInsensitive = true;

            return JsonSerializer.Deserialize<SearchResult>(responseString, options);
        }
    }
}
