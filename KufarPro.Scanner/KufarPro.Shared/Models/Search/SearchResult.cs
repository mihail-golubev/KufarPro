using KufarPro.Shared.Models.Ads;
using System.Text.Json.Serialization;

namespace KufarPro.Shared.Models.Search
{
    public class SearchResult
    {
        [JsonPropertyName("ads")]
        public List<Ad> Ads { get; set; }

        [JsonPropertyName("total")]
        public int Total { get; set; }
    }
}
