using AvKufarCarParser.Models.Database;

namespace AvKufarCarParser.DataAccess
{
    public interface IDbService
    {
        public Task<List<SearchFilter>> GetAllFiltersAsync();

        public Task<SearchFilter> GetFilterByParametersAsync(List<FilterParameter> parameters);

        public Task<SearchFilter> AddOrUpdateSubscriptionAsync(long chatId, List<FilterParameter> parameters);

        public Task<bool> RemoveSubscriptionAsync(long chatId, List<FilterParameter> parameters);
    }
}