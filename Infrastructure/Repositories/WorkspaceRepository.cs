using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PBL6.Common.Exceptions;
using PBL6.Domain.Data.Users;
using PBL6.Domain.Models.Users;
using PBL6.Infrastructure.Data;

namespace PBL6.Infrastructure.Repositories
{
    public class WorkspaceRepository : Repository<Workspace>, IWorkspaceRepository
    {
        public WorkspaceRepository(ApiDbContext context, ILogger logger)
            : base(context, logger) { }

        public async Task<WorkspaceRole> AddRoleAsync(Guid workspaceId, WorkspaceRole role)
        {
            role.WorkspaceId = workspaceId;
            foreach (var permission in role.Permissions)
            {
                permission.WorkspaceRoleId = role.Id;
            }

            return (await _apiDbContext.WorkspaceRoles.AddAsync(role)).Entity;
        }

        public async Task<bool> CheckIsExistAsync(Guid id)
        {
            return await _apiDbContext.Workspaces.AnyAsync(x => !x.IsDeleted && x.Id == id);
        }

        public async Task<bool> CheckIsExistRole(Guid workspaceId, Guid roleId)
        {
            if (!await CheckIsExistAsync(workspaceId))
            {
                throw new NotFoundException<Workspace>(workspaceId.ToString());
            }
            return await _apiDbContext.WorkspaceRoles.AnyAsync(
                x => x.Id == roleId && x.WorkspaceId == workspaceId
            );
        }

        public async Task<bool> CheckIsMemberAsync(Guid workspaceId, Guid userId)
        {
            if (!await CheckIsExistAsync(workspaceId))
            {
                throw new NotFoundException<Workspace>(workspaceId.ToString());
            }

            return await _apiDbContext.WorkspaceMembers.AnyAsync(
                x => !x.IsDeleted && x.WorkspaceId == workspaceId && x.UserId == userId
            );
        }

        public Task<bool> CheckIsOwnerAsync(Guid workspaceId, Guid userId)
        {
            return  _apiDbContext.Workspaces.AnyAsync(
                x => x.Id == workspaceId && x.OwnerId == userId
            );            
        }

        public async Task<WorkspaceMember> GetMemberByUserId(Guid workspaceId, Guid userId)
        {
            return await _apiDbContext.WorkspaceMembers.FirstOrDefaultAsync(
                x => !x.IsDeleted && x.WorkspaceId == workspaceId && x.UserId == userId
            );
        }

        public async Task<WorkspaceRole> GetRoleById(Guid workspaceId, Guid roleId)
        {
            return await _apiDbContext.WorkspaceRoles
                .Include(x => x.Permissions)
                .ThenInclude(x => x.Permission)
                .Include(x => x.Members)
                .ThenInclude(x => x.User)
                .ThenInclude(x => x.Information)
                .FirstOrDefaultAsync(x => x.Id == roleId && x.WorkspaceId == workspaceId);
        }

        public async Task<IEnumerable<WorkspaceRole>> GetRoles(Guid workspaceId)
        {
            await Task.CompletedTask;
            return _apiDbContext.WorkspaceRoles
                .Where(x => x.WorkspaceId == workspaceId && !x.IsDeleted)
                .Include(x => x.Members)
                .AsEnumerable();
        }
    }
}
