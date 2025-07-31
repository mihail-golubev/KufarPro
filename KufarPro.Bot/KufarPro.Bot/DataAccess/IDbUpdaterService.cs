using KufarPro.Bot.Models.Database;

namespace KufarPro.Bot.DataAccess
{
    public interface IDbUpdaterService
    {
        public Task UpdateSearchFilter(SearchFilter searchFilter);
    }
}
