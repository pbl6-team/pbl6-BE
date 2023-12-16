using PBL6.Domain.Models.Users;

namespace PBL6.Domain.Data.Users
{
public interface INotificationRepository : IRepository<Notification>
    {
        Task<Notification> GetUnsentNotificationById(Guid id);
    }
}