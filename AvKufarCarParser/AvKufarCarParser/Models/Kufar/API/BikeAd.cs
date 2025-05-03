using System.Text.Json.Serialization;

namespace AvKufarCarParser.Models.Kufar.API
{
    public class BikeAd : Ad
    {
        [JsonIgnore]
        public BikeParams BikeParams => ParseBikeParams();

        private BikeParams ParseBikeParams()
        {
            var bikeParams = new BikeParams();

            foreach (var param in AdParameters)
            {
                switch (param.Key)
                {
                    case "bicycles_brand":
                        bikeParams.Brand = param.ValueLong.ToString();
                        break;
                    case "bicycles_class":
                        bikeParams.Class = param.ValueLong.ToString();
                        break;
                    case "bicycles_size":
                        bikeParams.Size = param.ValueLong.ToString();
                        break;
                    case "bicycles_wheels_size":
                        bikeParams.WheelSize = param.Value.ToString();
                        break;
                    case "bicycles_frame_size":
                        bikeParams.FrameSize = param.ValueLong.ToString();
                        break;
                    case "bicycles_frame_material":
                        bikeParams.FrameMaterial = param.ValueLong.ToString();
                        break;
                    case "condition":
                        bikeParams.Condition = param.ValueLong.ToString();
                        break;
                    case "region":
                        bikeParams.Region = param.ValueLong.ToString();
                        break;
                    case "area":
                        bikeParams.City = param.ValueLong.ToString();
                        break;
                    case "bicycles_woman":
                        bikeParams.IsWoman = param.Value.ToString().Equals("1");
                        break;
                    case "bicycles_electric":
                        bikeParams.IsElectric = param.Value.ToString().Equals("1");
                        break;
                }
            }

            return bikeParams;
        }
    }
}
