using LiteDB;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace KufarPro.Models.Database
{
    public class SearchFilter
    {
        [LiteDB.BsonIgnore]
        [MongoDB.Bson.Serialization.Attributes.BsonId]
        [BsonRepresentation(MongoDB.Bson.BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonElement("urlQuery")]
        [BsonField("urlQuery")]
        public string UrlQuery {  get; set; }

        [BsonElement("latestAdsIds")]
        [BsonField("latestAdsIds")]
        public HashSet<int> LatestAdsIds { get; set; }

        [BsonElement("chatIds")]
        [BsonField("chatIds")]
        public List<long> ChatIds { get; set; }

        [LiteDB.BsonId]
        public LiteDB.ObjectId LiteDbId
        {
            get => new LiteDB.ObjectId(Id);
            set => Id = value.ToString();
        }
    }
}
