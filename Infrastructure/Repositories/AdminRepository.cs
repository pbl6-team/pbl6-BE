using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PBL6.Common;
using PBL6.Common.Functions;
using PBL6.Domain.Data.Admins;
using PBL6.Domain.Models.Admins;
using PBL6.Infrastructure.Data;

namespace PBL6.Infrastructure.Repositories
{
    public class AdminRepository : Repository<AdminAccount>, IAdminRepository
    {
        public AdminRepository(ApiDbContext context, ILogger logger)
            : base(context, logger) { }

        public async Task<AdminAccount> CheckAccountValidAsync(string userName, string password)
        {
            var existAccount =
                await _dbSet.FirstOrDefaultAsync(x => x.Username == userName)
                ?? throw new InvalidUsernamePasswordException();
            var passwordSalt = existAccount.PasswordSalt;
            var passwordHashed = SecurityFunction.HashPassword(password, passwordSalt);

            if (passwordHashed != existAccount.Password)
            {
                throw new InvalidUsernamePasswordException();
            }

            return existAccount;
        }
    }
}
