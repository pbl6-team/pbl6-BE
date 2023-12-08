using Microsoft.Extensions.Logging;
using PBL6.Domain.Models.Users;
using PBL6.Infrastructure.Data;
using PBL6.Infrastructure.Repositories;

namespace PBL6.Domain.Data.Users
{
    public class PermissionsOfChannelRoleRepository : Repository<PermissionsOfChannelRole>, IPermissionsOfChannelRoleRepository
    {
        public PermissionsOfChannelRoleRepository(ApiDbContext context, ILogger logger) : base(context, logger)
        {
        }
    }
}