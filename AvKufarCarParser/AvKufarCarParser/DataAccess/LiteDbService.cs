using AvKufarCarParser.EqualityComparers;
using AvKufarCarParser.Models.Database;
using LiteDB;

namespace AvKufarCarParser.DataAccess
{
    public class LiteDbService : IDbService
    {
        private readonly LiteDatabase _database;
        private readonly ILiteCollection<SearchFilter> _filters;
        private readonly object _lock = new object();

        public LiteDbService()
        {
            string dbPath = Path.Combine(AppContext.BaseDirectory, $"{Util.DbName}.db");
            _database = new LiteDatabase(dbPath);
            _filters = _database.GetCollection<SearchFilter>(Util.CollectionName);
        }

        public async Task<List<SearchFilter>> GetAllFiltersAsync()
        {
            return await Task.Run(() => _filters.FindAll().ToList());
        }

        public async Task<SearchFilter> GetFilterByParametersAsync(List<FilterParameter> parameters)
        {
            return await Task.Run(() =>
            {
                return _filters.FindAll().FirstOrDefault(f =>
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
                    _filters.Update(existingFilter);
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

                _filters.Insert(newFilter);

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
                        _filters.Delete(existingFilter.Id);
                    }
                    else
                    {
                        _filters.Update(existingFilter);
                    }

                    return true;
                }
            }

            return false;
        }
    }
}
