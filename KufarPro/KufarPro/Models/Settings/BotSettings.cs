namespace KufarPro.Models.Settings
{
    public class BotSettings
    {
        public string BotType { get; set; }
        public string BotTokenEnvVariableName { get; set; }
    }

    public class LogSettings
    {
        public string LogFileName { get; set; } = "KufarPro.log";
        public long MaxLogFileSize { get; set; } = 512 * 1024;
    }
}
