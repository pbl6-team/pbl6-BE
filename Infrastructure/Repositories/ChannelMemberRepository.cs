using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PBL6.Domain.Data.Users;
using PBL6.Domain.Models.Users;
using PBL6.Infrastructure.Data;

namespace PBL6.Infrastructure.Repositories;

public class ChannelMemberRepository : Repository<ChannelMember>, IChannelMemberRepository
{
    public ChannelMemberRepository(ApiDbContext context, ILogger logger)
        : base(context, logger) { }

    public async Task<IEnumerable<ChannelPermission>> GetPermissionOfUser(
        Guid channelId,
        Guid userId
    )
    {
        return await _apiDbContext.ChannelMembers
            .Include(x => x.ChannelRole)
            .ThenInclude(x => x.Permissions)
            .ThenInclude(x => x.Permission)
            .Where(x => x.ChannelId == channelId && x.UserId == userId)
            .SelectMany(x => x.ChannelRole.Permissions)
            .Select(x => x.Permission)
            .ToListAsync();
    }

    public IQueryable<ChannelMember> GetMembers()
    {
        return _apiDbContext.ChannelMembers.Where(x => !x.IsDeleted)
            .Include(x => x.User)
            .ThenInclude(x => x.Information)
            .AsQueryable();
    }
}