namespace KufarPro.Shared.Models.Settings
{
    public class LogSettings
    {
        public string LogFileName { get; set; } = "KufarPro.log";
        public long MaxLogFileSize { get; set; } = 512 * 1024;
    }
}
