using KufarPro.Bot.Models.Kufar.HelperModels;

namespace KufarPro.Bot.Helpers
{
    public class FilterBuilder
    {
        private readonly Dictionary<string, string> _parameters = new();

        public FilterBuilder SetCategory(string category)
        {
            if (!string.IsNullOrEmpty(category))
            {
                _parameters["cat"] = category;
            }

            return this;
        }

        public FilterBuilder SetRegion(string region)
        {
            if (!string.IsNullOrEmpty(region) && region != "0")
            {
                _parameters["rgn"] = region;
            }

            return this;
        }

        public FilterBuilder SetPriceRange(PriceRange priceRange)
        {
            if ((priceRange.From ?? 0) == 0 && (priceRange.To ?? 0) == 0)
            {
                return this;
            }

            int min = priceRange.From ?? 0;
            int max = priceRange.To ?? int.MaxValue;

            _parameters["prc"] = $"r:{min},{max}";

            return this;
        }

        public FilterBuilder SetBrand(string brand)
        {
            if (!string.IsNullOrEmpty(brand))
            {
                _parameters["brand"] = brand;
            }

            return this;
        }

        public string Build()
        {
            return string.Join("&", _parameters.Select(p => $"{p.Key}={p.Value}"));
        }
    }
}