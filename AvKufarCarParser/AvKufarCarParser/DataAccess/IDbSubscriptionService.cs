using AvKufarCarParser.Models.Database;

namespace AvKufarCarParser.DataAccess
{
    public interface IDbSubscriptionService
    {
        public Task<List<SearchFilter>> GetAllFiltersAsync();

        public Task<SearchFilter> GetFilterByParametersAsync(List<FilterParameter> parameters);

        public Task<SearchFilter> AddOrUpdateSubscriptionAsync(long chatId, List<FilterParameter> parameters);

        public Task<SearchFilter> RemoveSubscriptionAsync(long chatId, List<FilterParameter> parameters);
    }
}