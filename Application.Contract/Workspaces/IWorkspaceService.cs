using Application.Contract.Users.Dtos;
using Application.Contract.Workspaces.Dtos;
using PBL6.Application.Contract.Workspaces.Dtos;

namespace PBL6.Application.Contract.Workspaces
{
    public interface IWorkspaceService
    {
        Task<bool> IsExistAsync(Guid workspaceId);
        Task<WorkspaceDto> GetByIdAsync(Guid workspaceId);
        Task<IEnumerable<WorkspaceDto>> GetByNameAsync(string workspaceName);
        Task<IEnumerable<WorkspaceDto>> GetAllAsync();
        Task<Guid> UpdateAsync(Guid workspaceId, UpdateWorkspaceDto updateWorkspaceDto);
        Task<Guid> UpdateAvatarAsync(Guid workspaceId, UpdateAvatarWorkspaceDto updateAvatarWorkspaceDto);
        Task<Guid> AddAsync(CreateWorkspaceDto createWorkspaceDto);
        Task<Guid> DeleteAsync(Guid workspaceId);
        Task<Guid> AddMemberToWorkspaceAsync(Guid workspaceId, List<Guid> userIds);
        Task<Guid> RemoveMemberFromWorkspaceAsync(Guid workspaceId, List<Guid> userIds);
        Task<IEnumerable<WorkspaceRoleDto>> GetRolesAsync(Guid workspaceId);
        Task UpdateRoleAsync(Guid workspaceId, Guid roleId, CreateUpdateWorkspaceRoleDto input);
        Task<Guid> AddRoleAsync(Guid workspaceId, CreateUpdateWorkspaceRoleDto input);
        Task<IEnumerable<PermissionDto>> GetPermissionsByWorkspaceRoleIdAsync(Guid workspaceId, Guid roleId);
        Task<IEnumerable<PermissionDto>> GetPermissions();
        Task DeleteRoleAsync(Guid workspaceId, Guid roleId);
        Task SetRoleAsync(Guid workspaceId, Guid userId, Guid? roleId);
        Task<WorkspaceRoleDto> GetRoleAsync(Guid workspaceId, Guid roleId);
        Task<IEnumerable<PermissionDto>> GetPermissionOfUser(Guid workspaceId, Guid userId);
        Task<IEnumerable<UserDto2>> GetMembersbyRoleIdAsync(Guid workspaceId, Guid roleId);
        Task<IEnumerable<UserDto2>> GetMembersWithoutRoleAsync(Guid workspaceId);
        Task<IEnumerable<WorkspaceUserDto>> GetMembersAsync(Guid workspaceId);
    }
}