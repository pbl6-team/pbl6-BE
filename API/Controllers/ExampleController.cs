using Microsoft.AspNetCore.Mvc;
using PBL6.Application.Contract.Examples.Dtos;
using PBL6.Application.Contract.Examples;
using System.ComponentModel.DataAnnotations;
using PBL6.Common.Functions;

namespace PBL6.API.Controllers
{
    [ApiController]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [Route("[controller]")]
    public class ExampleController : BaseApiController
    {
        private readonly IExampleService _exampleService;

        public ExampleController(IExampleService exampleService)
        {
            _exampleService = exampleService;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<ExampleDto>))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetAllAsync()
        {
            return Ok(await _exampleService.GetAllAsync());
        }

        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ExampleDto))]
        public async Task<IActionResult> GetAsync(Guid id)
        {
            var existExample = await _exampleService.GetByIdAsync(id);
            if (existExample is null) return NotFound();

            return Ok(existExample);
        }

        [HttpGet("/byname/{name}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ExampleDto))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesDefaultResponseType]
        public async Task<IActionResult> GetByNameAsync(string name)
        {
            var existExample = await _exampleService.GetByNameAsync(name);
            if (existExample is null) return NotFound();

            return Ok(existExample);
        }

        [HttpDelete]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> DeleteAsync(Guid id)
        {
            var existExample = await _exampleService.DeleteAsync(id);
            if (existExample is null) return NotFound();

            return NoContent();
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesDefaultResponseType]
        public async Task<IActionResult> AddAsync(CreateUpdateExampleDto exampleDto)
        {

            var result = await _exampleService.AddAsync(exampleDto);

            return Created(nameof(GetAsync), new { Id = result });

        }

        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> UpdateAsync(Guid id, CreateUpdateExampleDto exampleDto)
        {
            var result = await _exampleService.UpdateAsync(id, exampleDto);
            if (result is null) return NotFound();

            return NoContent();
        }
    }
}