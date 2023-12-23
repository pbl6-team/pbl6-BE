using PBL6.Application.Contract.Channels.Dtos;
using PBL6.Application.Contract.Users.Dtos;
using PBL6.Application.Contract.Workspaces.Dtos;

namespace PBL6.Application.Contract.Channels
{
    public interface IChannelService
    {
        Task<ChannelDto> GetByIdAsync(Guid channelId);
        Task<IEnumerable<ChannelDto>> GetByNameAsync(string channelName);
        Task<IEnumerable<ChannelDto>> GetAllChannelsOfAWorkspaceAsync(Guid workspaceId);
        Task<Guid> UpdateAsync(Guid channelId, UpdateChannelDto updateChannelDto);
        Task<Guid> AddAsync(CreateChannelDto createUpdateChannelDto);
        Task<Guid> DeleteAsync(Guid channelId);
        Task<Guid> AddMemberToChannelAsync(Guid channelId, List<Guid> userIds);
        Task<Guid> RemoveMemberFromChannelAsync(Guid channelId, List<Guid> userIds);
        Task<IEnumerable<ChannelRoleDto>> GetRolesAsync(Guid channelId);
        Task<IEnumerable<PermissionDto>> GetPermissionsAsync(Guid channelId, Guid roleId);
        Task<IEnumerable<PermissionDto>> GetPermissions();
        Task UpdateRoleAsync(Guid channelId, Guid roleId, CreateUpdateChannelRoleDto input);
        Task<Guid> AddRoleAsync(Guid channelId, CreateUpdateChannelRoleDto input);
        Task DeleteRoleAsync(Guid channelId, Guid roleId);
        Task SetRoleToUserAsync(Guid channelId, Guid userId, Guid? role);
        Task<ChannelRoleDto> GetRoleAsync(Guid channelId, Guid roleId);
        Task<IEnumerable<PermissionDto>> GetPermissionOfUser(Guid channelId, Guid userId);
        Task<IEnumerable<Guid>> GetChannelsOfUserAsync(Guid userId);
        Task<IEnumerable<UserDetailDto>> GetMembersByRoleIdAsync(Guid channelId, Guid roleId);
        Task<IEnumerable<UserDetailDto>> GetMembersWithoutRoleAsync(Guid channelId);
        Task<IEnumerable<UserNotInChannelDto>> GetMembersThatNotInTheChannel(Guid workspaceId, Guid channelId);
        Task<IEnumerable<ChannelUserDto>> GetMembersAsync(Guid channelId, short status = 1);
        Task AcceptInvitationAsync(Guid channelId);
        Task DeclineInvitationAsync(Guid channelId);
        Task<Guid> LeaveChannelAsync(Guid channelId);
        Task<Guid> TransferOwnershipAsync(Guid channelId, Guid userId);

    }
}