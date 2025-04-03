using AvKufarCarParser.Models.Database;
using MongoDB.Driver;

namespace AvKufarCarParser.DataAccess
{
    public class DatabaseService
    {
        private readonly IMongoCollection<SearchFilter> _searchFilters;

        public DatabaseService(IMongoClient client)
        {
            var database = client.GetDatabase(Util.DbName);
            _searchFilters = database.GetCollection<SearchFilter>("searchFilters");
        }

        public async Task<List<SearchFilter>> GetAllFiltersAsync()
        {
            return await _searchFilters.Find(_ => true).ToListAsync();
        }

        public async Task<SearchFilter> GetFilterByParametersAsync(List<FilterParameter> parameters)
        {
            var filterDefinition = Builders<SearchFilter>.Filter.Eq(f => f.FilterParameters, parameters);
            return await _searchFilters.Find(filterDefinition).FirstOrDefaultAsync();
        }

        public async Task<SearchFilter> AddOrUpdateSubscriptionAsync(long chatId, List<FilterParameter> parameters)
        {
            var existingFilter = await GetFilterByParametersAsync(parameters);

            if (existingFilter != null)
            {
                if (!existingFilter.ChatIds.Contains(chatId))
                {
                    existingFilter.ChatIds.Add(chatId);
                    await _searchFilters.ReplaceOneAsync(f => f.Id == existingFilter.Id, existingFilter);

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

                await _searchFilters.InsertOneAsync(newFilter);

                return newFilter;
            }
        }

        public async Task<bool> RemoveSubscriptionAsync(long chatId, List<FilterParameter> parameters)
        {
            var existingFilter = await GetFilterByParametersAsync(parameters);

            if (existingFilter != null)
            {
                if (existingFilter.ChatIds.Contains(chatId))
                {
                    existingFilter.ChatIds.Remove(chatId);

                    if (existingFilter.ChatIds.Count == 0)
                    {
                        await _searchFilters.DeleteOneAsync(f => f.Id == existingFilter.Id);
                    }
                    else
                    {
                        await _searchFilters.ReplaceOneAsync(f => f.Id == existingFilter.Id, existingFilter);
                    }

                    return true;
                }
            }

            return false;
        }
    }
}
