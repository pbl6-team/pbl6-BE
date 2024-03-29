using PBL6.Domain.Models.Users;

namespace PBL6.Domain.Data.Users
{
    public interface INotificationRepository : IRepository<Notification>
    {
        Task<Notification> GetUnsentNotificationById(Guid id);
        Task<List<Notification>> GetUserNotifications(Guid userId, int offset, int limit, short? type = null);
        Task ReadUserNotification(Guid userId, Guid notificationId);
        Task DeleteUserNotification(Guid userId, Guid notificationId);
        Task<int> CountUnreadNotification(Guid userId, short? type = null);
    }
}
