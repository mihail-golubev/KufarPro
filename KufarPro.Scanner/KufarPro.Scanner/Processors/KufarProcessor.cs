using KufarPro.Scanner.HttpClients.Interfaces;
using KufarPro.Shared.Models.Ads;
using KufarPro.Shared.Models.Search;

namespace KufarPro.Scanner.Processors
{
    public class KufarProcessor
    {
        private readonly IUpdateSearchFilterApiClient _searchFiltersClient;
        private readonly IKufarApiClient _kufarClient;
        private readonly ILogger<KufarProcessor> _logger;

        public KufarProcessor(IUpdateSearchFilterApiClient searchFiltersClient, IKufarApiClient kufarClient, ILogger<KufarProcessor> logger)
        {
            _searchFiltersClient = searchFiltersClient;
            _kufarClient = kufarClient;
            _logger = logger;
        }

        public async Task<List<Ad>> ScanForNewAds(SearchFilter searchFilter)
        {
            var searchResult = await _kufarClient.GetSearchResult(searchFilter.UrlQuery);
            var newAds = GetNewAds(searchResult, searchFilter);

            return newAds;
        }

        private List<Ad> GetNewAds(SearchResult searchResult, SearchFilter searchFilter)
        {
            var result = new List<Ad>();
            var adsIds = searchResult.Ads.Select(x => x.Id).ToHashSet();

            if (searchFilter.LatestAdsIds == null || searchFilter.LatestAdsIds.Count == 0)
            {
                searchFilter.LatestAdsIds = adsIds;
                _searchFiltersClient.UpdateLatestAdsIds(searchFilter.UrlQuery, searchFilter.LatestAdsIds);

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
                    result = searchResult.Ads.ToList();
                }

                searchFilter.LatestAdsIds = adsIds;
                _searchFiltersClient.UpdateLatestAdsIds(searchFilter.UrlQuery, searchFilter.LatestAdsIds);

            }

            _logger.LogInformation($"{result.Count} new ad(s) detected.");

            result.ForEach(x => x.Images = x.Images?.Take(10).ToList());

            return result;
        }
    }
}
