using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using Application.Contract.Users.Dtos;
using Hangfire;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OneSignalApi.Model;
using PBL6.Application.Contract.Users;
using PBL6.Application.Contract.Users.Dtos;
using PBL6.Application.Hubs;
using PBL6.Application.Services;
using PBL6.Common.Consts;
using PBL6.Common.Enum;
using PBL6.Common.Exceptions;

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

    public async Task<IEnumerable<UserDetailDto>> GetByWorkspaceIdAsync(Guid workspaceId)
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
            return _mapper.Map<IEnumerable<UserDetailDto>>(users);
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

    public async Task<IEnumerable<UserDetailDto>> GetByChannelIdAsync(Guid channelId)
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
            return _mapper.Map<IEnumerable<UserDetailDto>>(users);
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
                    throw new BadRequestException("Email is exist");
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
            var file = updateUserPictureDto.Picture;
            var fileName = user.Id + Path.GetExtension(file.FileName);
            var url = await _fileService.UploadFileGetUrlAsync(fileName, file.OpenReadStream(), "image/png");
            user.Information.Picture = url;

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

    public async Task<UserDetailDto> GetByIdAsync(Guid userId)
    {
        var method = GetActualAsyncMethodName();
        try
        {
            _logger.LogInformation("[{_className}][{method}] Start", _className, method);
            var user = await _unitOfWork.Users.Queryable()
                                              .Include(x => x.Information)
                                              .FirstOrDefaultAsync(x => !x.IsDeleted && x.Id == userId);
            _logger.LogInformation("[{_className}][{method}] End", _className, method);
            return _mapper.Map<UserDetailDto>(user);
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

    public async Task<IEnumerable<UserDetailDto>> SearchUserAsync(string searchValue, int numberOfResults)
    {
        var method = GetActualAsyncMethodName();
        try
        {
            _logger.LogInformation("[{_className}][{method}] Start", _className, method);
            var users = await _unitOfWork.Users.Queryable()
                                           .Include(x => x.Information)
                                           .ToListAsync();

            searchValue = searchValue.ToUpper();

            users = users.Where(x => (x.Information.FirstName + " " + x.Information.LastName).ToUpper().Contains(searchValue)
                                    || x.Email.ToUpper().Contains(searchValue)
                                    || (x.Information.Phone != null && x.Information.Phone.ToUpper().Contains(searchValue))).ToList();
            users = users.Take(numberOfResults).ToList();

            _logger.LogInformation("[{_className}][{method}] End", _className, method);
            return _mapper.Map<IEnumerable<UserDetailDto>>(users);
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

    public async Task<PagedResult<AdminUserDto>> GetAllAsync(int pageSize, int pageNumber, short status)
    {
        var method = GetActualAsyncMethodName();
        try
        {
            _logger.LogInformation("[{_className}][{method}] Start", _className, method);
            if (pageNumber < 1)
            {
                throw new BadRequestException("Page number is not valid");
            }

            List<PBL6.Domain.Models.Users.User> users;
            if (status == 0)
            {
                users = await _unitOfWork.Users.Queryable()
                                           .Include(x => x.Information)
                                           .ToListAsync();
            }
            else
            {
                users = await _unitOfWork.Users.Queryable()
                                           .Include(x => x.Information)
                                           .Where(x => x.Information.Status == status)
                                           .ToListAsync();
            }

            var pagedUsers = users.Skip((pageNumber - 1) * pageSize)
                         .Take(pageSize)
                         .ToList();

            _logger.LogInformation("[{_className}][{method}] End", _className, method);
            return new PagedResult<AdminUserDto>
            {
                PageSize = pageSize,
                CurrentPage = pageNumber,
                TotalPages = (int)Math.Ceiling((double)users.Count() / pageSize),
                Items = _mapper.Map<IEnumerable<AdminUserDto>>(pagedUsers)
            };
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

    public async Task<Guid> UpdateUserStatusAsync(Guid userId, short status)
    {
        var method = GetActualAsyncMethodName();
        try
        {
            _logger.LogInformation("[{_className}][{method}] Start", _className, method);

            var user = await _unitOfWork.Users.Queryable()
                                              .Include(x => x.Information)
                                              .FirstOrDefaultAsync(x => !x.IsDeleted && x.Id == userId);
            switch (status)
            {
                case (short)USER.BLOCKED:
                    user.Information.Status = (short)USER.BLOCKED;
                    _backgroundJobClient.Enqueue(() => _mailService.Send(
                    user.Email,
                    MailConst.AccountBlocked.Subject,
                    MailConst.AccountBlocked.Template,
                    ""
                ));
                    break;
                case (short)USER.VERIFIED:
                    user.Information.Status = (short)USER.VERIFIED;
                    _backgroundJobClient.Enqueue(() => _mailService.Send(
                    user.Email,
                    MailConst.AccountReactivated.Subject,
                    MailConst.AccountReactivated.Template,
                    ""
                ));
                    break;
                default:
                    throw new BadRequestException("Status is not valid");
            }
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

    public async Task<AdminUserDto> GetByIdForAdminAsync(Guid userId)
    {
        var method = GetActualAsyncMethodName();
        try
        {
            _logger.LogInformation("[{_className}][{method}] Start", _className, method);
            var user = await _unitOfWork.Users.Queryable()
                                              .Include(x => x.Information)
                                              .FirstOrDefaultAsync(x => !x.IsDeleted && x.Id == userId);
            _logger.LogInformation("[{_className}][{method}] End", _className, method);
            return _mapper.Map<AdminUserDto>(user);
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

    public async Task<PagedResult<AdminUserDto>> SearchUserForAdminAsync(string searchValue, int pageSize, int pageNumber, short status)
    {
        var method = GetActualAsyncMethodName();

        _logger.LogInformation("[{_className}][{method}] Start", _className, method);
        var users = new List<PBL6.Domain.Models.Users.User>();
        if (pageNumber < 1)
        {
            throw new BadRequestException("Page number is not valid");
        }

        searchValue = searchValue.ToUpper();
        if (status == 0)
        {
            users = await _unitOfWork.Users.Queryable().Include(x => x.Information)
                                       .Where(x => x.Username.ToUpper().Contains(searchValue)
                                       || x.Username.ToUpper().Contains(searchValue)
                                       || (x.Information.FirstName + " " + x.Information.LastName).ToUpper().Contains(searchValue)
                                       || x.Email.ToUpper().Contains(searchValue)
                                       || (x.Information.Phone != null && x.Information.Phone.Contains(searchValue))
                                       ).ToListAsync();
        }
        else
        {
            users = await _unitOfWork.Users.Queryable().Include(x => x.Information)
                                       .Where(x => (x.Username.ToUpper().Contains(searchValue)
                                       || x.Username.ToUpper().Contains(searchValue)
                                       || (x.Information.FirstName + " " + x.Information.LastName).ToUpper().Contains(searchValue)
                                       || x.Email.ToUpper().Contains(searchValue)
                                       || (x.Information.Phone != null && x.Information.Phone.Contains(searchValue)))
                                       && x.Information.Status == status
                                       ).ToListAsync();
        }
        var pagedUsers = users.Skip((pageNumber - 1) * pageSize)
                     .Take(pageSize)
                     .ToList();

        _logger.LogInformation("[{_className}][{method}] End", _className, method);
        return new PagedResult<AdminUserDto>
        {
            PageSize = pageSize,
            CurrentPage = pageNumber,
            TotalPages = (int)Math.Ceiling((double)users.Count() / pageSize),
            Items = _mapper.Map<IEnumerable<AdminUserDto>>(pagedUsers)
        };
    }

    public async Task<IEnumerable<UserDetailDto>> SearchUserThatNotInWorkspaceAsync(Guid workspaceId, string searchValue, int numberOfResults)
    {
        var method = GetActualAsyncMethodName();

        _logger.LogInformation("[{_className}][{method}] Start", _className, method);
        var users = await _unitOfWork.Users.Queryable()
                                       .Include(x => x.Information)
                                       .ToListAsync();
        var idOfUsersInWorkspace = await _unitOfWork.WorkspaceMembers.Queryable()
                                       .Where(x => x.WorkspaceId == workspaceId && x.Status != (short)WORKSPACE_MEMBER_STATUS.REMOVED)
                                       .Select(x => x.UserId)
                                       .ToListAsync();

        users = users.Where(x => !idOfUsersInWorkspace.Contains(x.Id)).ToList();

        searchValue = searchValue.ToUpper();

        users = users.Where(x => x.Username.ToUpper().Contains(searchValue)
                                       || (x.Information.FirstName + " " + x.Information.LastName).ToUpper().Contains(searchValue)
                                       || x.Email.ToUpper().Contains(searchValue)
                                       || (x.Information.Phone != null && x.Information.Phone.Contains(searchValue))
                                       ).ToList();
        users = users.Take(numberOfResults).ToList();

        _logger.LogInformation("[{_className}][{method}] End", _className, method);
        return _mapper.Map<IEnumerable<UserDetailDto>>(users);
    }

    public int TotalOnlineUsers()
    {
        return ChatHub.TotalOnlineUsers();
    }
}