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

        public async Task<Notification> GetUnsentNotificationById(Guid id)
        {
            return await _dbSet
                .Include(x => x.UserNotifications)
                .FirstOrDefaultAsync(x => x.Id == id & x.Status == (short)NOTIFICATION_STATUS.PENDING);
        }
    }
}
