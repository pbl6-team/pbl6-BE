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


        public async Task CompleteAsync()
        {
            await _apiDbContext.SaveChangesAsync();
        }

        public void Dispose()
        {
            _apiDbContext.Dispose();
        }
    }
}