using AvKufarCarParser.Models.Database;
using AvKufarCarParser.Models.Kufar;
using AvKufarCarParser.Models.Kufar.API;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using static System.Net.WebRequestMethods;

namespace AvKufarCarParser.Kufar
{
    public class KufarProcessor
    {
        private int _previousTotal = -1;
        private int _total;
        private int _previousLatestAdId = -1;
        private int _latestAdId;

        private readonly HttpClient _httpClient;
        private readonly ILogger<KufarProcessor> _logger;

        public KufarProcessor(HttpClient httpClient, ILogger<KufarProcessor> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<List<Ad>> GetNewAds(List<FilterParameter> parameters)
        {
            var result = new List<Ad>();
            //var url = GetScanUrl(parameters);
            var url = Util.GetLink;

            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();
            var responseString = await response.Content.ReadAsStringAsync();
            var searchResult = JsonSerializer.Deserialize<SearchResult>(responseString);

            if (_previousLatestAdId == -1 || _previousTotal == -1)
            {
                _previousTotal = searchResult.Total;
                _total = searchResult.Total;
                _previousLatestAdId = searchResult.Ads.FirstOrDefault().Id;
                _latestAdId = searchResult.Ads.FirstOrDefault().Id;

                _logger.LogInformation("Initial values have been set up!");
            }
            else
            {
                _logger.LogInformation($"There are {searchResult.Total} ads in total.");

                _previousTotal = _total;
                _total = searchResult.Total;
                _previousLatestAdId = _latestAdId;
                _latestAdId = searchResult.Ads.FirstOrDefault().Id;

                var quantityOfNewAds = _total - _previousTotal;

                if (_previousLatestAdId != _latestAdId && quantityOfNewAds > 0)
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

                _logger.LogInformation($"{result.Count} new ad(s) detected.");
            }

            //result.Add(searchResult.Ads[0]);
            return result;
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
