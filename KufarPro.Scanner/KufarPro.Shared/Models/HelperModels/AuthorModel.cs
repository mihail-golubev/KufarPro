using System.Text.Json.Serialization;

namespace KufarPro.Shared.Models.HelperModels
{
    public class AuthorModel
    {
        [JsonPropertyName("account_id")]
        public string Id { get; set; }

        [JsonPropertyName("account_name")]
        public string Name { get; set; }
    }
}