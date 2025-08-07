using KufarPro.Shared.Models.Search;

namespace KufarPro.Scanner.HttpClients.Interfaces
{
    public interface IUpdateSearchFilterApiClient
    {
        Task<IEnumerable<SearchFilter>> GetAll();
        Task UpdateLatestAdsIds(string urlQuery, HashSet<int> latestAdsIds);
    }
}
