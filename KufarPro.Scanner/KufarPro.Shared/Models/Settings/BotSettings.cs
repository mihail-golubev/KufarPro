using KufarPro.Shared.Models.HelperModels;

namespace KufarPro.Shared.Models.Settings
{
    public class BotSettings
    {
        public BotType BotType { get; set; }
        public string BotTokenEnvVariableName { get; set; }
    }
}
