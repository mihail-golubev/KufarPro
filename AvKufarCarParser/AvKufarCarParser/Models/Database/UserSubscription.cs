using MongoDB.Bson.Serialization.Attributes;

namespace AvKufarCarParser.Models.Database
{
    public class UserSubscription
    {
        [BsonId]
        public long ChatId { get; set; }

        public List<SearchFilter> SearchFilters { get; set; }
    }
}
