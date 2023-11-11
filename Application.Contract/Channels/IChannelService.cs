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
        Task<Guid> AddMemberToChannelAsync(Guid channelId, Guid userId);
        Task<Guid> RemoveMemberFromChannelAsync(Guid channelId, Guid userId);
    }
}