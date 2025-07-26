using KufarPro.Models.Kufar.API;
using KufarPro.Models.Kufar.HelperModels;

namespace KufarPro.Helpers
{
    public static class AppHelper
    {
        //public const long MikhailId = 850063324;
        //public const long IlyaId = 849190529;
        //public const long AlenaId = 769603864;
        //public const long MaksId = 651255982;

        //public const string IlyaBotToken = "7401221219:AAGcFfwRghJq75JV_GByoHyIQF4H88E9S-8";
        public const string BaseKufarGetLink = "https://api.kufar.by/search-api/v2/search/rendered-paginated?sort=lst.d&size=10&cur=USD";
        //public const string GetAutoLink = "https://api.kufar.by/search-api/v2/search/rendered-paginated?cat=2010&cur=USD&prc=r%3A0%2C1500&rgn=2&size=10&sort=lst.d";
        //public const string GetBicycleLink = "https://api.kufar.by/search-api/v2/search/rendered-paginated?bcys=v.or%3A1&btc=1&bws=v.or%3A7%2C9%2C11&cat=4050&cmp=0&rgn=2&prn=4000&size=10&lang=ru&sort=lst.d";

        public static AdType GetAdType(string urlQuery)
        {
            var queryParams = System.Web.HttpUtility.ParseQueryString(urlQuery);
            var cat = queryParams["cat"];

            return cat switch
            {
                "2010" => AdType.Auto,
                "4050" => AdType.Bicycle,
                "1020" => AdType.RealEstate,
                _ => AdType.Unknown
            };
        }

        public static string GetNotifyMessage(AutoAd ad)
        {
            string message = $"Вышло новое объявление в {ad.ListTime:HH:mm dd/MM/yyyy}!" +
                $"\n\nНазвание автомобиля: {ad.AutoParams.Brand} {ad.AutoParams.Model}" +
                $"\nЦена: ${ad.PriceUsd}" +
                $"\nОбласть: {ad.AutoParams.Region}" +
                $"\nГород: {ad.AutoParams.City}" +
                $"\nСсылка на объявление: {ad.Link}" +
                $"\n\nХарактеристики автомобиля:" +
                $"\nДвигатель: {ad.AutoParams.FuelType} {ad.AutoParams.EngineCapacity}" +
                $"\nПробег: {ad.AutoParams.Mileage} км" +
                $"\nГод выпуска: {ad.AutoParams.Year}" +
                $"\nКоробка передач: {ad.AutoParams.GearboxType}" +
                $"\nТип кузова: {ad.AutoParams.BodyType}" +
                $"\nПривод: {ad.AutoParams.DriveType}";

            return message;
        }

        public static string GetNotifyMessage(BicycleAd ad)
        {
            string message = $"Вышло новое объявление в {ad.ListTime:HH:mm dd/MM/yyyy}!" +
                $"\n\nБренд велосипеда: {ad.BicycleParams.Brand}" +
                $"\nЦена: {ad.PriceByn} р." +
                $"\nОбласть: {ad.BicycleParams.Region}" +
                $"\nГород: {ad.BicycleParams.City}" +
                $"\nСсылка на объявление: {ad.Link}" +
                $"\n\nХарактеристики велосипеда:" +
                $"\nМатериал рамы: {ad.BicycleParams.FrameMaterial}" +
                $"\nДиаметр колес: {ad.BicycleParams.WheelSize} км" +
                $"\nРазмер рамы: {ad.BicycleParams.FrameSize}" +
                $"\nДля кого: {ad.BicycleParams.Size}" +
                $"\nСостояние: {ad.BicycleParams.Condition}";

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
