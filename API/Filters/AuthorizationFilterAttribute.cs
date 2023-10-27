using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;

namespace PBL6.API.Filters
{
    [AttributeUsage(AttributeTargets.All)]
    public class AuthorizationFilterAttribute : Attribute, IAuthorizationFilter
    {
        private readonly string _apiKey;
        private readonly string _apiKeySecondary;
        private readonly bool _canUseSecondaryApiKey;

        public AuthorizationFilterAttribute(IConfiguration configuration)
        {
            _apiKey = configuration["SecretKeys:ApiKey"];
            _apiKeySecondary = configuration["SecretKeys:ApiKeySecondary"];
            _canUseSecondaryApiKey = configuration["SecretKeys:UseSecondaryKey"] == "True";
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var apiKeyHeader = context.HttpContext.Request.Headers["Authorization"].ToString();


            if (apiKeyHeader.Any())
            {
                var keys = new List<string>
                {
                    _apiKey
                };

                if (_canUseSecondaryApiKey)
                {
                    keys.AddRange(_apiKeySecondary.Split(','));
                }

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