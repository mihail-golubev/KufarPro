using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using LiteDB;

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

        [BsonElement("chatIds")]
        [BsonField("chatIds")]
        public List<long> ChatIds { get; set; }

        [LiteDB.BsonId]
        public LiteDB.ObjectId LiteDbId { 
            get => string.IsNullOrEmpty(Id) ? LiteDB.ObjectId.NewObjectId() : new LiteDB.ObjectId(Id); 
            set => Id = value.ToString(); 
        }
    }
}
