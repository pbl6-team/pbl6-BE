using System.Runtime.CompilerServices;
using Application.Contract.Users.Dtos;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PBL6.Application.Contract.Users;
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
            var users = await _unitOfWork.Users.Queryable()
                                   .Include(x => x.Information)
                                   .Where(x => !x.IsDeleted)
                                   .ToListAsync();

            users = users.Where(x => _unitOfWork.Workspaces.CheckIsMemberAsync(workspaceId, x.Id).Result).ToList();

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
            var users = await _unitOfWork.Users.Queryable()
                                           .Include(x => x.Information)
                                           .Where(x => !x.IsDeleted)
                                           .ToListAsync();

            users = users.Where(x => _unitOfWork.Channels.CheckIsMemberAsync(channelId, x.Id).Result).ToList();
            
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
            var user = await _unitOfWork.Users.Queryable()
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
                var isExist = await _unitOfWork.Users.Queryable()
                                                     .AnyAsync(x => !x.IsDeleted && x.Email == updateUserDto.Email);
                if (isExist)
                {
                    throw new Exception("Email is exist");
                }
            }

            _mapper.Map(updateUserDto, user);
            await _unitOfWork.Users.UpdateAsync(user);
            await _unitOfWork.SaveChangeAsync();

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
            var user = _unitOfWork.Users.Queryable()
                                        .Include(x => x.Information)
                                        .FirstOrDefault(x => !x.IsDeleted && x.Id == userId);
            user.Information.Picture = await _fileService.UploadImageToImgbb(
                updateUserPictureDto.Picture,
                user.Id);

            await _unitOfWork.Users.UpdateAsync(user);
            await _unitOfWork.SaveChangeAsync();
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

    public async Task<IEnumerable<UserDto2>> SearchByNameAsync(string fullName)
    {
        var method = GetActualAsyncMethodName();
        try
        {
            _logger.LogInformation("[{_className}][{method}] Start", _className, method);
            var users = await _unitOfWork.Users.Queryable()
                                               .Include(x => x.Information)
                                               .Where(x => !x.IsDeleted && (x.Information.LastName + " " + x.Information.FirstName).ToUpper().Contains(fullName.ToUpper()))
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

    public async Task<IEnumerable<UserDto2>> SearchByEmailAsync(string email)
    {
        var method = GetActualAsyncMethodName();
        try
        {
            _logger.LogInformation("[{_className}][{method}] Start", _className, method);
            var users = await _unitOfWork.Users.Queryable()
                                               .Include(x => x.Information)
                                               .Where(x => !x.IsDeleted && x.Email.ToUpper().Contains(email.ToUpper()))
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

    public async Task<UserDto2> GetByIdAsync(Guid userId)
    {
        var method = GetActualAsyncMethodName();
        try
        {
            _logger.LogInformation("[{_className}][{method}] Start", _className, method);
            var user = await _unitOfWork.Users.Queryable()
                                              .Include(x => x.Information)
                                              .FirstOrDefaultAsync(x => !x.IsDeleted && x.Id == userId);
            _logger.LogInformation("[{_className}][{method}] End", _className, method);
            return _mapper.Map<UserDto2>(user);
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
