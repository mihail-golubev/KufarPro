using KufarPro.Api.Auth.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Newtonsoft.Json;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace KufarPro.Api.Auth
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
    public class TelegramAuthorizeAttribute : Attribute, IAsyncAuthorizationFilter
    {
        private const string INIT_DATA_HEADER = "X-Telegram-InitData";
        private const string BOT_TYPE_HEADER = "X-Bot-Type";

        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            var httpContext = context.HttpContext;

            if (!httpContext.Request.Headers.TryGetValue(INIT_DATA_HEADER, out var initDataRaw) ||
                !httpContext.Request.Headers.TryGetValue(BOT_TYPE_HEADER, out var botNameRaw))
            {
                context.Result = new UnauthorizedObjectResult("Unauthorized: Missing required headers.");
                return;
            }

            string botName = botNameRaw.ToString();
            string envVarName = $"{botName.ToUpper()}_KUFAR_PRO_BOT_TOKEN";
            string botToken = Environment.GetEnvironmentVariable(envVarName);

            if (string.IsNullOrEmpty(botToken))
            {
                context.Result = new UnauthorizedObjectResult("Unauthorized: Unknown bot.");
                return;
            }

            var initData = ParseTelegramData(initDataRaw!, botToken);

            if (initData?.User == null)
            {
                context.Result = new UnauthorizedObjectResult("Unauthorized: Invalid Telegram initData.");
                return;
            }

            httpContext.Items["ChatId"] = initData.User.Id;
        }

        private TelegramInitData ParseTelegramData(string initData, string botToken)
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
