using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using PBL6.Domain.Data;
using PBL6.Infrastructure.Repositories;

namespace PBL6.Infrastructure.Data
{
    public class UnitOfWork : IUnitOfwork, IDisposable
    {
        private readonly ApiDbContext _apiDbContext;

        public IExampleRepository Examples { get; }

        public UnitOfWork(
            ApiDbContext apiDbContext,
            ILoggerFactory loggerFactory
        )
        {
            _apiDbContext = apiDbContext;
            var logger = loggerFactory.CreateLogger("logs");

            Examples = new ExampleRepository(apiDbContext, logger);
        }


        public async Task SaveChangeAsync()
        {
            await _apiDbContext.SaveChangesAsync();
        }

        public void Dispose()
        {
            _apiDbContext.Dispose();
        }

        public async Task<IDbContextTransaction> BeginTransactionAsync()
        {
            return await _apiDbContext.Database.BeginTransactionAsync();
        }

        public async Task CommitAsync(IDbContextTransaction transaction)
        {
            if (transaction is not null) await transaction.CommitAsync();
        }

        public async Task RollbackAsync(IDbContextTransaction transaction)
        {
            if (transaction is not null) await transaction.RollbackAsync();
        }
    }
}