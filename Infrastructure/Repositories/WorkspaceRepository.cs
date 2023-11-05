using Microsoft.Extensions.Logging;
using PBL6.Domain.Data.Users;
using PBL6.Domain.Models.Users;
using PBL6.Infrastructure.Data;

namespace PBL6.Infrastructure.Repositories
{
    public class WorkspaceRepository : Repository<Workspace>, IWorkspaceRepository
    {
        public WorkspaceRepository(ApiDbContext context, ILogger logger) : base(context, logger)
        {
        }
    }
}