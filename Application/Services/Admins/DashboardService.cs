using System.Runtime.CompilerServices;
using Application.Contract.Dashboard;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
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

    public Task<int> TotalOnlineUsersAsync()
    {
        throw new NotImplementedException();
    }

    public async Task<int> TotalUsersAsync(short status)
    {
        var method = GetActualAsyncMethodName();
        try
        {
            _logger.LogInformation("[{_className}][{method}] Start", _className, method);
            
            var numberOfUsers = await _unitOfWork.Users.Queryable()
                                .Include(x => x.Information)
                                .Where(x => x.Information.Status == status)
                                .CountAsync();

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
            
            var numberOfWorkspaces = await _unitOfWork.Workspaces.Queryable()
                                .Where(x => x.Status == status)
                                .CountAsync();

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
}