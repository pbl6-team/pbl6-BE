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
            var apiKeyHeader = context.HttpContext.Request.Headers["x-apikey"].ToString();
            if(string.IsNullOrEmpty(apiKeyHeader))
                apiKeyHeader = context.HttpContext.Request.Query["x-apikey"].ToString();
            if (apiKeyHeader.Any())
            {
                var keys = new List<string>
                {
                    _apiKey
                };

                if (keys.FindIndex(x => x.Equals(apiKeyHeader, StringComparison.OrdinalIgnoreCase)) == -1)
                {
                    context.Result = new UnauthorizedResult();
                }
            }
            else
            {
                context.Result = new UnauthorizedResult();
            }
        }
    }
}