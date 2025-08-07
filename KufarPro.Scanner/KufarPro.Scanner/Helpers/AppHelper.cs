using KufarPro.Shared.Models.Ads;
using KufarPro.Shared.Models.HelperModels;
using KufarPro.Shared.Models.Search;

namespace KufarPro.Scanner.Helpers
{
    public static class AppHelper
    {
        //public const long MikhailId = 850063324;
        //public const long IlyaId = 849190529;
        //public const long AlenaId = 769603864;
        //public const long MaksId = 651255982;

        //public const string IlyaBotToken = "7401221219:AAGcFfwRghJq75JV_GByoHyIQF4H88E9S-8";
        //public const string BaseKufarGetLink = "https://api.kufar.by/search-api/v2/search/rendered-paginated?sort=lst.d&size=10&cur=USD";
        //public const string GetAutoLink = "https://api.kufar.by/search-api/v2/search/rendered-paginated?cat=2010&cur=USD&prc=r%3A0%2C1500&rgn=2&size=10&sort=lst.d";
        //public const string GetBicycleLink = "https://api.kufar.by/search-api/v2/search/rendered-paginated?bcys=v.or%3A1&btc=1&bws=v.or%3A7%2C9%2C11&cat=4050&cmp=0&rgn=2&prn=4000&size=10&lang=ru&sort=lst.d";
        public const string BaseKufarQuery = "sort=lst.d&size=10&cur=USD&";

        public static AdType GetAdType(string urlQuery)
        {
            var queryParams = System.Web.HttpUtility.ParseQueryString(urlQuery);
            var cat = queryParams["cat"];

            return cat switch
            {
                "2010" => AdType.Car,
                "4050" => AdType.Bicycle,
                "1020" => AdType.House,
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

        public static List<SearchFilter> AddOrUpdate(this List<SearchFilter> searchFilters, SearchFilter newFilter)
        {
            var existingFilter = searchFilters.FirstOrDefault(filter => filter.UrlQuery == newFilter.UrlQuery);

            if (existingFilter != null)
            {
                foreach (var chatId in newFilter.ChatIds)
                {
                    if (!existingFilter.ChatIds.Contains(chatId))
                    {
                        existingFilter.ChatIds.Add(chatId);
                    }
                }
            }
            else
            {
                searchFilters.Add(newFilter);
            }

            return searchFilters;
        }
    }
}
