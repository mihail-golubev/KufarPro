using MongoDB.Bson.Serialization.Attributes;

namespace AvKufarCarParser.Models.Database
{
    public class FilterParameter
    {
        [BsonElement("queryName")]
        public string QueryName { get; set; }

        [BsonElement("value")]
        public string Value { get; set; }
    }
}
