namespace PBL6.Domain.Data
{
    public interface IRepository<T>
        where T : class
    {
        IQueryable<T> Queryable(bool IncludeDeleted = false);
        Task<T> FindAsync(object id, bool IncludeDeleted = false);
        Task<T> AddAsync(T entity);
        Task<bool> DeleteAsync(T entity, bool isHardDelete = false);
        Task<bool> UpdateAsync(T entity);
    }
}
