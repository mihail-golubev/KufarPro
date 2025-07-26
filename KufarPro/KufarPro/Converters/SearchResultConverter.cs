using KufarPro.Models.Kufar.API;
using KufarPro.Models.Kufar.HelperModels;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace KufarPro.Converters
{
    public class SearchResultConverter : JsonConverter<SearchResult>
    {
        private readonly AdType _adType;

        public SearchResultConverter(AdType adType)
        {
            _adType = adType;
        }

        public override SearchResult Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            using var jsonDoc = JsonDocument.ParseValue(ref reader);
            var root = jsonDoc.RootElement;

            var total = root.GetProperty("total").GetInt32();
            var adsElement = root.GetProperty("ads");

            var ads = new List<Ad>();

            foreach (var element in adsElement.EnumerateArray())
            {
                Ad ad = _adType switch
                {
                    AdType.Auto => JsonSerializer.Deserialize<AutoAd>(element.GetRawText(), options),
                    AdType.Bicycle => JsonSerializer.Deserialize<BicycleAd>(element.GetRawText(), options),
                    _ => JsonSerializer.Deserialize<Ad>(element.GetRawText(), options)
                };

                if (ad != null)
                    ads.Add(ad);
            }

            return new SearchResult
            {
                Ads = ads,
                Total = total
            };
        }

        public override void Write(Utf8JsonWriter writer, SearchResult value, JsonSerializerOptions options)
        {
            throw new NotImplementedException("Serialization not required.");
        }
    }
}
