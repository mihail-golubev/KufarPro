using System.Text.Json.Serialization;

namespace KufarPro.Shared.Models.Ads
{
    public class BicycleAd : Ad
    {
        [JsonIgnore]
        public BicycleParams BicycleParams => ParseBicycleParams();

        private BicycleParams ParseBicycleParams()
        {
            var bicycleParams = new BicycleParams();

            foreach (var param in AdParameters)
            {
                switch (param.Key)
                {
                    case "bicycles_brand":
                        bicycleParams.Brand = param.ValueLong.ToString();
                        break;
                    case "bicycles_class":
                        bicycleParams.Class = param.ValueLong.ToString();
                        break;
                    case "bicycles_size":
                        bicycleParams.Size = param.ValueLong.ToString();
                        break;
                    case "bicycles_wheels_size":
                        bicycleParams.WheelSize = param.Value.ToString();
                        break;
                    case "bicycles_frame_size":
                        bicycleParams.FrameSize = param.ValueLong.ToString();
                        break;
                    case "bicycles_frame_material":
                        bicycleParams.FrameMaterial = param.ValueLong.ToString();
                        break;
                    case "condition":
                        bicycleParams.Condition = param.ValueLong.ToString();
                        break;
                    case "region":
                        bicycleParams.Region = param.ValueLong.ToString();
                        break;
                    case "area":
                        bicycleParams.City = param.ValueLong.ToString();
                        break;
                    case "bicycles_woman":
                        bicycleParams.IsWoman = param.Value.ToString().Equals("1");
                        break;
                    case "bicycles_electric":
                        bicycleParams.IsElectric = param.Value.ToString().Equals("1");
                        break;
                }
            }

            return bicycleParams;
        }
    }
}
