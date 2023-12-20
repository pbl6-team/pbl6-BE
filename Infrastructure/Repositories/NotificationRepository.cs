using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PBL6.Common.Enum;
using PBL6.Domain.Data.Users;
using PBL6.Infrastructure.Data;
using PBL6.Infrastructure.Repositories;

namespace PBL6.Domain.Models.Users
{
    public class NotificationRepository : Repository<Notification>, INotificationRepository
    {
        public NotificationRepository(ApiDbContext context, ILogger logger)
            : base(context, logger) { }

        public async Task ReadUserNotification(Guid userId, Guid notificationId)
        {
            var notification = _dbSet
                .Include(x => x.UserNotifications)
                .FirstOrDefault(x => x.Id == notificationId);
            var userNotification = notification.UserNotifications.FirstOrDefault(
                x => x.UserId == userId
            );

            if (notification.Type == ((short)NOTIFICATION_TYPE.NEW_MESSAGE))
            {
                notification.UserNotifications.Remove(userNotification);
            }
            else
            {
                userNotification.Status = (short)NOTIFICATION_STATUS.READ;
            }

            await _apiDbContext.SaveChangesAsync();
        }

        public async Task<Notification> GetUnsentNotificationById(Guid id)
        {
            return await _dbSet
                .Include(x => x.UserNotifications)
                .FirstOrDefaultAsync(
                    x => x.Id == id & x.Status == (short)NOTIFICATION_STATUS.PENDING
                );
        }

        public Task<List<Notification>> GetUserNotifications(Guid userId, int offset, int limit)
        {
            return _dbSet
                .Include(x => x.UserNotifications.Where(y => y.UserId == userId))
                .Where(x => x.UserNotifications.Any(y => y.UserId == userId && !y.IsDeleted))
                .OrderByDescending(x => x.CreatedAt)
                .Skip(offset)
                .Take(limit)
                .ToListAsync();
        }

        public async Task DeleteUserNotification(Guid userId, Guid notificationId)
        {
            var notification = _dbSet
                .Include(x => x.UserNotifications)
                .FirstOrDefault(x => x.Id == notificationId);
            var userNotification = notification.UserNotifications.FirstOrDefault(
                x => x.UserId == userId
            );
            if (notification.Type == ((short)NOTIFICATION_TYPE.NEW_MESSAGE))
            {
                notification.UserNotifications.Remove(userNotification);
            }
            else
            {
                userNotification.IsDeleted = true;
            }

            await _apiDbContext.SaveChangesAsync();
        }

        public Task<int> CountUnreadNotification(Guid userId, short? type = null)
        {
            return _dbSet
                .Include(x => x.UserNotifications)
                .Where(x => type == null || x.Type == type)
                .CountAsync(x => x.UserNotifications.Any(y => y.UserId == userId && y.Status != (short)NOTIFICATION_STATUS.READ && !y.IsDeleted)  && !x.IsDeleted);
        }
    }
}
