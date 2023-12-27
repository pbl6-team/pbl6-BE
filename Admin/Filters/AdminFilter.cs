using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Application.Contract.Admins;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.IdentityModel.Tokens;
using PBL6.Common;
using PBL6.Common.Enum;
using PBL6.Common.Exceptions;
using PBL6.Common.Functions;

namespace PBL6.Admin.Filters
{
    [AttributeUsage(AttributeTargets.All)]
    public class AdminFilter : Attribute, IAsyncAuthorizationFilter
    {
        private readonly bool _rootRequired = false;

        public AdminFilter(bool rootRequired = false)
        {
            _rootRequired = rootRequired;
        }
        
        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            var configuration = context.HttpContext.RequestServices.GetService<IConfiguration>();
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
            var isAdmin = claims.FirstOrDefault(x => x.Type == CustomClaimTypes.IsActive)?.Value;
            var isRoot = claims.FirstOrDefault(x => x.Type == CustomClaimTypes.Username)?.Value == "root"; 
            if (isAdmin != "True")
            {
                throw new UnauthorizedException("You are not admin");
            }
            var adminService = context.HttpContext.RequestServices.GetService<IAdminService>();

            var admin = await adminService.GetByIdAsync(Guid.Parse(userId));
            if (admin.Status == (short)ADMIN_STATUS.BLOCKED)
            {
                throw new BlockedUserException();
            }
            
            if (_rootRequired && !isRoot)
            {
                throw new UnauthorizedException("You are not root");
            }

            var identity = new ClaimsIdentity(context.HttpContext.User.Identity);
            identity.AddClaim(new Claim(CustomClaimTypes.UserId, userId));
            identity.AddClaim(new Claim(CustomClaimTypes.Email, email));
            identity.AddClaim(new Claim(CustomClaimTypes.IsAdmin, isAdmin));
            identity.AddClaim(new Claim(CustomClaimTypes.IsRoot, isRoot.ToString()));

            var principal = new ClaimsPrincipal(identity);
            context.HttpContext.User = principal;
            await Task.CompletedTask;
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
