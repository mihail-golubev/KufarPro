using System.Text.Json.Serialization;
using KufarPro.Models.Kufar.HelperModels;

namespace KufarPro.Models.Kufar.API
{
    public class Ad
    {
        [JsonPropertyName("ad_id")]
        public int Id { get; set; }

        [JsonPropertyName("account_id")]
        public string AuthorId { get; set; }

        [JsonPropertyName("account_parameters")]
        public List<Parameter> AccountParameters { get; set; }

        [JsonIgnore]
        public AuthorParams AuthorParams => ParseAuthorParams();

        [JsonPropertyName("ad_parameters")]
        public List<Parameter> AdParameters { get; set; }

        [JsonPropertyName("ad_link")]
        public string Link { get; set; }

        [JsonPropertyName("phone_hidden")]
        public bool PhoneHidden { get; set; }

        [JsonPropertyName("list_time")]
        public string RawListTime { get; set; }

        [JsonIgnore]
        public DateTime ListTime => DateTime.TryParse(RawListTime, out var parsedDate) ? parsedDate : DateTime.MinValue;

        [JsonPropertyName("price_byn")]
        public string RawPriceByn { get; set; }

        [JsonIgnore]
        public double PriceByn
        {
            get
            {
                if (double.TryParse(RawPriceByn, out double parsedPrice))
                {
                    return parsedPrice / 100;
                }
                return 0;
            }
        }

        [JsonPropertyName("price_usd")]
        public string RawPriceUsd { get; set; }

        [JsonIgnore]
        public double PriceUsd
        {
            get
            {
                if (double.TryParse(RawPriceUsd, out double parsedPrice))
                {
                    return parsedPrice / 100;
                }
                return 0;
            }
        }

        [JsonPropertyName("images")]
        public List<ImageModel> Images { get; set; }


        [JsonPropertyName("subject")]
        public string Subject { get; set; }


        private AuthorParams ParseAuthorParams()
        {
            var authorParams = new AuthorParams();

            authorParams.Id = AuthorId;

            foreach (var param in AccountParameters)
            {
                switch (param.Key)
                {
                    case "name":
                        authorParams.Name = param.Value.ToString();
                        break;
                }
            }

            return authorParams;
        }
    }
}