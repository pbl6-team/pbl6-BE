using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PBL6.Application.Contract.Users.Dtos;
using PBL6.Application.Contract.Workspaces;
using PBL6.Application.Contract.Workspaces.Dtos;

namespace PBL6.API.Controllers.Workspaces
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
        public async Task<IActionResult> Create([FromForm] CreateWorkspaceDto input)
        {
            return Ok(new {Id = await _workspaceService.AddAsync(input)});
        }

        /// <summary>
        /// API Update workspace - cần đăng nhập
        /// </summary>
        /// <param name="workspaceId"></param>
        /// <param name="input"></param>
        /// <returns></returns>
        /// <response code="200">Cập nhật thành công</response>
        /// <response code="400">Có lỗi xảy ra</response>
        [HttpPut("{workspaceId}")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [Authorize]
        public async Task<IActionResult> Update([FromRoute] Guid workspaceId, [FromBody] UpdateWorkspaceDto input)
        {
            return Ok(new {Id = await _workspaceService.UpdateAsync(workspaceId, input)});
        }

        /// <summary>
        /// API Update avatar workspace - cần đăng nhập
        /// </summary>
        /// <param name="workspaceId"></param>
        /// <param name="input"></param>
        /// <returns></returns>
        /// <response code="200">Cập nhật thành công</response>
        /// <response code="400">Có lỗi xảy ra</response>
        [HttpPut("{workspaceId}/avatar")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [Authorize]
        public async Task<IActionResult> UpdateAvatar([FromRoute] Guid workspaceId, [FromForm] UpdateAvatarWorkspaceDto input)
        {
            return Ok(new {Id = await _workspaceService.UpdateAvatarAsync(workspaceId, input)});
        }

        /// <summary>
        /// API delete workspace - cần đăng nhập
        /// </summary>
        /// <param name="workspaceId"></param>
        /// <returns></returns>
        /// <response code="200">Xoá thành công</response>
        /// <response code="400">Có lỗi xảy ra</response>
        [HttpDelete("{workspaceId}")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [Authorize]
        public async Task<IActionResult> Delete([FromRoute] Guid workspaceId)
        {
            return Ok(new {Id = await _workspaceService.DeleteAsync(workspaceId)});
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
        /// <param name="workspaceId"></param>
        /// <returns></returns>
        /// <response code="200">Get thành công</response>
        /// <response code="400">Có lỗi xảy ra</response>
        [HttpGet("{workspaceId}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(WorkspaceDto))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Authorize]
        public async Task<IActionResult> GetById([FromRoute] Guid workspaceId)
        {
            return Ok(await _workspaceService.GetByIdAsync(workspaceId));
        }

        /// <summary>
        /// API Get workspace by name - cần đăng nhập
        /// </summary>
        /// <param name="workspaceName"></param>
        /// <returns></returns>
        /// <response code="200">Get thành công</response>
        /// <response code="400">Có lỗi xảy ra</response>
        [HttpGet("byname/{workspaceName}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<WorkspaceDto>))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Authorize]
        public async Task<IActionResult> GetByName([FromRoute] string workspaceName)
        {
            return Ok(await _workspaceService.GetByNameAsync(workspaceName));
        }

        /// <summary>
        /// API Add member to workspace - cần đăng nhập
        /// </summary>
        /// <param name="workspaceId"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        /// <response code="200">Add thành công</response>
        /// <response code="400">Có lỗi xảy ra</response>
        [HttpPost("{workspaceId}/members/{userId}")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [Authorize]
        public async Task<IActionResult> AddMember([FromRoute] Guid workspaceId, [FromRoute] Guid userId)
        {
            return Ok(new {Id = await _workspaceService.AddMemberToWorkspaceAsync(workspaceId, userId)});
        }

        /// <summary>
        /// API Remove member from workspace - cần đăng nhập
        /// </summary>
        /// <param name="workspaceId"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        /// <response code="200">Remove thành công</response>
        /// <response code="400">Có lỗi xảy ra</response>
        [HttpDelete("{workspaceId}/members/{userId}")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [Authorize]
        public async Task<IActionResult> RemoveMember([FromRoute] Guid workspaceId, [FromRoute] Guid userId)
        {
            return Ok(new {Id = await _workspaceService.RemoveMemberFromWorkspaceAsync(workspaceId, userId)});
        }
    }
}