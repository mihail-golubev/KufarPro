using KufarPro.Api.Auth.Models;
using Newtonsoft.Json;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace KufarPro.Api.Auth
{
    public class TelegramInitDataParser
    {
        public static TelegramInitData Parse(string initData, string botToken)
        {
            var result = new TelegramInitData();
            var data = HttpUtility.UrlDecode(initData);

            if (string.IsNullOrEmpty(data))
            {
                return null;
            }

            var keyValuePairs = data.Split('&')
                .Select(p => p.Split('='))
                .Where(p => p.Length == 2)
                .ToDictionary(p => p[0], p => p[1]);

            if (!keyValuePairs.TryGetValue("hash", out var hash))
            {
                return null;
            }

            var dataCheckString = string.Join('\n', keyValuePairs
                .Where(p => p.Key != "hash")
                .OrderBy(p => p.Key)
                .Select(p => $"{p.Key}={p.Value}"));

            var secretKey = SHA256.HashData(Encoding.UTF8.GetBytes(botToken));
            using var hmac = new HMACSHA256(secretKey);
            var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(dataCheckString));
            var computedHashHex = BitConverter.ToString(computedHash).Replace("-", "").ToLower();

            if (!computedHashHex.Equals(hash, StringComparison.CurrentCultureIgnoreCase))
            {
                return null;
            }

            if (!keyValuePairs.TryGetValue("user", out var userJson))
            {
                return null;
            }

            result.User = JsonConvert.DeserializeObject<TelegramUser>(
                userJson,
                new JsonSerializerSettings
                {
                    Error = (sender, args) => { args.ErrorContext.Handled = true; },
                    MissingMemberHandling = MissingMemberHandling.Error
                });
            result.Hash = hash;

            return result;
        }
    }
}
