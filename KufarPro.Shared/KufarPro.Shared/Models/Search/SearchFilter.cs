using MongoDB.Bson.Serialization.Attributes;

namespace KufarPro.Shared.Models.Search
{
    public class SearchFilter
    {
        [BsonId]
        [BsonElement("urlQuery")]
        public string UrlQuery { get; set; }

        [BsonElement("latestAdsIds")]
        public HashSet<int> LatestAdsIds { get; set; } = new HashSet<int>();

        [BsonElement("chatIds")]
        public List<long> ChatIds { get; set; } = new List<long>();
    }
}
