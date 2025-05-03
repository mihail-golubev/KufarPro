using AvKufarCarParser.Models.Database;
using AvKufarCarParser.Models.Kufar.API;
using AvKufarCarParser.Models.Kufar.HelperModels;

namespace AvKufarCarParser.Helpers
{
    public static class AppHelper
    {
        public const long MikhailId = 850063324;
        public const long IlyaId = 849190529;
        public const long AlenaId = 769603864;
        public const long MaksId = 651255982;

        public const string IlyaBotToken = "7401221219:AAGcFfwRghJq75JV_GByoHyIQF4H88E9S-8";
        public const string GetCarLink = "https://api.kufar.by/search-api/v2/search/rendered-paginated?cat=2010&cur=USD&prc=r%3A0%2C1500&rgn=2&size=10&sort=lst.d";
        public const string GetBikeLink = "https://api.kufar.by/search-api/v2/search/rendered-paginated?bcys=v.or%3A1&btc=1&bws=v.or%3A7%2C9%2C11&cat=4050&cmp=0&rgn=2&prn=4000&size=10&lang=ru&sort=lst.d";

        public const string DbConnectionString = "mongodb://localhost:27017";
        public const string DbName = "AvKufarCarParserDb";
        public const string CollectionName = "searchFilters";

        public const long MaxFileSize = 512 * 1024;

        public static LoginModel GetLoginModel()
        {
            return new LoginModel() { Login = "03-minuet.mezzos@icloud.com", Password = "Password_1" };
        }

        public static AdType GetAdType(List<FilterParameter> parameters)
        {
            var categoryParameter = parameters.FirstOrDefault(x => x.QueryName == "cat");

            if (categoryParameter != null)
            {
                return categoryParameter.Value switch
                {
                    "2010" => AdType.Car,
                    "4050" => AdType.Bike,
                    "1020" => AdType.Estate,
                    _ => AdType.Unknown,
                };
            }
            else
            {
                return AdType.Unknown;
            }
        }

        public static string GetNotifyMessage(CarAd ad)
        {
            string message = $"Вышло новое объявление в {ad.ListTime:HH:mm dd/MM/yyyy}!" +
                $"\n\nНазвание автомобиля: {ad.CarParams.Brand} {ad.CarParams.Model}" +
                $"\nЦена: ${ad.PriceUsd}" +
                $"\nОбласть: {ad.CarParams.Region}" +
                $"\nГород: {ad.CarParams.City}" +
                $"\nСсылка на объявление: {ad.Link}" +
                $"\n\nХарактеристики автомобиля:" +
                $"\nДвигатель: {ad.CarParams.FuelType} {ad.CarParams.EngineCapacity}" +
                $"\nПробег: {ad.CarParams.Mileage} км" +
                $"\nГод выпуска: {ad.CarParams.Year}" +
                $"\nКоробка передач: {ad.CarParams.GearboxType}" +
                $"\nТип кузова: {ad.CarParams.BodyType}" +
                $"\nПривод: {ad.CarParams.DriveType}";

            return message;
        }

        public static string GetNotifyMessage(BikeAd ad)
        {
            string message = $"Вышло новое объявление в {ad.ListTime:HH:mm dd/MM/yyyy}!" +
                $"\n\nБренд велосипеда: {ad.BikeParams.Brand}" +
                $"\nЦена: {ad.PriceByn} р." +
                $"\nОбласть: {ad.BikeParams.Region}" +
                $"\nГород: {ad.BikeParams.City}" +
                $"\nСсылка на объявление: {ad.Link}" +
                $"\n\nХарактеристики велосипеда:" +
                $"\nМатериал рамы: {ad.BikeParams.FrameMaterial}" +
                $"\nДиаметр колес: {ad.BikeParams.WheelSize} км" +
                $"\nРазмер рамы: {ad.BikeParams.FrameSize}" +
                $"\nДля кого: {ad.BikeParams.Size}" +
                $"\nСостояние: {ad.BikeParams.Condition}";

            return message;
        }

        public static string GetNotifyMessage(Ad ad)
        {
            string message = $"Вышло новое объявление в {ad.ListTime:HH:mm dd/MM/yyyy}!" +
                $"\n\nНазвание объявления: {ad.Subject}" +
                $"\nЦена: {ad.PriceByn} р." +
                $"\nСсылка на объявление: {ad.Link}";

            return message;
        }

        public static List<FilterParameter> ParseFilterParameters(string messageText)
        {
            var parameters = new List<FilterParameter>();
            var parts = messageText.Split(' ').Skip(1);

            foreach (var part in parts)
            {
                var kv = part.Split('=');
                if (kv.Length == 2)
                {
                    parameters.Add(new FilterParameter { QueryName = kv[0], Value = kv[1] });
                }
            }

            return parameters;
        }
    }
}
