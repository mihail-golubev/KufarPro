using System.Text.Json.Serialization;

namespace AvKufarCarParser.Models
{
    public class Parameter
    {
        [JsonPropertyName("p")]
        public string Key { get; set; }

        [JsonPropertyName("pl")]
        public string KeyRu { get; set; }

        [JsonPropertyName("vl")]
        public string ValueLong { get; set; }

        [JsonPropertyName("v")]
        public object Value { get; set; }
    }
}
