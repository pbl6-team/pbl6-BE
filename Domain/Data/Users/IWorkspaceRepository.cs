using PBL6.Domain.Models.Users;

namespace PBL6.Domain.Data.Users 
{
    public interface IWorkspaceRepository : IRepository<Workspace>
    {
        Task<Workspace> GetAsync(Guid id, short status = 1);
        Task<bool> CheckIsExistAsync(Guid id);
        Task<bool> CheckIsMemberAsync(Guid workspaceId, Guid userId);
        Task<bool> CheckIsInvitedAsync(Guid workspaceId, Guid userId);
        Task<bool> CheckIsExistRole(Guid workspaceId, Guid roleId);
        Task<IEnumerable<WorkspaceRole>> GetRoles(Guid workspaceId);
        Task<WorkspaceRole> AddRoleAsync(Guid workspaceId, WorkspaceRole role);
        Task<WorkspaceRole> GetRoleById(Guid workspaceId, Guid roleId);
        Task<WorkspaceMember> GetMemberByUserId(Guid workspaceId, Guid userId);
        Task<bool> CheckIsOwnerAsync(Guid workspaceId, Guid userId);
        IQueryable<Workspace> GetWorkspaces(short status = 1);
        IQueryable<Workspace> GetWorkspacesWithMembers();
    }
}