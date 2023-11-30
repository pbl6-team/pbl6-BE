using PBL6.Application.Contract.Channels.Dtos;
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
        Task SetRoleToUserAsync(Guid channelId, Guid userId, Guid role);
        Task<ChannelRoleDto> GetRoleAsync(Guid channelId, Guid roleId);
    }
}