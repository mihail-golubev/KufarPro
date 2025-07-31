using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace KufarPro.Api.Auth
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
    public class ApiKeyAuthorizeAttribute : Attribute, IAsyncAuthorizationFilter
    {
        private const string API_KEY_HEADER_NAME = "X-API-Key";
        private const string PARAM_NAME = "ApiKeyEnvVariableName";

        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            var httpContext = context.HttpContext;
            var config = httpContext.RequestServices.GetService<IConfiguration>();

            var providedKey = httpContext.Request.Headers[API_KEY_HEADER_NAME].FirstOrDefault();
            var validKey = Environment.GetEnvironmentVariable(config[PARAM_NAME]);

            if (string.IsNullOrEmpty(validKey) || providedKey != validKey)
            {
                context.Result = new UnauthorizedObjectResult("Invalid API Key.");
                return;
            }
        }
    }
}
