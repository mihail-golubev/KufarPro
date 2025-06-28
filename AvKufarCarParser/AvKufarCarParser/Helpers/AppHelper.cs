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
        public const string BaseKufarGetLink = "https://api.kufar.by/search-api/v2/search/rendered-paginated?sort=lst.d&size=10&cur=USD";
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

        public static AdType GetAdType(string urlQuery)
        {
            var queryParams = System.Web.HttpUtility.ParseQueryString(urlQuery);
            var cat = queryParams["cat"];

            return cat switch
            {
                "2010" => AdType.Car,
                "4050" => AdType.Bike,
                "1020" => AdType.Estate,
                _ => AdType.Unknown
            };
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

        public static string ParseFilterParameters(string messageText)
        {
            var parts = messageText.Split(' ').Skip(1);

            var parameters = new List<KeyValuePair<string, string>>();

            foreach (var part in parts)
            {
                var kv = part.Split('=');
                if (kv.Length == 2)
                {
                    parameters.Add(new KeyValuePair<string, string>(kv[0], kv[1]));
                }
            }

            var sorted = parameters
                .OrderBy(p => p.Key)
                .Select(p => $"{p.Key}={p.Value}");

            return string.Join("&", sorted);
        }

        public static string BuildQuery(UserFilterState state)
        {
            var parameters = new List<string>();

            if (!string.IsNullOrEmpty(state.Category))
            {
                parameters.Add($"cat={state.Category}");
            }

            if (!string.IsNullOrEmpty(state.Region))
            {
                parameters.Add($"rgn={state.Region}");
            }

            if (state.PriceRange.From.HasValue && state.PriceRange.To.HasValue)
            {
                parameters.Add($"prc=r:{state.PriceRange.From},{state.PriceRange.To}");
            }
            else if (state.PriceRange.From.HasValue)
            {
                parameters.Add($"prc=r:{state.PriceRange.From},");
            }
            else if (state.PriceRange.To.HasValue)
            {
                parameters.Add($"prc=r:, {state.PriceRange.To}");
            }

            return string.Join("&", parameters);
        }

        public static PriceRange ParsePriceRange(string text)
        {
            var priceRange = new PriceRange { From = null, To = null };

            if (string.IsNullOrWhiteSpace(text))
            {
                return priceRange;
            }

            text = text.Trim().Replace("–", "-");
            var parts = text.Split('-', StringSplitOptions.None);

            if (parts.Length == 2)
            {
                if (int.TryParse(parts[0].Trim(), out int from) && int.TryParse(parts[1].Trim(), out int to))
                {
                    priceRange.From = from;
                    priceRange.To = to;
                }
                else if (int.TryParse(parts[0].Trim(), out from) && string.IsNullOrWhiteSpace(parts[1]))
                {
                    priceRange.From = from;
                    priceRange.To = int.MaxValue;
                }
                else if (string.IsNullOrWhiteSpace(parts[0]) && int.TryParse(parts[1].Trim(), out to))
                {
                    priceRange.From = 0;
                    priceRange.To = to;
                }
            }
            else if (parts.Length == 1)
            {
                if (int.TryParse(parts[0].Trim(), out int value))
                {
                    priceRange.From = value;
                    priceRange.To = value;
                }
            }

            return priceRange;
        }
    }
}
