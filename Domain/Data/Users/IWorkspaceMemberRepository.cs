using PBL6.Domain.Models.Users;

namespace PBL6.Domain.Data.Users;

public interface IWorkspaceMemberRepository : IRepository<WorkspaceMember>
{
    Task<WorkspaceRole> GetRole(Guid workspaceId, Guid userId);
    Task<IEnumerable<WorkspacePermission>> GetPermissionOfUser(Guid workspaceId, Guid userId);
}