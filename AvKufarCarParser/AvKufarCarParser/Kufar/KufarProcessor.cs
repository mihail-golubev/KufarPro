using AvKufarCarParser.Models;
using Microsoft.Extensions.Logging;
using System.Text.Json;

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

        public async Task<List<Ad>> GetNewAds()
        {
            var result = new List<Ad>();

            var response = await _httpClient.GetAsync(Util.GetLink);
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
                        //result.Add(searchResult.Ads.FirstOrDefault());
                    }
                    else
                    {
                        result = searchResult.Ads;
                    }
                }

                _logger.LogInformation($"{result.Count} new ad(s) detected.");
            }

            return result;
        }
    }
}
