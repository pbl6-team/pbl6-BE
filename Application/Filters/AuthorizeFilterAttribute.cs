using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using PBL6.Common.Exceptions;
using PBL6.Common.Functions;

namespace PBL6.Application.Filters
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
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
                    if (context.HttpContext.Request.Path.StartsWithSegments("/chathub"))
                    {
                        token = context.HttpContext.Request.Query["access_token"];
                    }
                    else
                    {
                        throw new UnauthorizedException("Token is required");
                    }
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
                var claims = jwtToken.Claims;
                var userId = claims.FirstOrDefault(x => x.Type == CustomClaimTypes.UserId)?.Value;
                var email = claims.FirstOrDefault(x => x.Type == CustomClaimTypes.Email)?.Value;
                var IsAdmin = claims.FirstOrDefault(x => x.Type == CustomClaimTypes.IsAdmin)?.Value;
                var isVerified = claims
                    .FirstOrDefault(x => x.Type == CustomClaimTypes.IsActive)
                    ?.Value;
                if (context.HttpContext.Request.Path.Value.Contains("verify-register"))
                {
                    if (isVerified == "True")
                    {
                        throw new UnauthorizedException("Account is verified");
                    }
                }
                else
                {
                    if (isVerified == "False")
                    {
                        throw new UnauthorizedException("Account is not verified");
                    }
                }
                var identity = new ClaimsIdentity(context.HttpContext.User.Identity);
                identity.AddClaim(new Claim(CustomClaimTypes.UserId, userId));
                identity.AddClaim(new Claim(CustomClaimTypes.Email, email));
                identity.AddClaim(new Claim(CustomClaimTypes.IsAdmin, IsAdmin));

                var principal = new ClaimsPrincipal(identity);
                context.HttpContext.User = principal;
                await Task.CompletedTask;
            }
            catch (Exception e)
            {
                if (e is UnauthorizedException)
                    throw;
                throw new UnauthorizedException("Invalid token");
            }
        }

        static string GetValue(AuthorizationFilterContext context, string key)
        {
            try
            {
                var apiKeyHeader = context.HttpContext.Request.Headers[key].ToString();
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
