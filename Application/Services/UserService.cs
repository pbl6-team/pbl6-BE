using System.Runtime.CompilerServices;
using Application.Contract.Users.Dtos;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PBL6.Application.Contract.Users;
using PBL6.Application.Contract.Users.Dtos;
using PBL6.Application.Services;

namespace Application.Services;

public class UserService : BaseService, IUserService
{
    private readonly string _className;

    public UserService(IServiceProvider serviceProvider)
        : base(serviceProvider)
    {
        _className = typeof(UserService).Name;
    }


    static string GetActualAsyncMethodName([CallerMemberName] string name = null) => name;

    public async Task<IEnumerable<UserDto2>> GetByWorkspaceIdAsync(Guid workspaceId)
    {
        var method = GetActualAsyncMethodName();
        try
        {
            _logger.LogInformation("[{_className}][{method}] Start", _className, method);
            var users = await _unitOfwork.Users.Queryable()
                                               .Include(x => x.Information)
                                               .Where(x => !x.IsDeleted && _unitOfwork.Workspaces.CheckIsMemberAsync(workspaceId, x.Id).Result)
                                               .ToListAsync();
            _logger.LogInformation("[{_className}][{method}] End", _className, method);
            return _mapper.Map<IEnumerable<UserDto2>>(users);
        }
        catch (Exception e)
        {
            _logger.LogInformation(
                "[{_className}][{method}] Error: {message}",
                _className,
                method,
                e.Message
            );

            throw;
        }
    }
    
    public async Task<IEnumerable<UserDto2>> GetByChannelIdAsync(Guid channelId)
    {
        var method = GetActualAsyncMethodName();
        try
        {
            _logger.LogInformation("[{_className}][{method}] Start", _className, method);
            var users = await _unitOfwork.Users.Queryable()
                                               .Include(x => x.Information)
                                               .Where(x => !x.IsDeleted && _unitOfwork.Channels.CheckIsMemberAsync(channelId, x.Id).Result)
                                               .ToListAsync();
            _logger.LogInformation("[{_className}][{method}] End", _className, method);
            return _mapper.Map<IEnumerable<UserDto2>>(users);
        }
        catch (Exception e)
        {
            _logger.LogInformation(
                "[{_className}][{method}] Error: {message}",
                _className,
                method,
                e.Message
            );

            throw;
        }
    }

    public async Task<Guid> UpdateAsync(Guid userId, UpdateUserDto updateUserDto)
    {
        var method = GetActualAsyncMethodName();
        try
        {
            _logger.LogInformation("[{_className}][{method}] Start", _className, method);
            var user = await _unitOfwork.Users.Queryable()
                                              .Include(x => x.Information)
                                              .FirstOrDefaultAsync(x => !x.IsDeleted && x.Id == userId);
            if (updateUserDto.FirstName == null)
            {
                updateUserDto.FirstName = user.Information.FirstName;
            }
            if (updateUserDto.LastName == null)
            {
                updateUserDto.LastName = user.Information.LastName;
            }
            if (updateUserDto.Phone == null)
            {
                updateUserDto.Phone = user.Information.Phone;
            }
            if (updateUserDto.Gender == null)
            {
                updateUserDto.Gender = user.Information.Gender;
            }

            if (updateUserDto.Email == null)
            {
                updateUserDto.Email = user.Email;
            }

            if (updateUserDto.Email != user.Email)
            {
                var isExist = await _unitOfwork.Users.Queryable()
                                                     .AnyAsync(x => !x.IsDeleted && x.Email == updateUserDto.Email);
                if (isExist)
                {
                    throw new Exception("Email is exist");
                }
            }

            _mapper.Map(updateUserDto, user);
            await _unitOfwork.Users.UpdateAsync(user);
            await _unitOfwork.SaveChangeAsync();

            _logger.LogInformation("[{_className}][{method}] End", _className, method);
            return user.Id;
        }
        catch (Exception e)
        {
            _logger.LogInformation(
                "[{_className}][{method}] Error: {message}",
                _className,
                method,
                e.Message
            );

            throw;
        }
    }

    public async Task<Guid> UpdateAvatarAsync(Guid userId, UpdateUserPictureDto updateUserPictureDto)
    {

        var method = GetActualAsyncMethodName();
        try
        {
            _logger.LogInformation("[{_className}][{method}] Start", _className, method);
            var user = _unitOfwork.Users.Queryable()
                                        .Include(x => x.Information)
                                        .FirstOrDefault(x => !x.IsDeleted && x.Id == userId);
            user.Information.Picture = await _fileService.UploadImageToImgbb(
                updateUserPictureDto.Picture,
                user.Id);

            await _unitOfwork.Users.UpdateAsync(user);
            await _unitOfwork.SaveChangeAsync();
            _logger.LogInformation("[{_className}][{method}] End", _className, method);

            return user.Id;
        }
        catch (Exception e)
        {
            _logger.LogInformation(
                "[{_className}][{method}] Error: {message}",
                _className,
                method,
                e.Message
            );

            throw;
        }

    }
}
