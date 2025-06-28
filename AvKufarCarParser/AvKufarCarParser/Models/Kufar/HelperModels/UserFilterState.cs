namespace AvKufarCarParser.Models.Kufar.HelperModels
{
    public class UserFilterState
    {
        public AdType AdType { get; set; } = AdType.Unknown;
        public string Category { get; set; }
        public string Region { get; set; }
        public PriceRange PriceRange { get; set; } = new PriceRange();

        // Car
        public string CarBrand { get; set; }
        public string CarModel { get; set; }

        // Bike
        public bool IsCompany { get; set; }

        // Estate
    }
}
