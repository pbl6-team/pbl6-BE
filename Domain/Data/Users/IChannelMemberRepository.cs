using PBL6.Domain.Models.Users;

namespace PBL6.Domain.Data.Users;

public interface IChannelMemberRepository : IRepository<ChannelMember>
{
    Task<IEnumerable<ChannelPermission>> GetPermissionOfUser(Guid channelId, Guid userId);
    IQueryable<ChannelMember> GetMembers();
}