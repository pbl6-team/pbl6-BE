using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;

namespace PBL6.API.Filters
{
    [AttributeUsage(AttributeTargets.All)]
    public class ApiKeyFilterAttribute : Attribute, IAuthorizationFilter
    {
        private readonly string _apiKey;

        public ApiKeyFilterAttribute(IConfiguration configuration)
        {
            _apiKey = configuration["SecretKeys:ApiKey"];
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var apiKeyHeader = GetValue(context, "x-apikey");
            if (apiKeyHeader.Any())
            {
                var keys = new List<string> { _apiKey };

                if (
                    keys.FindIndex(x => x.Equals(apiKeyHeader, StringComparison.OrdinalIgnoreCase))
                    == -1
                )
                {
                    context.Result = new UnauthorizedResult();
                }
            }
            else
            {
                context.Result = new UnauthorizedResult();
            }
        }

        public static string GetValue(AuthorizationFilterContext context, string key)
        {
            try
            {
                var apiKeyHeader = context.HttpContext.Request.Headers[key].ToString();
                if (string.IsNullOrEmpty(apiKeyHeader))
                    apiKeyHeader = context.HttpContext.Request.Query[key].ToString();
                if (string.IsNullOrEmpty(apiKeyHeader))
                    apiKeyHeader = context.HttpContext.Request.Cookies[key].ToString();
                return apiKeyHeader;
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
