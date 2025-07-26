namespace KufarPro.Models.Kufar.HelperModels
{
    public class UserFilterState
    {
        public AdType AdType { get; set; } = AdType.Unknown;
        public string Category { get; set; }
        public string Region { get; set; }
        public PriceRange PriceRange { get; set; } = new PriceRange();

        // Auto
        public string AutoBrand { get; set; }
        public string AutoModel { get; set; }

        // Bicycle
        public bool IsCompany { get; set; }

        // Real Estate
    }
}
