using System.Text.Json.Serialization;

namespace KufarPro.Bot.Models.Kufar.API
{
    public class SearchResult
    {
        [JsonPropertyName("ads")]
        public List<Ad> Ads { get; set; }

        [JsonPropertyName("total")]
        public int Total { get; set; }
    }
}
