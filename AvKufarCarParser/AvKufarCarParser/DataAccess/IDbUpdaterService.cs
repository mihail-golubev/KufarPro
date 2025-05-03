using AvKufarCarParser.Models.Database;

namespace AvKufarCarParser.DataAccess
{
    public interface IDbUpdaterService
    {
        public Task UpdateSearchFilter(SearchFilter searchFilter);
    }
}
