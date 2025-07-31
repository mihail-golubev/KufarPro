using KufarPro.Api.Models;

namespace KufarPro.Api.Services.Interfaces
{
    public interface IDbSubscriptionService
    {
        public Task<IEnumerable<SearchFilter>> GetAll(long chatId);

        public Task<SearchFilter> Get(string urlQuery);

        public Task<SearchFilter> AddOrUpdate(long chatId, string urlQuery);

        public Task<bool> Delete(long chatId, string urlQuery);
    }
}
