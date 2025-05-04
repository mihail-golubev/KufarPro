using LiteDB;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace AvKufarCarParser.Models.Database
{
    public class SearchFilter
    {
        [LiteDB.BsonIgnore]
        [MongoDB.Bson.Serialization.Attributes.BsonId]
        [BsonRepresentation(MongoDB.Bson.BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonElement("filterParameters")]
        [BsonField("filterParameters")]
        public List<FilterParameter> FilterParameters { get; set; }

        [BsonElement("latestAdsIds")]
        [BsonField("latestAdsIds")]
        public List<int> LatestAdsIds { get; set; }

        [BsonElement("total")]
        [BsonField("total")]
        public int Total { get; set; }

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
