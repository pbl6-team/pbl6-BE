using System.Runtime.CompilerServices;
using Application.Contract.Dashboard;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PBL6.Application.Hubs;
using PBL6.Application.Services;

namespace Application.Services.Admins;

public class DashboardService : BaseService, IDashboardService
{

    private readonly string _className;

    public DashboardService(IServiceProvider serviceProvider)
        : base(serviceProvider)
    {
        _className = typeof(DashboardService).Name;
    }

    static string GetActualAsyncMethodName([CallerMemberName] string name = null) => name;

    public int TotalOnlineUsers()
    {
        return ChatHub.TotalOnlineUsers();
    }

    public async Task<int> TotalUsersAsync(short status)
    {
        var method = GetActualAsyncMethodName();
        try
        {
            _logger.LogInformation("[{_className}][{method}] Start", _className, method);
            int numberOfUsers;
            if (status == 0)
            {
                numberOfUsers = await _unitOfWork.Users.Queryable()
                                .CountAsync();
            }
            else
            {
                numberOfUsers = await _unitOfWork.Users.Queryable()
                                    .Include(x => x.Information)
                                    .Where(x => x.Information.Status == status)
                                    .CountAsync();
            }
            _logger.LogInformation("[{_className}][{method}] End", _className, method);
            return numberOfUsers;
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

    public async Task<int> TotalWorkspacesAsync(short status)
    {
        var method = GetActualAsyncMethodName();
        try
        {
            _logger.LogInformation("[{_className}][{method}] Start", _className, method);
            int numberOfWorkspaces;
            if (status == 0)
            {
                numberOfWorkspaces = await _unitOfWork.Workspaces.Queryable()
                                .CountAsync();
            }
            else
            {
                numberOfWorkspaces = await _unitOfWork.Workspaces.Queryable()
                                    .Where(x => x.Status == status)
                                    .CountAsync();
            }
            _logger.LogInformation("[{_className}][{method}] End", _className, method);
            return numberOfWorkspaces;
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

    public async Task<IEnumerable<DateTimeOffset>> GetAllUserCreatedDatesAsync()
    {
        var method = GetActualAsyncMethodName();

        _logger.LogInformation("[{_className}][{method}] Start", _className, method);
        var userCreatedDate = await _unitOfWork.Users.Queryable()
                                     .Select(x => x.CreatedAt)
                                     .ToListAsync();
        _logger.LogInformation("[{_className}][{method}] End", _className, method);
        return userCreatedDate;
    }


    public async Task<IEnumerable<DateTimeOffset>> GetAllWorkspaceCreatedDatesAsync()
    {
        var method = GetActualAsyncMethodName();

        _logger.LogInformation("[{_className}][{method}] Start", _className, method);

        var workspaces = await _unitOfWork.Workspaces.Queryable().ToListAsync();
        var createdDates = workspaces.Select(x => x.CreatedAt).ToList();
        _logger.LogInformation("[{_className}][{method}] End", _className, method);
        return createdDates;
    }
}