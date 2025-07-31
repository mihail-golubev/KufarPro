using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

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

            var initData = TelegramInitDataParser.Parse(initDataRaw!, botToken);

            if (initData?.User == null)
            {
                context.Result = new UnauthorizedObjectResult("Unauthorized: Invalid Telegram initData.");
                return;
            }

            httpContext.Items["ChatId"] = initData.User.Id;
        }
    }
}
