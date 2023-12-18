using PBL6.Application.Contract.Users.Dtos;

namespace PBL6.Application.Contract.Users
{
    public interface IUserService
    {
        Task<IEnumerable<UserDetailDto>> GetByWorkspaceIdAsync(Guid workspaceId);
        Task<IEnumerable<UserDetailDto>> GetByChannelIdAsync(Guid channelId);
        Task<IEnumerable<UserDetailDto>> SearchUserAsync(string searchType, string searchValue, int numberOfResults);
        Task<UserDetailDto> GetByIdAsync(Guid userId);
        Task<Guid> UpdateAsync(Guid userId, UpdateUserDto updateUserDto);
        Task<Guid> UpdateAvatarAsync(Guid userId, UpdateUserPictureDto updateUserPictureDto);
    }
}