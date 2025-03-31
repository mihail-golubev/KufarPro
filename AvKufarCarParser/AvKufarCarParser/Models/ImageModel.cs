using System.Text.Json.Serialization;

namespace AvKufarCarParser.Models
{
    public class ImageModel
    {
        [JsonPropertyName("media_storage")]
        public string MediaStorage { get; set; }

        [JsonPropertyName("path")]
        public string Path { get; set; }

        [JsonIgnore]
        public string Link => $"https://{MediaStorage}.kufar.by/v1/gallery/{Path}";
    }
}
