using PBL6.Domain.Models.Users;

namespace PBL6.Domain.Data.Users 
{
    public interface IChannelRepository : IRepository<Channel>
    {
        Task<ChannelRole> AddRoleAsync(Guid channelId, ChannelRole role);
        Task<bool> CheckIsExistRole(Guid channelId, Guid roleId);
        Task<bool> CheckIsMemberAsync(Guid channelId, Guid guid);
        Task<ChannelMember> GetMemberByUserId(Guid channelId, Guid userId);
        Task<ChannelRole> GetRoleById(Guid channelId, Guid roleId);
        Task<IEnumerable<ChannelRole>> GetRoles(Guid channelId);
    }
}