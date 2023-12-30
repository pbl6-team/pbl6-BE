using System.Runtime.CompilerServices;
using Application.Contract.Admins;
using Application.Contract.Admins.Dtos;
using Hangfire;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Minio.DataModel;
using PBL6.Application.Services;
using PBL6.Common;
using PBL6.Common.Consts;
using PBL6.Common.Enum;
using PBL6.Common.Exceptions;
using PBL6.Common.Functions;
using PBL6.Domain.Models.Admins;

namespace Application.Services.Admins;

public class AdminService : BaseService, IAdminService
{
    private readonly string _className;

    public AdminService(IServiceProvider serviceProvider)
        : base(serviceProvider)
    {
        _className = typeof(AdminService).Name;
    }


    static string GetActualAsyncMethodName([CallerMemberName] string name = null) => name;

    public async Task<Guid> CreateAsync(CreateAdminDto createAdminDto)
    {
        var method = GetActualAsyncMethodName();
        _logger.LogInformation("[{_className}][{method}] Start", _className, method);
        var userId = Guid.Parse(
                    _currentUser.UserId ?? throw new UnauthorizedAccessException()
                );
        var user = await _unitOfWork.Admins.Queryable()
                                       .Include(a => a.Information)
                                       .FirstOrDefaultAsync(a => a.Id == userId);
        if (user.Username != "root")
            throw new UnauthorizedAccessException("Only root can create new admin");

        var userQueryable = _unitOfWork.Admins.Queryable();
        if (userQueryable.Any(x => x.Email == createAdminDto.Email && x.IsActive))
            throw new EmailExistedException(createAdminDto.Email);

        if (userQueryable.Any(x => x.Username == createAdminDto.Username && x.IsActive))
            throw new UsernameExistedException(createAdminDto.Username);

        var passwordSalt = SecurityFunction.GenerateRandomString();
        var password = SecurityFunction.GenerateRandomString(10);
        var hashPassword = SecurityFunction.HashPassword(password, passwordSalt);
        AdminAccount newAccount =
            new()
            {
                Email = createAdminDto.Email,
                Username = createAdminDto.Username,
                Password = hashPassword,
                PasswordSalt = passwordSalt,
                IsActive = true,
                Information = new()
                {
                    FirstName = createAdminDto.FirstName,
                    LastName = createAdminDto.LastName,
                    BirthDate = createAdminDto.BirthDay,
                    Phone = createAdminDto.Phone,
                    Gender = createAdminDto.Gender,
                    Status = (short)ADMIN_STATUS.ACTIVE
                }
            };

        await _unitOfWork.Admins.AddAsync(newAccount);
        await _unitOfWork.SaveChangeAsync();
        _backgroundJobClient.Enqueue(() => _mailService.Send(
                    createAdminDto.Email,
                    MailConst.AdminRandomPassword.Subject,
                    MailConst.AdminRandomPassword.Template,
                    password
                ));
        _logger.LogInformation("[{_className}][{method}] End", _className, method);
        return newAccount.Id;
    }

    public async Task<PagedResult<AdminDto>> GetAllAsync(int pageSize, int pageNumber, short status)
    {
        var method = GetActualAsyncMethodName();

        _logger.LogInformation("[{_className}][{method}] Start", _className, method);
        if (pageNumber < 1)
        {
            throw new BadRequestException("Page number is not valid");
        }
        List<AdminAccount> admins;
        if (status == 0)
        {
            admins = await _unitOfWork.Admins.Queryable()
                                       .Include(a => a.Information)
                                       .ToListAsync();
        }
        else
        {
            admins = await _unitOfWork.Admins.Queryable()
                                       .Include(a => a.Information)
                                       .Where(a => a.Information.Status == status)
                                       .ToListAsync();
        }
        var pagedAdmins = admins.Skip((pageNumber - 1) * pageSize)
                       .Take(pageSize)
                       .ToList();
        _logger.LogInformation("[{_className}][{method}] End", _className, method);

        return new PagedResult<AdminDto>
        {
            PageSize = pageSize,
            CurrentPage = pageNumber,
            TotalPages = (int)Math.Ceiling((double)admins.Count() / pageSize),
            Items = _mapper.Map<IEnumerable<AdminDto>>(pagedAdmins),
        };
    }

    public async Task<AdminDetailDto> GetByIdAsync(Guid adminId)
    {
        var method = GetActualAsyncMethodName();

        _logger.LogInformation("[{_className}][{method}] Start", _className, method);
        var admin = await _unitOfWork.Admins.Queryable()
                                      .Include(a => a.Information)
                                      .FirstOrDefaultAsync(a => a.Id == adminId);
        _logger.LogInformation("[{_className}][{method}] End", _className, method);
        return _mapper.Map<AdminDetailDto>(admin);
    }

    public async Task<PagedResult<AdminDto>> SearchAsync(string searchValue, int pageSize, int pageNumber, short status)
    {
        var method = GetActualAsyncMethodName();

        _logger.LogInformation("[{_className}][{method}] Start", _className, method);
        var admins = new List<AdminAccount>();
        if (pageNumber < 1)
        {
            throw new BadRequestException("Page number is not valid");
        }

        searchValue = searchValue.ToUpper();
        if (status == 0)
        {
            admins = await _unitOfWork.Admins.Queryable()
                                       .Include(a => a.Information)
                                       .Where(a => a.Username.ToUpper().Contains(searchValue)
                                       || a.Username.ToUpper().Contains(searchValue)
                                       || (a.Information.FirstName + " " + a.Information.LastName).ToUpper().Contains(searchValue)
                                       || a.Email.ToUpper().Contains(searchValue)
                                       || (a.Information.Phone != null && a.Information.Phone.Contains(searchValue))
                                       ).ToListAsync();
        }
        else
        {
            admins = await _unitOfWork.Admins.Queryable()
                                       .Include(a => a.Information)
                                       .Where(a => a.Information.Status == status
                                       && (a.Username.ToUpper().Contains(searchValue)
                                       || a.Username.ToUpper().Contains(searchValue)
                                       || (a.Information.FirstName + " " + a.Information.LastName).ToUpper().Contains(searchValue)
                                       || a.Email.ToUpper().Contains(searchValue)
                                       || (a.Information.Phone != null && a.Information.Phone.Contains(searchValue))
                                       )).ToListAsync();
        }
        var pagedAdmins = admins.Skip((pageNumber - 1) * pageSize)
                       .Take(pageSize)
                       .ToList();
        _logger.LogInformation("[{_className}][{method}] End", _className, method);
        return new PagedResult<AdminDto>
        {
            PageSize = pageSize,
            CurrentPage = pageNumber,
            TotalPages = (int)Math.Ceiling((double)admins.Count() / pageSize),
            Items = _mapper.Map<IEnumerable<AdminDto>>(pagedAdmins),
        };
    }

    public async Task<Guid> UpdateAdminStatusAsync(Guid adminId, short status)
    {
        var method = GetActualAsyncMethodName();

        _logger.LogInformation("[{_className}][{method}] Start", _className, method);
        var admin = await _unitOfWork.Admins.Queryable()
                                      .Include(a => a.Information)
                                      .FirstOrDefaultAsync(a => a.Id == adminId);

        if (admin.Username == "root")
            throw new BadRequestException("Cannot change root status");

        var userId = Guid.Parse(
                    _currentUser.UserId ?? throw new UnauthorizedAccessException()
                );
        if (userId == adminId)
            throw new BadRequestException("Cannot change your own status");

        switch (status)
        {
            case (short)ADMIN_STATUS.BLOCKED:
                admin.Information.Status = (short)ADMIN_STATUS.BLOCKED;
                _backgroundJobClient.Enqueue(() => _mailService.Send(
                admin.Email,
                MailConst.AccountBlocked.Subject,
                MailConst.AccountBlocked.Template,
                ""
            ));
                break;
            case (short)ADMIN_STATUS.ACTIVE:
                admin.Information.Status = (short)ADMIN_STATUS.ACTIVE;
                _backgroundJobClient.Enqueue(() => _mailService.Send(
                admin.Email,
                MailConst.AccountReactivated.Subject,
                MailConst.AccountReactivated.Template,
                ""
            ));
                break;
            default:
                throw new BadRequestException("Status is not valid");
        }

        await _unitOfWork.Admins.UpdateAsync(admin);
        await _unitOfWork.SaveChangeAsync();
        _logger.LogInformation("[{_className}][{method}] End", _className, method);
        return admin.Id;
    }

    public async Task<Guid> UpdateAsync(Guid adminId, UpdateAdminDto updateAdminDto)
    {
        var method = GetActualAsyncMethodName();

        _logger.LogInformation("[{_className}][{method}] Start", _className, method);
        var admin = await _unitOfWork.Admins.Queryable()
                                          .Include(x => x.Information)
                                          .FirstOrDefaultAsync(x => x.Id == adminId);
        var userId = Guid.Parse(
                    _currentUser.UserId ?? throw new UnauthorizedAccessException()
                );

        if (admin.Id != userId)
        {
            throw new UnauthorizedAccessException();
        }

        updateAdminDto.FirstName ??= admin.Information.FirstName;
        updateAdminDto.LastName ??= admin.Information.LastName;
        updateAdminDto.Phone ??= admin.Information.Phone;
        updateAdminDto.Gender ??= admin.Information.Gender;
        updateAdminDto.Email ??= admin.Email;

        if (updateAdminDto.Email != admin.Email)
        {
            var isExist = await _unitOfWork.Admins.Queryable()
                                                 .AnyAsync(x => !x.IsDeleted && x.Email == updateAdminDto.Email);
            if (isExist)
            {
                throw new EmailExistedException(updateAdminDto.Email);
            }
        }

        _mapper.Map(updateAdminDto, admin);
        await _unitOfWork.Admins.UpdateAsync(admin);
        await _unitOfWork.SaveChangeAsync();

        _logger.LogInformation("[{_className}][{method}] End", _className, method);
        return admin.Id;
    }
}