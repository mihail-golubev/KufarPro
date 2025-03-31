using AvKufarCarParser.Models;
using System.Text.Json;

namespace AvKufarCarParser.Kufar
{
    public class KufarProcessor
    {
        private readonly HttpClient _httpClient;

        private int _previousTotal = -1;
        private int _currentTotal;

        public KufarProcessor(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<Ad>> GetNewAds()
        {
            var result = new List<Ad>();

            var response = await _httpClient.GetAsync(Util.GetLink);
            response.EnsureSuccessStatusCode();
            var responseString = await response.Content.ReadAsStringAsync();
            var searchResult = JsonSerializer.Deserialize<SearchResult>(responseString);

            if (_previousTotal != -1)
            {
                _previousTotal = _currentTotal;
                _currentTotal = searchResult.Total;

                var quantityOfNewAds = _currentTotal - _previousTotal;

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
            else
            {
                _previousTotal = searchResult.Total;
                _currentTotal = searchResult.Total;
            }

            return result;
        }
    }
}
