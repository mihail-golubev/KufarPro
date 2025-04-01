using AvKufarCarParser.Models;

namespace AvKufarCarParser
{
    public static class Util
    {
        public const long MikhailId = 850063324;
        public const long IlyaId = 849190529;
        public const long AlenaId = 769603864;

        public const string MikhailBotToken = "7938282959:AAHUtMKr1UqvsePTo48f8cNY4vUN82YOl80";
        public const string IlyaBotToken = "7401221219:AAGcFfwRghJq75JV_GByoHyIQF4H88E9S-8";
        public const string GetLink = "https://api.kufar.by/search-api/v2/search/rendered-paginated?cat=2010&cur=USD&prc=r%3A0%2C1500&rgn=2&size=10&sort=lst.d";

        public static LoginModel GetLoginModel() { 
            return new LoginModel() { Login = "03-minuet.mezzos@icloud.com", Password = "Password_1" }; 
        }
    }
}
