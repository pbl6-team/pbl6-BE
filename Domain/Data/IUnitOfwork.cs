using Microsoft.EntityFrameworkCore.Storage;
using PBL6.Domain.Data.Users;

namespace PBL6.Domain.Data
{
    public interface IUnitOfwork
    {
        IExampleRepository Examples { get; }
        IUserRepository Users { get; }
        IUserTokenRepository UserTokens { get; }

        Task SaveChangeAsync();
        Task<IDbContextTransaction> BeginTransactionAsync();
        Task CommitAsync(IDbContextTransaction transaction);
        Task RollbackAsync(IDbContextTransaction transaction);
        void Dispose();
    }
}