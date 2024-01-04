using Microsoft.AspNetCore.Mvc.Filters;
using PBL6.Application.Contract.Workspaces;
using PBL6.Common.Enum;
using PBL6.Common.Exceptions;
using PBL6.Common.Functions;

namespace PBL6.Application.Filters
{
    [AttributeUsage(AttributeTargets.All)]
    public class WorkspaceFilter : Attribute, IAsyncAuthorizationFilter
    {
        private readonly string _policyName = string.Empty;

        public WorkspaceFilter(string policyName)
        {
            _policyName = policyName;
        }

        public WorkspaceFilter()
        {
        }

        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            var workspaceId = GetValue(context, "workspace-id");
            if (string.IsNullOrEmpty(workspaceId))
            {
                throw new ForbidException("workspace-id is required");
            }

            var workspaceService =
                context.HttpContext.RequestServices.GetService<IWorkspaceService>();
            var workspace = await workspaceService.GetByIdAsync(Guid.Parse(workspaceId));
            if (workspace.Status == (short)WORKSPACE_STATUS.SUSPENDED)
            {
                throw new SuspendedWorkspaceException();
            }
            if (_policyName != string.Empty)
            {
                var userId = context.HttpContext.User.Claims
                    .FirstOrDefault(x => x.Type == CustomClaimTypes.UserId)
                    ?.Value;
                if (workspace.OwnerId == Guid.Parse(userId))
                {
                    return;
                }
                var permissions = await workspaceService.GetPermissionOfUser(
                    Guid.Parse(workspaceId),
                    Guid.Parse(userId)
                );
                if (!permissions.Any(x => x.Code == _policyName))
                {
                    throw new ForbidException();
                }
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
