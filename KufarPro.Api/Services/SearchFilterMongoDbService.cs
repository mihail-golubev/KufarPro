using KufarPro.Api.Services.Interfaces;
using KufarPro.Shared.Mappers;
using KufarPro.Shared.Messaging.Interfaces;
using KufarPro.Shared.Models.Search;
using KufarPro.Shared.Models.Settings;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace KufarPro.Api.Services;

public class SearchFilterService : IDbSubscriptionService, IDbUpdaterService
{
    private const string CollectionName = "search-filters";

    private readonly string _queueName;
    private readonly IMongoCollection<SearchFilter> _searchFiltersCollection;
    private readonly IMessageQueueService _messageQueueService;
    private readonly MessageQueueSettings _messageQueueSettings;

    public SearchFilterService(IMongoDatabase database, IMessageQueueService messageQueueService, IOptions<MessageQueueSettings> messageQueueOptions)
    {
        _searchFiltersCollection = database.GetCollection<SearchFilter>(CollectionName);
        _messageQueueService = messageQueueService;
        _queueName = messageQueueOptions.Value.NewFiltersQueueName;
    }

    public async Task<IEnumerable<SearchFilter>> GetAll()
    {
        return await _searchFiltersCollection.Find(_ => true).ToListAsync();
    }

    public async Task<IEnumerable<SearchFilter>> GetAll(long chatId)
    {
        var filterDefinition = Builders<SearchFilter>.Filter.AnyEq(filter => filter.ChatIds, chatId);
        return await _searchFiltersCollection.Find(filterDefinition).ToListAsync();
    }

    public async Task<SearchFilter> Get(string urlQuery)
    {
        var filterDefinition = Builders<SearchFilter>.Filter.Eq(filter => filter.UrlQuery, urlQuery);
        return await _searchFiltersCollection.Find(filterDefinition).FirstOrDefaultAsync();
    }

    public async Task<SearchFilter> AddOrUpdate(long chatId, string urlQuery)
    {
        var existingFilter = await Get(urlQuery);

        if (existingFilter != null)
        {
            if (!existingFilter.ChatIds.Contains(chatId))
            {
                existingFilter.ChatIds.Add(chatId);
                await _searchFiltersCollection.ReplaceOneAsync(filter => filter.UrlQuery == existingFilter.UrlQuery, existingFilter);

                await _messageQueueService.PublishAsync(_queueName, existingFilter.MapToNewFilter());
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

            await _messageQueueService.PublishAsync(_queueName, newFilter.MapToNewFilter());
            return newFilter;
        }
    }

    public async Task<bool> Delete(long chatId, string urlQuery)
    {
        var existingFilter = await Get(urlQuery);

        if (existingFilter != null)
        {
            if (existingFilter.ChatIds.Contains(chatId))
            {
                existingFilter.ChatIds.Remove(chatId);

                if (existingFilter.ChatIds.Count == 0)
                {
                    await _searchFiltersCollection.DeleteOneAsync(filter => filter.UrlQuery == existingFilter.UrlQuery);
                }
                else
                {
                    await _searchFiltersCollection.ReplaceOneAsync(filter => filter.UrlQuery == existingFilter.UrlQuery, existingFilter);
                }

                await _messageQueueService.PublishAsync(_queueName, existingFilter.MapToNewFilter());
                return true;
            }
        }

        return false;
    }

    public async Task Update(SearchFilter searchFilter)
    {
        var filter = Builders<SearchFilter>.Filter.Eq(filter => filter.UrlQuery, searchFilter.UrlQuery);
        var updatedFilter = Builders<SearchFilter>.Update
            .Set(x => x.LatestAdsIds, searchFilter.LatestAdsIds);

        await _searchFiltersCollection.UpdateOneAsync(filter, updatedFilter);
    }
}
