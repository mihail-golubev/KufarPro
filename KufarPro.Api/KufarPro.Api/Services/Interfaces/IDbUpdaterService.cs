using KufarPro.Api.Models;

namespace KufarPro.Api.Services.Interfaces
{
    public interface IDbUpdaterService
    {
        Task<IEnumerable<SearchFilter>> GetAll();
        Task Update(SearchFilter searchFilter);
    }
}
