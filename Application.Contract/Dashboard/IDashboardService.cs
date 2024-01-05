namespace Application.Contract.Dashboard;

public interface IDashboardService
{
    Task<int> TotalWorkspacesAsync(short status);
    Task<int> TotalUsersAsync(short status);
    int TotalOnlineUsers();
    Task<IEnumerable<DateTimeOffset>> GetAllUserCreatedDatesAsync();
    Task<IEnumerable<DateTimeOffset>> GetAllWorkspaceCreatedDatesAsync();
}