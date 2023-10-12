using PBL6.Domain.Models;

namespace PBL6.Domain.Data
{
    public interface IExampleRepository : IRepository<Example>
    {
        Task<Example> GetExampleByName(string name);
    }
}