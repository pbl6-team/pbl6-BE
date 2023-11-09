using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PBL6.Application.Contract.Users.Dtos;
using PBL6.Application.Contract.Workspaces;
using PBL6.Application.Contract.Workspaces.Dtos;

namespace PBL6.API.Controllers.Auth
{
    [Produces("application/json")]
    [Route("api/[controller]")]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public class WorkspaceController : Controller
    {
        private readonly IWorkspaceService _workspaceService;

        public WorkspaceController(IWorkspaceService workspaceService) => _workspaceService = workspaceService;

        /// <summary>
        /// API để tạo workspace - cần đăng nhập
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        /// <response code="200">Tạo thành công</response>
        /// <response code="400">Có lỗi xảy ra</response>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesDefaultResponseType]
        [Authorize]
        public async Task<IActionResult> Create(CreateWorkspaceDto input)
        {
            return Ok(await _workspaceService.AddAsync(input));
        }
        
        /// <summary>
        /// API Update workspace - cần đăng nhập
        /// </summary>
        /// <param name="id"></param>
        /// <param name="input"></param>
        /// <returns></returns>
        /// <response code="204">Cập nhật thành công</response>
        /// <response code="400">Có lỗi xảy ra</response>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [Authorize]
        public async Task<IActionResult> Update(Guid id, UpdateWorkspaceDto input)
        {
            var result = await _workspaceService.UpdateAsync(id, input);
            if (result is null) return NotFound();

            return NoContent();
        }

        /// <summary>
        /// API Update avatar workspace - cần đăng nhập
        /// </summary>
        /// <param name="id"></param>
        /// <param name="input"></param>
        /// <returns></returns>
        /// <response code="204">Cập nhật thành công</response>
        /// <response code="400">Có lỗi xảy ra</response>
        [HttpPut("avatar")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [Authorize]
        public async Task<IActionResult> UpdateAvatar(Guid id, UpdateAvatarWorkspaceDto input)
        {
            var result = await _workspaceService.UpdateAvatarAsync(id, input);
            if (result is null) return NotFound();

            return NoContent();
        }

        /// <summary>
        /// API delete workspace - cần đăng nhập
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        /// <response code="204">Xoá thành công</response>
        /// <response code="400">Có lỗi xảy ra</response>
        [HttpDelete]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [Authorize]
        public async Task<IActionResult> Delete(Guid id)
        {
            var result = await _workspaceService.DeleteAsync(id);
            if (result is null) return NotFound();

            return NoContent();
        }

        /// <summary>
        /// API Get all workspaces - cần đăng nhập
        /// </summary>
        /// <returns></returns>
        /// <response code="200">Get thành công</response>
        /// <response code="400">Có lỗi xảy ra</response>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<WorkspaceDto>))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Authorize]
        public async Task<IActionResult> GetAll()
        {
            return Ok(await _workspaceService.GetAllAsync());
        }

        /// <summary>
        /// API Get workspace by id - cần đăng nhập
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        /// <response code="200">Get thành công</response>
        /// <response code="400">Có lỗi xảy ra</response>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(WorkspaceDto))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Authorize]
        public async Task<IActionResult> GetById(Guid id)
        {
            return Ok(await _workspaceService.GetByIdAsync(id));
        }

        /// <summary>
        /// API Get workspace by name - cần đăng nhập
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        /// <response code="200">Get thành công</response>
        /// <response code="400">Có lỗi xảy ra</response>
        [HttpGet("byname/{input}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<WorkspaceDto>))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Authorize]
        public async Task<IActionResult> GetByName(string input)
        {
            return Ok(await _workspaceService.GetByNameAsync(input));
        }
    }
}