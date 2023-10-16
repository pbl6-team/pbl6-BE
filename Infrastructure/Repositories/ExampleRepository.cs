using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PBL6.Domain.Data;
using PBL6.Domain.Models;
using PBL6.Infrastructure.Data;

namespace PBL6.Infrastructure.Repositories
{
    public class ExampleRepository : Repository<Example>, IExampleRepository
    {
        public ExampleRepository(ApiDbContext context, ILogger logger) : base(context, logger)
        {
        }

        public async Task<Example> GetExampleByName(string name)
        {
            try
            {
                return await _apiDbContext.Examples
                    .FirstOrDefaultAsync(x => x.Name.Contains(name));
            }
            catch (Exception e)
            {
                _logger.LogError("{message}", e.Message);
                throw;
            }
        }
    }
}