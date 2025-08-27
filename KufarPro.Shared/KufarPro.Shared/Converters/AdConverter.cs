using KufarPro.Shared.Helpers;
using KufarPro.Shared.Models.Ads;
using KufarPro.Shared.Models.HelperModels;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace KufarPro.Shared.Converters
{
    public class AdConverter : JsonConverter<Ad>
    {
        public override Ad Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var ad = new Ad();

            using var doc = JsonDocument.ParseValue(ref reader);
            var root = doc.RootElement;

            ad.Id = root.GetProperty("ad_id").GetInt32();
            ad.Type = AdTypeHelper.GetAdType(int.Parse(root.GetProperty("category").GetString()));
            ad.Url = root.GetProperty("ad_link").GetString();
            ad.Subject = root.GetProperty("subject").GetString();
            ad.ListTime = DateTime.TryParse(root.GetProperty("list_time").GetString(), out var dateTime) ? dateTime : DateTime.MinValue;
            ad.Currency = root.GetProperty("currency").GetString();

            string rawPrice = ad.Currency switch
            {
                "USD" => root.GetProperty("price_usd").GetString(),
                _ => root.GetProperty("price_byn").GetString()
            };
            ad.Price = Math.Round(double.TryParse(rawPrice, out var priceValue) ? priceValue / 100 : 0);

            ad.Images = root.GetProperty("images").EnumerateArray()
                .Select(img => $"https://{img.GetProperty("media_storage").GetString()}.kufar.by/v1/gallery/{img.GetProperty("path").GetString()}")
                .ToList();

            ad.Author = new AuthorModel { Id = root.GetProperty("account_id").GetString() };

            if (root.TryGetProperty("account_parameters", out var accParams))
            {
                foreach (var param in accParams.EnumerateArray())
                {
                    if (param.GetProperty("p").GetString() == "name")
                    {
                        ad.Author.Name = param.GetProperty("v").GetString();
                    }
                }
            }

            if (root.TryGetProperty("ad_parameters", out var adParamsFromJson))
            {
                var adParams = adParamsFromJson.EnumerateArray().Select(x => GetAdParameter(x));
                ad.Parameters = adParams.Where(param => AdTypeHelper.GetAllowedKeys(ad.Type).Contains(param.Property)).ToList();

                ad.City = adParams.FirstOrDefault(x => x.Property == "area").ValueText;
                ad.Region = adParams.FirstOrDefault(x => x.Property == "region").ValueText;
            }

            return ad;
        }

        public override void Write(Utf8JsonWriter writer, Ad value, JsonSerializerOptions options)
        {
            throw new NotImplementedException("Serialization not required");
        }

        private static AdParameter GetAdParameter(JsonElement adParamFromJson)
        {
            var result = new AdParameter
            {
                Label = adParamFromJson.GetProperty("pl").GetString(),
                Property = adParamFromJson.GetProperty("p").GetString()
            };

            if (adParamFromJson.TryGetProperty("vl", out var vl))
            {
                if (vl.ValueKind == JsonValueKind.Array)
                {
                    result.ValueText = string.Join(", ", vl.EnumerateArray().Select(x => x.ToString()));
                }
                else if (vl.ValueKind == JsonValueKind.String || vl.ValueKind == JsonValueKind.Number)
                {
                    result.ValueText = vl.ToString();
                }
                else
                {
                    result.ValueText = null;
                }
            }

            if (adParamFromJson.TryGetProperty("v", out var v))
            {
                if (v.ValueKind == JsonValueKind.Array)
                {
                    result.ValueRaw = string.Join(",", v.EnumerateArray().Select(x => x.ToString()));
                }
                else if (v.ValueKind == JsonValueKind.String || v.ValueKind == JsonValueKind.Number)
                {
                    result.ValueRaw = v.ToString();
                }
                else
                {
                    result.ValueRaw = null;
                }
            }

            return result;
        }
    }
}
