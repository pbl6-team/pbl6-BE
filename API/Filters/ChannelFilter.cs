using Microsoft.AspNetCore.Mvc.Filters;
using PBL6.Application.Contract.Channels;
using PBL6.Common.Exceptions;
using PBL6.Common.Functions;

namespace PBL6.API.Filters
{
    [AttributeUsage(AttributeTargets.All)]
    public class ChannelFilter : Attribute, IAsyncAuthorizationFilter
    {
        private readonly string _policyName;

        public ChannelFilter(string policyName)
        {
            _policyName = policyName;
        }

        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            var channelId = GetValue(context, "channel-id");
            if (string.IsNullOrEmpty(channelId))
            {
                throw new Exception("channel-id is required");
            }

            var channelService =
                context.HttpContext.RequestServices.GetService<IChannelService>();

            var userId = context.HttpContext.User.Claims
                .FirstOrDefault(x => x.Type == CustomClaimTypes.UserId)
                ?.Value;
            var permissions = await channelService.GetPermissionOfUser(
                Guid.Parse(channelId),
                Guid.Parse(userId)
            );
            if (!permissions.Any(x => x.Code == _policyName))
            {
                throw new ForbidException();
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
