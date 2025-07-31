using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace KufarPro.Api.Models;

public class SearchFilter
{
    [BsonId]
    [BsonElement("urlQuery")]
    public string UrlQuery { get; set; }

    [BsonElement("latestAdsIds")]
    public HashSet<int> LatestAdsIds { get; set; } = new();

    [BsonElement("chatIds")]
    public List<long> ChatIds { get; set; } = new();
}