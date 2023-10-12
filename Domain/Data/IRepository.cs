namespace PBL6.Domain.Data
{
    public interface IRepository<T>
        where T : class
    {
        Task<IEnumerable<T>> All();
        Task<T> GetById(object id);
        Task<bool> Add(T entity);
        Task<bool> Delete(T entity);
        Task<bool> Update(T entity);
    }
}
