using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace AvKufarCarParser.Models.Database
{
    public class SearchFilter
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonElement("filterParameters")]
        public List<FilterParameter> FilterParameters { get; set; }

        [BsonElement("chatIds")]
        public List<long> ChatIds { get; set; }
    }
}
