using KufarPro.Scanner.HttpClients.Interfaces;
using KufarPro.Shared.Models.Search;
using System.Text.Json;

namespace KufarPro.Scanner.HttpClients
{
    public class KufarClient : IKufarApiClient
    {
        public const string BaseKufarQuery = "sort=lst.d&size=10&cur=USD&cmp=0&";

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
                var response = await _client.GetAsync($"?{BaseKufarQuery}{query}");
                response.EnsureSuccessStatusCode();
                var responseString = await response.Content.ReadAsStringAsync();

                var result = JsonSerializer.Deserialize<SearchResult>(responseString);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occurred while getting ads from Kufar.");
                return new SearchResult();
            }
        }
    }
}
