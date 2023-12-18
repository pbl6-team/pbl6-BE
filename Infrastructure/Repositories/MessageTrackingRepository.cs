using Microsoft.Extensions.Logging;
using PBL6.Domain.Data.Users;
using PBL6.Domain.Models.Users;
using PBL6.Infrastructure.Data;

namespace PBL6.Infrastructure.Repositories
{
    public class MessageTrackingRepository : Repository<MessageTracking>, IMessageTrackingRepository
    {
        public MessageTrackingRepository(ApiDbContext dbContext, ILogger logger)
            : base(dbContext, logger) { }
    }
}
