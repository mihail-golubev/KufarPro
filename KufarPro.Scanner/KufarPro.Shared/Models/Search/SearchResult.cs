using KufarPro.Shared.Models.Ads;
using System.Text.Json.Serialization;

namespace KufarPro.Shared.Models.Search
{
    public class SearchResult
    {
        [JsonPropertyName("ads")]
        public IEnumerable<Ad> Ads { get; set; } = new List<Ad>();

        [JsonPropertyName("total")]
        public int Total { get; set; }
    }
}
