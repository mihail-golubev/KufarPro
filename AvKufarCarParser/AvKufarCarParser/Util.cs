using AvKufarCarParser.Models;

namespace AvKufarCarParser
{
    public static class Util
    {
        public const long UserId = 850063324;
        //public const long UserId = 849190529;
        public const string BotToken = "7938282959:AAHUtMKr1UqvsePTo48f8cNY4vUN82YOl80";
        //public const string BotToken = "7401221219:AAGcFfwRghJq75JV_GByoHyIQF4H88E9S-8";
        public const string GetLink = "https://api.kufar.by/search-api/v2/search/rendered-paginated?cat=2010&cbnd2=category_2010.mark_lada_vaz&cmp=0&cur=USD&lang=ru&prc=r%3A0%2C1200&rgn=2&size=10&sort=lst.d";

        public static LoginModel GetLoginModel() { 
            return new LoginModel() { Login = "03-minuet.mezzos@icloud.com", Password = "Password_1" }; 
        }
    }
}
