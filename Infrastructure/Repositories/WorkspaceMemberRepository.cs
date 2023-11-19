using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PBL6.Domain.Data.Users;
using PBL6.Domain.Models.Users;
using PBL6.Infrastructure.Data;

namespace PBL6.Infrastructure.Repositories;

public class WorkspaceMemberRepository : Repository<WorkspaceMember>, IWorkspaceMemberRepository
{
    public WorkspaceMemberRepository(ApiDbContext context, ILogger logger)
        : base(context, logger) { }

    public async Task<WorkspaceRole> GetRole(Guid workspaceId, Guid userId)
    {
        return await _apiDbContext.WorkspaceMembers
            .Include(x => x.WorkspaceRole)
            .ThenInclude(x => x.Permissions)
            .Where(x => x.WorkspaceId == workspaceId && x.UserId == userId)
            .Select(x => x.WorkspaceRole)
            .FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<WorkspacePermission>> GetPermissionOfUser(Guid workspaceId, Guid userId)
    {
        return await _apiDbContext.WorkspaceMembers
            .Include(x => x.WorkspaceRole)
            .ThenInclude(x => x.Permissions)
            .ThenInclude(x => x.Permission)
            .Where(x => x.WorkspaceId == workspaceId && x.UserId == userId)
            .SelectMany(x => x.WorkspaceRole.Permissions)
            .Select(x => x.Permission)
            .AsNoTracking()
            .ToListAsync();
    }
}
