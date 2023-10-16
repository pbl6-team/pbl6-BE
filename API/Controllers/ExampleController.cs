using Microsoft.AspNetCore.Mvc;
using PBL6.Application.Contract.Examples.Dtos;
using PBL6.Application.Contract.Examples;


namespace PBL6.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ExampleController : BaseApiController
    {
        private readonly IExampleService _exampleService;

        public ExampleController(IExampleService exampleService)
        {
            _exampleService = exampleService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllAsync()
        {
            return Ok(await _exampleService.GetAllAsync());
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetAsync(Guid id)
        {
            var existExample = await _exampleService.GetByIdAsync(id);
            if (existExample is null) return NotFound();

            return Ok(existExample);
        }

        [HttpGet("/byname/{name}")]
        public async Task<IActionResult> GetByNameAsync(string name)
        {
            var existExample = await _exampleService.GetByNameAsync(name);
            if (existExample is null) return NotFound();

            return Ok(existExample);
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteAsync(Guid id)
        {
            var existExample = await _exampleService.DeleteAsync(id);
            if (existExample is null) return NotFound();

            return NoContent();
        }

        [HttpPost]
        public async Task<IActionResult> AddAsync(CreateUpdateExampleDto exampleDto)
        {
            var result = await _exampleService.AddAsync(exampleDto);

            return Created(nameof(GetAsync), new { Id = result });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAsync(Guid id, CreateUpdateExampleDto exampleDto)
        {
            var result = await _exampleService.UpdateAsync(id, exampleDto);
            if (result is null) return NotFound();

            return NoContent();
        }
    }
}