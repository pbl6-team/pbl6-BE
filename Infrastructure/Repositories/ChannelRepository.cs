using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PBL6.Common.Enum;
using PBL6.Common.Exceptions;
using PBL6.Domain.Data.Users;
using PBL6.Domain.Models.Users;
using PBL6.Infrastructure.Data;

namespace PBL6.Infrastructure.Repositories
{
    public class ChannelRepository : Repository<Channel>, IChannelRepository
    {
        public ChannelRepository(ApiDbContext context, ILogger logger)
            : base(context, logger) { }

        public async Task<ChannelRole> AddRoleAsync(Guid channelId, ChannelRole role)
        {
            role.ChannelId = channelId;
            foreach (var permission in role.Permissions)
            {
                permission.ChannelRoleId = role.Id;
            }

            return (await _apiDbContext.ChannelRoles.AddAsync(role)).Entity;
        }

        public async Task<bool> CheckIsExistRole(Guid channelId, Guid roleId)
        {
            if (!await CheckIsExistAsync(channelId))
            {
                throw new NotFoundException<Channel>(channelId.ToString());
            }
            return await _apiDbContext.ChannelRoles.AnyAsync(
                x => x.Id == roleId && x.ChannelId == channelId
            );
        }

        public async Task<bool> CheckIsExistAsync(Guid channelId)
        {
            return await _apiDbContext.Channels.AnyAsync(x => !x.IsDeleted && x.Id == channelId);
        }

        public async Task<bool> CheckIsMemberAsync(Guid channelId, Guid userId)
        {
            if (!await CheckIsExistAsync(channelId))
            {
                throw new NotFoundException<Channel>(channelId.ToString());
            }

            return await _apiDbContext.ChannelMembers.AnyAsync(
                x => !x.IsDeleted && x.ChannelId == channelId 
                    && x.UserId == userId
                    && x.Status == ((short)CHANNEL_MEMBER_STATUS.ACTIVE)
            );
        }

        public async Task<ChannelMember> GetMemberByUserId(Guid channelId, Guid userId)
        {
            return await _apiDbContext.ChannelMembers.FirstOrDefaultAsync(
                x => !x.IsDeleted && x.ChannelId == channelId && x.UserId == userId
            );
        }

        public async Task<ChannelRole> GetRoleById(Guid channelId, Guid roleId)
        {
            return await _apiDbContext.ChannelRoles
                .Include(x => x.Permissions)
                .ThenInclude(x => x.Permission)
                .Include(x => x.Members)
                .ThenInclude(x => x.User)
                .ThenInclude(x => x.Information)
                .FirstOrDefaultAsync(x => x.Id == roleId && x.ChannelId == channelId);
        }

        public async Task<IEnumerable<ChannelRole>> GetRoles(Guid channelId)
        {
            await Task.CompletedTask;
            return _apiDbContext.ChannelRoles
                .Where(x => x.ChannelId == channelId && !x.IsDeleted)
                .Include(x => x.Members)
                .AsEnumerable();
        }

        public async Task<bool> CheckIsOwnerAsync(Guid channelId, Guid userId)
        {
            return await _apiDbContext.Channels.AnyAsync(
                x => x.Id == channelId && x.OwnerId == userId
            );
        }

        public Task<IEnumerable<ChannelPermission>> GetPermissionsOfUser(
            Guid channelId,
            Guid userId
        )
        {
            var query = _apiDbContext.Channels
                .Include(x => x.ChannelMembers)
                .ThenInclude(x => x.ChannelRole)
                .ThenInclude(x => x.Permissions)
                .ThenInclude(x => x.Permission)
                .Where(x => x.Id == channelId && x.ChannelMembers.Any(x => x.UserId == userId))
                .SelectMany(x => x.ChannelMembers.Select(x => x.ChannelRole))
                .SelectMany(x => x.Permissions)
                .Select(x => x.Permission);

            return Task.FromResult(query.AsEnumerable());
        }

        public async Task<IEnumerable<Guid>> GetUserIds(Guid channelId)
        {
            return await _dbSet.Include(x => x.ChannelMembers)
                .Where(x => x.Id == channelId)
                .SelectMany(x => x.ChannelMembers)
                .Select(m => m.UserId)
                .ToListAsync();
        }

        public Task<bool> CheckIsInvitedAsync(Guid channelId, Guid userId)
        {
            return _apiDbContext.ChannelMembers.AnyAsync(
                x => !x.IsDeleted && x.ChannelId == channelId 
                    && x.UserId == userId 
                    && x.Status == ((short)CHANNEL_MEMBER_STATUS.INVITED)
            );
        }
    }
}
