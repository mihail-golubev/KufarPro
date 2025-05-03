using AvKufarCarParser.Helpers;
using System.Text;
using System.Text.Json;
using Telegram.Bot;

namespace AvKufarCarParser.Av
{
    public class AvProcessor
    {
        private readonly HttpClient _httpClient;
        private readonly ITelegramBotClient _botClient;

        public AvProcessor(HttpClient httpClient, ITelegramBotClient botClient)
        {
            _httpClient = httpClient;
            _botClient = botClient;
        }

        private async Task<string> Login()
        {
            var loginModel = AppHelper.GetLoginModel();

            var jsonPayload = JsonSerializer.Serialize(loginModel);
            var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync("https://api.av.by/auth/token/login", content);

            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadAsStringAsync();

            return result;
        }
    }
}
