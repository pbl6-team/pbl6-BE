using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.IdentityModel.Tokens;
using PBL6.Common.Exceptions;

namespace PBL6.API.Filters
{
    [AttributeUsage(AttributeTargets.All)]
    public class AuthorizeFilter : Attribute, IAsyncAuthorizationFilter
    {
        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            try
            {
                var configuration =
                    context.HttpContext.RequestServices.GetService<IConfiguration>();
                var token = GetValue(context, "Authorization");
                if (string.IsNullOrEmpty(token))
                {
                    throw new UnauthorizedException("Token is required");
                }

                if (token.ToString().StartsWith("Bearer "))
                {
                    token = token.ToString()[7..];
                }

                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.ASCII.GetBytes(configuration["Jwt:SecretKey"]);
                tokenHandler.ValidateToken(
                    token,
                    new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(key),
                        ValidateIssuer = false,
                        ValidateAudience = false,
                    },
                    out SecurityToken validatedToken
                );

                var jwtToken =
                    (JwtSecurityToken)validatedToken
                    ?? throw new UnauthorizedException("Invalid token");
                await Task.CompletedTask;
            }
            catch (Exception)
            {
                throw new UnauthorizedException("Invalid token");
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
