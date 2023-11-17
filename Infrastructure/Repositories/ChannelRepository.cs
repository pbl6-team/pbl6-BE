using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
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

        private async Task<bool> CheckIsExistAsync(Guid channelId)
        {
            return await _apiDbContext.Channels.AnyAsync(x => x.Id == channelId);
        }

        public async Task<bool> CheckIsMemberAsync(Guid channelId, Guid userId)
        {
            if (!await CheckIsExistAsync(channelId))
            {
                throw new NotFoundException<Channel>(channelId.ToString());
            }

            return await _apiDbContext.ChannelMembers.AnyAsync(
                x => x.ChannelId == channelId && x.UserId == userId
            );
        }

        public async Task<ChannelMember> GetMemberByUserId(Guid channelId, Guid userId)
        {
            return await _apiDbContext.ChannelMembers.FirstOrDefaultAsync(
                x => x.ChannelId == channelId && x.UserId == userId
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
                .AsEnumerable();
        }
    }
}