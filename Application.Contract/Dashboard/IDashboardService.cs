namespace Application.Contract.Dashboard;

public interface IDashboardService
{
    Task<int> TotalWorkspacesAsync(short status);
    Task<int> TotalUsersAsync(short status);
    Task<int> TotalOnlineUsersAsync();    
}