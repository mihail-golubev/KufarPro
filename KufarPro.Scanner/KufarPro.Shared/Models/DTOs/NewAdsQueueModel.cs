using KufarPro.Shared.Models.HelperModels;

namespace KufarPro.Shared.Models.DTOs
{
    public class NewAdsQueueModel
    {
        public List<NewAd> Ads { get; set; } = new List<NewAd>();

        public BotType BotType { get; set; } = BotType.Unknown;

        public List<long> ChatIds { get; set; } = new List<long>();
    }
}
