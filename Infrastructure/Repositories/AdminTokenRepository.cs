using Microsoft.Extensions.Logging;
using PBL6.Domain.Models.Admins;
using PBL6.Infrastructure.Data;
using PBL6.Infrastructure.Repositories;

namespace PBL6.Domain.Data.Admins
{
    public class AdminTokenRepository : Repository<AdminToken>, IAdminTokenRepository
    {
        public AdminTokenRepository(ApiDbContext context, ILogger logger) : base(context, logger)
        {
        }
    }
}