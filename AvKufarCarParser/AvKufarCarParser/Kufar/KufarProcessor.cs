using AvKufarCarParser.Converters;
using AvKufarCarParser.DataAccess;
using AvKufarCarParser.Helpers;
using AvKufarCarParser.Models.Database;
using AvKufarCarParser.Models.Kufar.API;
using AvKufarCarParser.Models.Kufar.HelperModels;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace AvKufarCarParser.Kufar
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
            var adType = AppHelper.GetAdType(searchFilter.FilterParameters);
            //var url = GetScanUrl(parameters);
            var url = adType == AdType.Car ? AppHelper.GetCarLink : AppHelper.GetBikeLink;

            var searchResult = await GetSearchResult(url, adType);

            return GetNewAds(searchResult, searchFilter);
        }

        private List<Ad> GetNewAds(SearchResult searchResult, SearchFilter searchFilter)
        {
            var result = new List<Ad>();
            var adsIds = searchResult.Ads.Select(x => x.Id).ToList();

            if (searchFilter.LatestAdsIds == null || searchFilter.Total == 0)
            {
                searchFilter.Total = searchResult.Total;
                searchFilter.LatestAdsIds = adsIds;
                _dbUpdaterService.UpdateSearchFilter(searchFilter);

                _logger.LogInformation("Initial values have been set up!");
            }
            else
            {
                _logger.LogInformation($"There are {searchResult.Total} ads in total.");

                if (searchResult.Total != searchFilter.Total || !searchFilter.LatestAdsIds.SequenceEqual(adsIds))
                {
                    var quantityOfNewAds = searchResult.Total - searchFilter.Total;

                    if (quantityOfNewAds > 0 && !searchFilter.LatestAdsIds.SequenceEqual(adsIds))
                    {
                        if (searchResult.Ads.Count >= quantityOfNewAds)
                        {
                            result = searchResult.Ads.Take(quantityOfNewAds).ToList();
                        }
                        else
                        {
                            result = searchResult.Ads;
                        }
                    }

                    searchFilter.Total = searchResult.Total;
                    searchFilter.LatestAdsIds = adsIds;

                    _dbUpdaterService.UpdateSearchFilter(searchFilter);
                }

                _logger.LogInformation($"{result.Count} new ad(s) detected.");
            }

            //result.Add(searchResult.Ads[1]);
            result.ForEach(x => x.Images = x.Images?.Take(10).ToList());

            return result;
        }

        private async Task<SearchResult> GetSearchResult(string url, AdType adType)
        {
            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();
            var responseString = await response.Content.ReadAsStringAsync();

            var options = new JsonSerializerOptions();
            options.Converters.Add(new SearchResultConverter(adType));
            options.PropertyNameCaseInsensitive = true;

            return JsonSerializer.Deserialize<SearchResult>(responseString, options);
        }

        private string GetScanUrl(List<FilterParameter> parameters)
        {
            var baseUrl = "https://api.kufar.by/search-api/v2/search/rendered-paginated?";
            var defaultParams = new Dictionary<string, string>
            {
                { "cat", "2010" },  // Car category
                { "cur", "USD" },   // Currency
                { "size", "10" },   // Number of results per page
                { "sort", "lst.d" } // Sorting by newest listings
            };

            var queryParams = parameters.ToDictionary(p => p.QueryName, p => p.Value);

            // Merge default params with user-defined ones (overwriting defaults if needed)
            foreach (var kvp in defaultParams)
            {
                if (!queryParams.ContainsKey(kvp.Key))
                {
                    queryParams[kvp.Key] = kvp.Value;
                }
            }

            // Build query string
            var queryString = string.Join("&", queryParams.Select(kvp => $"{kvp.Key}={Uri.EscapeDataString(kvp.Value)}"));

            return $"{baseUrl}{queryString}";
        }
    }
}
