using Microsoft.AspNetCore.Http;
using PBL6.Application.Contract.Workspaces.Dtos;

namespace PBL6.Application.Contract.Workspaces
{
    public interface IWorkspaceService
    {
        Task<WorkspaceDto> GetByIdAsync(Guid workspaceId);
        Task<IEnumerable<WorkspaceDto>> GetByNameAsync(string workspaceName);
        Task<IEnumerable<WorkspaceDto>> GetAllAsync();
        Task<Guid> UpdateAsync(Guid workspaceId, UpdateWorkspaceDto updateWorkspaceDto);
        Task<Guid> UpdateAvatarAsync(Guid workspaceId, UpdateAvatarWorkspaceDto updateAvatarWorkspaceDto);
        Task<Guid> AddAsync(CreateWorkspaceDto createWorkspaceDto);
        Task<Guid> DeleteAsync(Guid workspaceId);
        Task<Guid> AddMemberToWorkspaceAsync(Guid workspaceId, Guid userId);
        Task<Guid> RemoveMemberFromWorkspaceAsync(Guid workspaceId, Guid userId);
    }
}