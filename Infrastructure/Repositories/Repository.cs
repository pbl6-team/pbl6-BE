using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PBL6.Domain.Data;
using PBL6.Infrastructure.Data;

namespace PBL6.Infrastructure.Repositories
{
    public class Repository<T> : IRepository<T> where T : class
    {
        protected ApiDbContext _apiDbContext;
        internal DbSet<T> _dbSet;
        internal readonly ILogger _logger;

        public Repository(
            ApiDbContext context,
            ILogger logger
        )
        {
            _apiDbContext = context;
            _logger = logger;
            _dbSet = context.Set<T>();
        }

        public virtual async Task<bool> Add(T entity)
        {
            await _dbSet.AddAsync(entity);
            return true;
        }

        public virtual async Task<IEnumerable<T>> All()
        {
            return await _dbSet.AsNoTracking().ToListAsync();
        }

        public virtual async Task<bool> Delete(T entity)
        {
            _dbSet.Remove(entity);
            await Task.CompletedTask;
            return true;
        }

        public virtual async Task<T> GetById(object id)
        {
            return await _dbSet.FindAsync(id);
        }

        public virtual async Task<bool> Update(T entity)
        {
            _dbSet.Update(entity);
            await Task.CompletedTask;
            return true;
        }
    }
}