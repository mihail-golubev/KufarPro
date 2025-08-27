namespace KufarPro.Shared.Models.Settings
{
    public class MessageQueueSettings
    {
        public string HostName { get; set; } = string.Empty;
        public int Port { get; set; }
        public string Username { get; set; } = string.Empty;
        public string PasswordEnvVariableName { get; set; } = string.Empty;
        public string NewFiltersQueueName { get; set; } = string.Empty;
        public string NewAdsQueueName { get; set; } = string.Empty;
    }
}