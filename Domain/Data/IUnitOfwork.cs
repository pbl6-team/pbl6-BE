using Microsoft.EntityFrameworkCore.Storage;

namespace PBL6.Domain.Data
{
    public interface IUnitOfwork
    {
        IExampleRepository Examples { get; }
        Task SaveChangeAsync();
        Task<IDbContextTransaction> BeginTransactionAsync();
        Task CommitAsync(IDbContextTransaction transaction);
        Task RollbackAsync(IDbContextTransaction transaction);
        void Dispose();
    }
}