// using Microsoft.AspNetCore.Mvc;
// using Microsoft.AspNetCore.Mvc.Filters;
// using PBL6.Application.Contract.Workspaces;
// using PBL6.Common.Exceptions;

// namespace TaskManagement.API.Filters
// {
//     public class WorkspaceFilter : Attribute, IAsyncAuthorizationFilter
//     {
//         private readonly string _policyName;

//         public WorkspaceFilter(string policyName)
//         {
//             _policyName = policyName;
//         }

//         public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
//         {
//             if (context.HttpContext.Request.Headers.ContainsKey("workspaceId"))
//             {
//                 var workspaceId = context.HttpContext.Request.Headers["workspaceId"];
//                 var workspaceService = context.HttpContext.RequestServices.GetService<IWorkspaceService>();
//                 var workspace = await workspaceService.GetWorkspaceById(Guid.Parse(workspaceId));
//                 if (workspace == null)
//                 {
//                     throw new NotFoundException("Workspace not found");
//                 }
//                 var userId = context.HttpContext.User.Claims.FirstOrDefault(x => x.Type == "UserId")?.Value;
//                 var permission = await workspaceService.GetPermissionOfWorkspace(Guid.Parse(userId), Guid.Parse(workspaceId));
//                 if (permission == null)
//                 {
//                     throw new ForbiddenException("You don't have permission in this workspace");
//                 }
//                 if (permission.Permission == Permission.Admin || permission.Permission == Permission.Owner)
//                 {
//                     return;
//                 }
//                 context.Result = new ForbidResult();
//             }
//             else
//             {
//                 context.Result = new ForbidResult();
//             }
//         }
//     }
// }