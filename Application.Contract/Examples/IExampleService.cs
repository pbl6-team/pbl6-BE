using  PBL6.Application.Contract.Examples.Dtos;

namespace PBL6.Application.Contract.Examples
{
    public interface IExampleService
    {
        Task<ExampleDto> GetByIdAsync(Guid id);
        Task<ExampleDto> GetByNameAsync(string name);
        Task<IEnumerable<ExampleDto>> GetAllAsync();
        Task<ExampleDto> UpdateAsync(Guid id, CreateUpdateExampleDto exampleDto);
        Task<Guid> AddAsync(CreateUpdateExampleDto exampleDto);
        Task<ExampleDto> DeleteAsync(Guid id);
    }
}