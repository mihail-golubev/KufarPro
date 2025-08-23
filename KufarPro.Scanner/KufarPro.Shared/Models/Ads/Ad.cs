using KufarPro.Shared.Converters;
using KufarPro.Shared.Models.HelperModels;
using System.Text.Json.Serialization;

namespace KufarPro.Shared.Models.Ads
{
    [JsonConverter(typeof(AdConverter))]
    public class Ad
    {
        public int Id { get; set; }
        public string Url { get; set; }
        public string Subject { get; set; }
        public AuthorModel Author { get; set; }
        public DateTime ListTime { get; set; }
        public double Price { get; set; }
        public string Currency { get; set; }
        public string City { get; set; }
        public string Region { get; set; }
        public bool IsPhoneHidden { get; set; }
        public List<string> Images { get; set; } = new List<string>();
        public AdType Type { get; set; }
        public List<AdParameter> Parameters { get; set; } = new List<AdParameter>();
    }
}