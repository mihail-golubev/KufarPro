using System.Text.Json.Serialization;

namespace KufarPro.Bot.Models.Kufar.API
{
    public class AutoAd : Ad
    {
        [JsonIgnore]
        public AutoParams AutoParams => ParseAutoParams();

        private AutoParams ParseAutoParams()
        {
            var autoParams = new AutoParams();

            foreach (var param in AdParameters)
            {
                switch (param.Key)
                {
                    case "brand":
                        autoParams.Brand = param.ValueLong.ToString();
                        break;
                    case "cars_level_1":
                        autoParams.Model = param.ValueLong.ToString();
                        break;
                    case "regdate":
                        autoParams.Year = param.ValueLong.ToString();
                        break;
                    case "mileage":
                        autoParams.Mileage = param.Value.ToString();
                        break;
                    case "cars_engine":
                        autoParams.FuelType = param.ValueLong.ToString();
                        break;
                    case "cars_capacity":
                        autoParams.EngineCapacity = param.ValueLong.ToString();
                        break;
                    case "cars_gearbox":
                        autoParams.GearboxType = param.ValueLong.ToString();
                        break;
                    case "cars_type":
                        autoParams.BodyType = param.ValueLong.ToString();
                        break;
                    case "cars_drive":
                        autoParams.DriveType = param.ValueLong.ToString();
                        break;
                    case "region":
                        autoParams.Region = param.ValueLong.ToString();
                        break;
                    case "area":
                        autoParams.City = param.ValueLong.ToString();
                        break;
                }
            }

            return autoParams;
        }
    }
}
