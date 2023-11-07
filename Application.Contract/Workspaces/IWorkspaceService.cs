using Microsoft.AspNetCore.Http;
using PBL6.Application.Contract.Workspaces.Dtos;

namespace PBL6.Application.Contract.Workspaces
{
    public interface IWorkspaceService
    {
        Task<WorkspaceDto> GetByIdAsync(Guid id);
        Task<IEnumerable<WorkspaceDto>> GetByNameAsync(string name);
        Task<IEnumerable<WorkspaceDto>> GetAllAsync();
        Task<WorkspaceDto> UpdateAsync(Guid id, CreateUpdateWorkspaceDto workspaceDto);
        Task<WorkspaceDto> UpdateAvatarAsync(Guid id, IFormFile workspaceDto);
        Task<Guid> AddAsync(CreateUpdateWorkspaceDto workspaceDto);
        Task<WorkspaceDto> DeleteAsync(Guid id);
    }
}