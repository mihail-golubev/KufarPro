using AvKufarCarParser.EqualityComparers;
using AvKufarCarParser.Helpers;
using AvKufarCarParser.Models.Database;
using LiteDB;

namespace AvKufarCarParser.DataAccess
{
    public class LiteDbService : IDbSubscriptionService, IDbUpdaterService
    {
        private readonly LiteDatabase _database;
        private readonly ILiteCollection<SearchFilter> _searchFiltersCollection;
        private readonly object _lock = new object();

        public LiteDbService()
        {
            string dbPath = Path.Combine(AppContext.BaseDirectory, $"{AppHelper.DbName}.db");
            _database = new LiteDatabase(dbPath);
            _searchFiltersCollection = _database.GetCollection<SearchFilter>(AppHelper.CollectionName);
        }

        public async Task<List<SearchFilter>> GetAllFiltersAsync()
        {
            return await Task.Run(() => _searchFiltersCollection.FindAll().ToList());
        }

        public async Task<SearchFilter> GetFilterByParametersAsync(List<FilterParameter> parameters)
        {
            return await Task.Run(() =>
            {
                return _searchFiltersCollection.FindAll().FirstOrDefault(f =>
                    f.FilterParameters.Count == parameters.Count &&
                    !f.FilterParameters.Except(parameters, new FilterParameterComparer()).Any()
                );
            });
        }

        public async Task<SearchFilter> AddOrUpdateSubscriptionAsync(long chatId, List<FilterParameter> parameters)
        {
            var existingFilter = await GetFilterByParametersAsync(parameters);

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
                    FilterParameters = parameters,
                    ChatIds = new List<long> { chatId }
                };

                _searchFiltersCollection.Insert(newFilter);

                return newFilter;
            }
        }

        public async Task<SearchFilter> RemoveSubscriptionAsync(long chatId, List<FilterParameter> parameters)
        {
            var existingFilter = await GetFilterByParametersAsync(parameters);

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
                var f = _searchFiltersCollection.FindAll().ToList();
                var existingFilter = _searchFiltersCollection.FindById(searchFilter.LiteDbId) ?? throw new InvalidOperationException("Filter not found.");

                existingFilter.Total = searchFilter.Total;
                existingFilter.LatestAdsIds = searchFilter.LatestAdsIds;

                _searchFiltersCollection.Update(existingFilter);
            }

            return Task.CompletedTask;
        }
    }
}
