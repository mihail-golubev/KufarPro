using KufarPro.Scanner.Helpers;
using KufarPro.Scanner.HttpClients.Interfaces;
using KufarPro.Shared.Converters;
using KufarPro.Shared.Models.Search;
using System.Text.Json;

namespace KufarPro.Scanner.HttpClients
{
    public class KufarClient : IKufarApiClient
    {
        private readonly HttpClient _client;
        private readonly ILogger<SearchFiltersApiClient> _logger;

        public KufarClient(IHttpClientFactory httpClientFactory, ILogger<SearchFiltersApiClient> logger)
        {
            _client = httpClientFactory.CreateClient("KufarApiClient");
            _logger = logger;
        }

        public async Task<SearchResult> GetSearchResult(string query)
        {
            try
            {
                var adType = AppHelper.GetAdType(query);
                var response = await _client.GetAsync($"?{AppHelper.BaseKufarQuery}{query}");
                response.EnsureSuccessStatusCode();
                var responseString = await response.Content.ReadAsStringAsync();

                var options = new JsonSerializerOptions();
                options.Converters.Add(new SearchResultConverter(adType));
                options.PropertyNameCaseInsensitive = true;

                return JsonSerializer.Deserialize<SearchResult>(responseString, options);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occurred while getting ads from Kufar.");
                return new SearchResult();
            }
        }
    }
}
