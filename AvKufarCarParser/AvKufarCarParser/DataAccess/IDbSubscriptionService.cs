using AvKufarCarParser.Models.Database;

namespace AvKufarCarParser.DataAccess
{
    public interface IDbSubscriptionService
    {
        public Task<List<SearchFilter>> GetAllFiltersAsync();

        public Task<SearchFilter> GetFilterByParametersAsync(string urlQuery);

        public Task<SearchFilter> AddOrUpdateSubscriptionAsync(long chatId, string urlQuery);

        public Task<SearchFilter> RemoveSubscriptionAsync(long chatId, string urlQuery);
    }
}