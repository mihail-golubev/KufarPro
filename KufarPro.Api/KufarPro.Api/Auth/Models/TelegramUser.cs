using Newtonsoft.Json;

namespace KufarPro.Api.Auth.Models
{
    public class TelegramUser
    {
        [JsonProperty("id")]
        public long Id { get; set; }
        [JsonProperty("username")]
        public string Username { get; set; }
    }
}
