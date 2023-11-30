using Application.Contract.Users.Dtos;
using PBL6.Application.Contract.Users.Dtos;

namespace PBL6.Application.Contract.Users
{
    public interface IUserService
    {
        Task<IEnumerable<UserDto2>> GetByWorkspaceIdAsync(Guid workspaceId);
        Task<IEnumerable<UserDto2>> GetByChannelIdAsync(Guid channelId);
        Task<Guid> UpdateAsync(Guid userId, UpdateUserDto updateUserDto);
        Task<Guid> UpdateAvatarAsync(Guid userId, UpdateUserPictureDto updateUserPictureDto);
    }
}