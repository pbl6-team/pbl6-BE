using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PBL6.Domain.Data;
using PBL6.Domain.Models.Common;
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

        public virtual async Task<T> AddAsync(T entity)
        {
            return (await _dbSet.AddAsync(entity)).Entity;
        }

        public virtual IQueryable<T> Queryable(bool IncludeDeleted = false)
        {
            if (IncludeDeleted || !typeof(T).IsSubclassOf(typeof(FullAuditedEntity)))
            {
                return _dbSet.AsQueryable<T>();
            }
            else
            {
                return _dbSet.Where(e => !(e as FullAuditedEntity).IsDeleted).AsQueryable();
            }
        }


        public virtual async Task<bool> DeleteAsync(T entity, bool isHardDelete = false)
        {
            if (entity is FullAuditedEntity root && !isHardDelete)
            {
                root.IsDeleted = true;
                _dbSet.Update(entity);
            }
            else
            {
                _dbSet.Remove(entity);
            }
            await Task.CompletedTask;

            return true;
        }

        public virtual async Task<T> FindAsync(object id, bool IncludeDeleted = false)
        {
            if (IncludeDeleted)
            {
                return await _dbSet.FindAsync(id);
            }
            else
            {
                if (typeof(T).IsSubclassOf(typeof(FullAuditedEntity)))
                {
                    // If T is a subclass of AuditEntity, check IsDeleted
                    return _dbSet.Where(e => !(e as FullAuditedEntity).IsDeleted && (e as FullAuditedEntity).Id.Equals(id)).FirstOrDefault();
                }
                else
                {
                    // If T is not a subclass of AuditEntity, just check by Id
                    return _dbSet.Find(id);
                }
            }
        }
        public virtual async Task<bool> UpdateAsync(T entity)
        {
            _dbSet.Update(entity);
            await Task.CompletedTask;

            return true;
        }
    }
}