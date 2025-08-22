using KufarPro.Shared.Models.Ads;
using KufarPro.Shared.Models.HelperModels;

namespace KufarPro.Shared.Models.DTOs
{
    public class NewAd
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
        public IEnumerable<string> Images { get; set; } = new List<string>();
        public AdType AdType { get; set; }
        public IEnumerable<string> Parameters { get; set; } = new List<string>();
    }
}
