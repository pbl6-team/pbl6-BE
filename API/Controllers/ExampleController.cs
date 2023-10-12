using Microsoft.AspNetCore.Mvc;
using PBL6.Domain.Data;
using PBL6.Domain.Models;

namespace PBL6.API.Controllers
{

    [ApiController]
    [Route("[controller]")]
    public class UserController : BaseApiController
    {
        private readonly IUnitOfwork _unitOfwork;

        public UserController(IUnitOfwork unitOfwork)
        {
            _unitOfwork = unitOfwork;
        }

        [HttpGet("GetAll")]
        public async Task<IActionResult> GetAsync()
        {
            return Ok(await _unitOfwork.Examples.All());
        }

        [HttpGet("GetById")]
        public async Task<IActionResult> GetAsync(Guid id)
        {
            return base.Ok(await _unitOfwork.Examples.GetById((object)id));
        }

        [HttpDelete("Delete")]
        public async Task<IActionResult> DeleteAsync(Guid id)
        {
            var existExample = await _unitOfwork.Examples.GetById(id);
            if (existExample is null) return NotFound();
            await _unitOfwork.Examples.Delete(existExample);
            await _unitOfwork.CompleteAsync();

            return base.Ok();
        }


        [HttpPost("Add")]
        public async Task<IActionResult> AddAsync(Example example)
        {
            await _unitOfwork.Examples.Add(example);
            await _unitOfwork.CompleteAsync();

            return NoContent();
        }

        [HttpPatch("Update")]
        public async Task<IActionResult> UpdateAsync(Example example)
        {
            var existExample = await _unitOfwork.Examples.GetById(example.Id);
            if (existExample is null) return NotFound();

            await _unitOfwork.Examples.Update(example);
            await _unitOfwork.CompleteAsync();

            return NoContent();
        }
    }
}