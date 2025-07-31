using KufarPro.Bot.Models.Database;
using KufarPro.Bot.Models.Settings;
using LiteDB;
using Microsoft.Extensions.Options;

namespace KufarPro.Bot.DataAccess
{
    public class LiteDbService : IDbSubscriptionService, IDbUpdaterService
    {
        private readonly LiteDatabase _database;
        private readonly ILiteCollection<SearchFilter> _searchFiltersCollection;
        private readonly object _lock = new object();

        public LiteDbService(IOptions<DatabaseSettings> databaseOptions)
        {
            string dbPath = Path.Combine(AppContext.BaseDirectory, $"{databaseOptions.Value.DbName}.db");
            _database = new LiteDatabase(dbPath);
            _searchFiltersCollection = _database.GetCollection<SearchFilter>(databaseOptions.Value.DbName);
        }

        public async Task<List<SearchFilter>> GetAllFiltersAsync()
        {
            return await Task.Run(() => _searchFiltersCollection.FindAll().ToList());
        }

        public async Task<SearchFilter> GetFilterByParametersAsync(string urlQuery)
        {
            return await Task.Run(() =>
            {
                return _searchFiltersCollection
                    .FindAll()
                    .FirstOrDefault(filter => string.Equals(filter.UrlQuery, urlQuery, StringComparison.OrdinalIgnoreCase));
            });
        }

        public async Task<SearchFilter> AddOrUpdateSubscriptionAsync(long chatId, string urlQuery)
        {
            var existingFilter = await GetFilterByParametersAsync(urlQuery);

            if (existingFilter != null)
            {
                if (!existingFilter.ChatIds.Contains(chatId))
                {
                    existingFilter.ChatIds.Add(chatId);
                    _searchFiltersCollection.Update(existingFilter);
                    return existingFilter;
                }

                return null;
            }
            else
            {
                var newFilter = new SearchFilter
                {
                    Id = ObjectId.NewObjectId().ToString(),
                    UrlQuery = urlQuery,
                    ChatIds = new List<long> { chatId }
                };

                _searchFiltersCollection.Insert(newFilter);

                return newFilter;
            }
        }

        public async Task<SearchFilter> RemoveSubscriptionAsync(long chatId, string urlQuery)
        {
            var existingFilter = await GetFilterByParametersAsync(urlQuery);

            if (existingFilter != null)
            {
                if (existingFilter.ChatIds.Contains(chatId))
                {
                    existingFilter.ChatIds.Remove(chatId);

                    if (existingFilter.ChatIds.Count == 0)
                    {
                        _searchFiltersCollection.Delete(existingFilter.Id);
                    }
                    else
                    {
                        _searchFiltersCollection.Update(existingFilter);
                    }

                    return existingFilter;
                }
            }

            return null;
        }

        public Task UpdateSearchFilter(SearchFilter searchFilter)
        {
            lock (_lock)
            {
                var existingFilter = _searchFiltersCollection.FindById(searchFilter.LiteDbId) ?? throw new InvalidOperationException("Filter not found.");
                existingFilter.LatestAdsIds = searchFilter.LatestAdsIds;

                _searchFiltersCollection.Update(existingFilter);
            }

            return Task.CompletedTask;
        }
    }
}
