using KufarPro.Scanner.HttpClients.Interfaces;
using KufarPro.Shared.Models.Search;
using System.Net.Http.Json;

namespace KufarPro.Scanner.HttpClients
{
    public class SearchFiltersApiClient : IGetSearchFiltersApiClient, IUpdateSearchFilterApiClient
    {
        private readonly HttpClient _client;
        private readonly ILogger<SearchFiltersApiClient> _logger;

        public SearchFiltersApiClient(IHttpClientFactory httpClientFactory, ILogger<SearchFiltersApiClient> logger)
        {
            _client = httpClientFactory.CreateClient("SearchFiltersApiClient");
            _logger = logger;
        }

        public async Task<IEnumerable<SearchFilter>> GetAll()
        {
            try
            {
                var response = await _client.GetAsync("search-filters");

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError($"Failed to fetch search filters. StatusCode: {response.StatusCode}");
                    return Enumerable.Empty<SearchFilter>();
                }

                var filters = await response.Content.ReadFromJsonAsync<List<SearchFilter>>();

                return filters ?? Enumerable.Empty<SearchFilter>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occurred while fetching search filters.");
                return Enumerable.Empty<SearchFilter>();
            }
        }

        public async Task UpdateLatestAdsIds(string urlQuery, HashSet<int> latestAdsIds)
        {
            try
            {
                var filter = new SearchFilter
                {
                    UrlQuery = urlQuery,
                    LatestAdsIds = latestAdsIds
                };

                var response = await _client.PatchAsJsonAsync("search-filters/update", filter);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError($"Failed to update filter for {urlQuery}: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occurred while updating search filters.");
            }
        }
    }
}