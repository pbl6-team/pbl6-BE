using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using PBL6.Application.Contract.Workspaces;
using PBL6.Common.Exceptions;
using PBL6.Common.Functions;

namespace PBL6.Application.Filters
{
    [AttributeUsage(AttributeTargets.All)]
    public class WorkspaceFilter : Attribute, IAsyncAuthorizationFilter
    {
        private readonly string _policyName;

        public WorkspaceFilter(string policyName)
        {
            _policyName = policyName;
        }

        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            var workspaceId = GetValue(context, "workspace-id");
            if (string.IsNullOrEmpty(workspaceId))
            {
                throw new Exception("workspace-id is required");
            }

            var workspaceService =
                context.HttpContext.RequestServices.GetService<IWorkspaceService>();

            var userId = context.HttpContext.User.Claims
                .FirstOrDefault(x => x.Type == CustomClaimTypes.UserId)
                ?.Value;
            var permissions = await workspaceService.GetPermissionOfUser(
                Guid.Parse(workspaceId),
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
