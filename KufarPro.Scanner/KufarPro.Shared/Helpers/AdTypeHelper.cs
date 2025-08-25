using KufarPro.Shared.Models.Ads;
using KufarPro.Shared.Models.HelperModels;

namespace KufarPro.Shared.Helpers
{
    public static class AdTypeHelper
    {
        private static readonly HashSet<string> _carAllowedKeys = new HashSet<string>()
        {
            "brand",
            "cars_level_1",
            "regdate",
            "mileage",
            "cars_engine",
            "cars_capacity",
            "cars_gearbox",
            "cars_type",
            "cars_drive"
        };

        private static readonly HashSet<string> _bicycleAllowedKeys = new HashSet<string>()
        {
            "bicycles_brand",
            "bicycles_class",
            "bicycles_size",
            "bicycles_wheels_size",
            "bicycles_frame_size",
            "bicycles_frame_material",
            "condition",
            "bicycles_woman",
            "bicycles_electric"
        };

        private static readonly Dictionary<int, AdType> _categoriesDictionary = new()
        {
            // Auto
            { 2010, AdType.Car },
            { 2030, AdType.Motorcycle },
            // Real Estate
            { 1010, AdType.Flat },
            { 1020, AdType.House },
            { 1030, AdType.Garage },
            // Other
            { 4050, AdType.Bicycle },
        };

        public static AdType GetAdType(int category)
        {
            return _categoriesDictionary.TryGetValue(category, out var type) ? type : AdType.Unknown;
        }

        public static BotType GetBotType(AdType adType)
        {
            // Auto
            if (adType == AdType.Car || adType == AdType.Motorcycle)
            {
                return BotType.Auto;
            }
            // Real estate
            else if (adType == AdType.Flat || adType == AdType.House || adType == AdType.Garage)
            {
                return BotType.RealEstate;
            }
            // Other
            else if (adType == AdType.Bicycle || adType == AdType.Other)
            {
                return BotType.Other;
            }
            else
            {
                return BotType.Unknown;
            }
        }

        public static HashSet<string> GetAllowedKeys(AdType adType)
        {
            switch (adType)
            {
                case AdType.Car:
                    return _carAllowedKeys;
                case AdType.Bicycle:
                    return _bicycleAllowedKeys;
                default:
                    return new HashSet<string>();
            }
        }
    }
}
