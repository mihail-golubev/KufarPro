using KufarPro.Shared.Models.HelperModels;

namespace KufarPro.Shared.Models.DTOs
{
    public class NewAdsQueueModel
    {
        public IEnumerable<NewAd> Ads { get; set; } = new List<NewAd>();

        public BotType BotType { get; set; } = BotType.Unknown;

        public IEnumerable<long> ChatIds { get; set; } = new List<long>();
    }
}
