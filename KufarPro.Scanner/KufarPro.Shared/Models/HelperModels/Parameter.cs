using System.Text.Json.Serialization;

namespace KufarPro.Shared.Models.HelperModels
{
    public class Parameter
    {
        [JsonPropertyName("p")]
        public string Key { get; set; }

        [JsonPropertyName("pl")]
        public string KeyRu { get; set; }

        [JsonPropertyName("vl")]
        public object ValueLong { get; set; }

        [JsonPropertyName("v")]
        public object Value { get; set; }
    }
}
