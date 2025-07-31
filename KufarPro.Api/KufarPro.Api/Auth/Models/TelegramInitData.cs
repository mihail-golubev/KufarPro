namespace KufarPro.Api.Auth.Models
{
    public class TelegramInitData
    {
        public string Hash { get; set; }
        public TelegramUser User { get; set; } = null!;
    }
}
