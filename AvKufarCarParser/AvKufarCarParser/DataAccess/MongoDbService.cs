using AvKufarCarParser.Helpers;
using AvKufarCarParser.Models.Database;
using MongoDB.Driver;

namespace AvKufarCarParser.DataAccess
{
    public class MongoDbService : IDbSubscriptionService, IDbUpdaterService
    {
        private readonly IMongoCollection<SearchFilter> _searchFiltersCollection;

        public MongoDbService(IMongoClient client)
        {
            var database = client.GetDatabase(AppHelper.DbName);
            _searchFiltersCollection = database.GetCollection<SearchFilter>(AppHelper.CollectionName);
        }

        public async Task<List<SearchFilter>> GetAllFiltersAsync()
        {
            return await _searchFiltersCollection.Find(_ => true).ToListAsync();
        }

        public async Task<SearchFilter> GetFilterByParametersAsync(List<FilterParameter> parameters)
        {
            var filterDefinition = Builders<SearchFilter>.Filter.Eq(f => f.FilterParameters, parameters);
            return await _searchFiltersCollection.Find(filterDefinition).FirstOrDefaultAsync();
        }

        public async Task<SearchFilter> AddOrUpdateSubscriptionAsync(long chatId, List<FilterParameter> parameters)
        {
            var existingFilter = await GetFilterByParametersAsync(parameters);

            if (existingFilter != null)
            {
                if (!existingFilter.ChatIds.Contains(chatId))
                {
                    existingFilter.ChatIds.Add(chatId);
                    await _searchFiltersCollection.ReplaceOneAsync(f => f.Id == existingFilter.Id, existingFilter);

                    return existingFilter;
                }

                return null;
            }
            else
            {
                var newFilter = new SearchFilter
                {
                    FilterParameters = parameters,
                    ChatIds = new List<long> { chatId }
                };

                await _searchFiltersCollection.InsertOneAsync(newFilter);

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
                        await _searchFiltersCollection.DeleteOneAsync(f => f.Id == existingFilter.Id);
                    }
                    else
                    {
                        await _searchFiltersCollection.ReplaceOneAsync(f => f.Id == existingFilter.Id, existingFilter);
                    }

                    return existingFilter;
                }
            }

            return null;
        }

        public async Task UpdateSearchFilter(SearchFilter searchFilter)
        {
            var filter = Builders<SearchFilter>.Filter.Eq(x => x.Id, searchFilter.Id);
            var updatedFilter = Builders<SearchFilter>.Update
                .Set(x => x.Total, searchFilter.Total)
                .Set(x => x.LatestAdsIds, searchFilter.LatestAdsIds);

            await _searchFiltersCollection.UpdateOneAsync(filter, updatedFilter);
        }
    }
}
