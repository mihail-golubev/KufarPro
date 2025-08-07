using KufarPro.Shared.Models.Search;

namespace KufarPro.Scanner.HttpClients.Interfaces
{
    public interface IKufarApiClient
    {
        Task<SearchResult> GetSearchResult(string query);
    }
}
