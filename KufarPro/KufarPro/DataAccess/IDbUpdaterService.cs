using KufarPro.Models.Database;

namespace KufarPro.DataAccess
{
    public interface IDbUpdaterService
    {
        public Task UpdateSearchFilter(SearchFilter searchFilter);
    }
}
