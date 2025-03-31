using System.Text.Json.Serialization;

namespace AvKufarCarParser.Models
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

        [JsonPropertyName("ad_link")]
        public string Link { get; set; }

        [JsonPropertyName("ad_parameters")]
        public List<Parameter> AdParameters { get; set; }

        [JsonIgnore]
        public CarParams CarParams => ParseCarParams();

        [JsonPropertyName("phone_hidden")]
        public bool PhoneHidden { get; set; }

        [JsonPropertyName("price_usd")]
        public string RawPrice { get; set; }

        [JsonIgnore]
        public int Price
        {
            get
            {
                if (int.TryParse(RawPrice, out int parsedPrice))
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

        private CarParams ParseCarParams()
        {
            var carParams = new CarParams();

            foreach (var param in AdParameters)
            {
                switch (param.Key)
                {
                    case "brand":
                        carParams.Brand = param.ValueLong;
                        break;
                    case "cars_level_1":
                        carParams.Model = param.ValueLong;
                        break;
                    case "regdate":
                        carParams.Year = param.ValueLong;
                        break;
                    case "mileage":
                        carParams.Mileage = param.Value.ToString();
                        break;
                    case "cars_engine":
                        carParams.FuelType = param.ValueLong;
                        break;
                    case "cars_capacity":
                        carParams.EngineCapacity = param.ValueLong;
                        break;
                    case "cars_gearbox":
                        carParams.GearboxType = param.ValueLong;
                        break;
                    case "cars_type":
                        carParams.BodyType = param.ValueLong;
                        break;
                    case "cars_drive":
                        carParams.DriveType = param.ValueLong;
                        break;
                    case "region":
                        carParams.Region = param.ValueLong;
                        break;
                    case "area":
                        carParams.City = param.ValueLong;
                        break;
                }
            }

            return carParams;
        }
    }
}
