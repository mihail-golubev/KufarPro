using KufarPro.Shared.Models.Search;

namespace KufarPro.Scanner.HttpClients.Interfaces
{
    public interface IGetSearchFiltersApiClient
    {
        Task<IEnumerable<SearchFilter>> GetAll();
    }
}
