using KufarPro.Models.Database;
using KufarPro.Models.Settings;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace KufarPro.DataAccess
{
    public class MongoDbService : IDbSubscriptionService, IDbUpdaterService
    {
        private readonly IMongoCollection<SearchFilter> _searchFiltersCollection;

        public MongoDbService(IMongoClient client, IOptions<DatabaseSettings> databaseOptions)
        {
            var database = client.GetDatabase(databaseOptions.Value.DbName);
            _searchFiltersCollection = database.GetCollection<SearchFilter>(databaseOptions.Value.CollectionName);
        }

        public async Task<List<SearchFilter>> GetAllFiltersAsync()
        {
            return await _searchFiltersCollection.Find(_ => true).ToListAsync();
        }

        public async Task<SearchFilter> GetFilterByParametersAsync(string urlQuery)
        {
            var filterDefinition = Builders<SearchFilter>.Filter.Eq(f => f.UrlQuery, urlQuery);
            return await _searchFiltersCollection.Find(filterDefinition).FirstOrDefaultAsync();
        }

        public async Task<SearchFilter> AddOrUpdateSubscriptionAsync(long chatId, string urlQuery)
        {
            var existingFilter = await GetFilterByParametersAsync(urlQuery);

            if (existingFilter != null)
            {
                if (!existingFilter.ChatIds.Contains(chatId))
                {
                    existingFilter.ChatIds.Add(chatId);
                    await _searchFiltersCollection.ReplaceOneAsync(filter => filter.Id == existingFilter.Id, existingFilter);

                    return existingFilter;
                }

                return null;
            }
            else
            {
                var newFilter = new SearchFilter
                {
                    UrlQuery = urlQuery,
                    ChatIds = new List<long> { chatId }
                };

                await _searchFiltersCollection.InsertOneAsync(newFilter);

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
                        await _searchFiltersCollection.DeleteOneAsync(filter => filter.Id == existingFilter.Id);
                    }
                    else
                    {
                        await _searchFiltersCollection.ReplaceOneAsync(filter => filter.Id == existingFilter.Id, existingFilter);
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
                .Set(x => x.LatestAdsIds, searchFilter.LatestAdsIds);

            await _searchFiltersCollection.UpdateOneAsync(filter, updatedFilter);
        }
    }
}
