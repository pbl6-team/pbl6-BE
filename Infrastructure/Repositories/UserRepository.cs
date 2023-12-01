using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PBL6.Domain.Data.Users;
using PBL6.Domain.Models.Users;
using PBL6.Infrastructure.Data;

namespace PBL6.Infrastructure.Repositories
{
    public class UserRepository : Repository<User>, IUserRepository
    {
        public UserRepository(ApiDbContext context, ILogger logger) : base(context, logger)
        {
        }

        public Task<bool> CheckIsUserAsync(Guid userId)
        {
            return _apiDbContext.Users.AnyAsync(x => x.Id == userId && !x.IsDeleted);
        }
    }
}