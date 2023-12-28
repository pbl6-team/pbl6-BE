using Application.Contract.Users.Dtos;
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
        Task<PagedResult<AdminUserDto>> GetAllAsync(int pageSize, int pageNumber);
        Task<Guid> UpdateUserStatusAsync(Guid userId, short status);
        Task<AdminUserDto> GetByIdForAdminAsync(Guid userId);
        Task<IEnumerable<AdminUserDto>> SearchUserForAdminAsync(short searchType, string searchValue, int numberOfResults);

    }
}