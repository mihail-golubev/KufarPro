using System.Text.Json.Serialization;

namespace AvKufarCarParser.Models.Kufar.API
{
    public class CarAd : Ad
    {
        [JsonIgnore]
        public CarParams CarParams => ParseCarParams();

        private CarParams ParseCarParams()
        {
            var carParams = new CarParams();

            foreach (var param in AdParameters)
            {
                switch (param.Key)
                {
                    case "brand":
                        carParams.Brand = param.ValueLong.ToString();
                        break;
                    case "cars_level_1":
                        carParams.Model = param.ValueLong.ToString();
                        break;
                    case "regdate":
                        carParams.Year = param.ValueLong.ToString();
                        break;
                    case "mileage":
                        carParams.Mileage = param.Value.ToString();
                        break;
                    case "cars_engine":
                        carParams.FuelType = param.ValueLong.ToString();
                        break;
                    case "cars_capacity":
                        carParams.EngineCapacity = param.ValueLong.ToString();
                        break;
                    case "cars_gearbox":
                        carParams.GearboxType = param.ValueLong.ToString();
                        break;
                    case "cars_type":
                        carParams.BodyType = param.ValueLong.ToString();
                        break;
                    case "cars_drive":
                        carParams.DriveType = param.ValueLong.ToString();
                        break;
                    case "region":
                        carParams.Region = param.ValueLong.ToString();
                        break;
                    case "area":
                        carParams.City = param.ValueLong.ToString();
                        break;
                }
            }

            return carParams;
        }
    }
}
