using Microsoft.AspNetCore.Mvc;
using PBL6.Application.Contract.Workspaces;
using PBL6.Application.Contract.Workspaces.Dtos;
using PBL6.Common.Consts;
using Application.Contract.Users.Dtos;
using Application.Contract.Workspaces.Dtos;
using PBL6.Admin.Filters;

namespace PBL6.API.Controllers.Workspaces
{
    [Produces("application/json")]
    [Route("api/[controller]")]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public class WorkspaceController : Controller
    {
        private readonly IWorkspaceService _workspaceService;

        public WorkspaceController(IWorkspaceService workspaceService) =>
            _workspaceService = workspaceService;

        /// <summary>
        /// API Get all workspaces - cần đăng nhập
        /// Status = 0 - Get all, 1 - Active, 2 - Suspended
        /// </summary>
        /// <returns></returns>
        /// <response code="200">Get thành công</response>
        /// <response code="400">Có lỗi xảy ra</response>
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PagedResult<AdminWorkspaceDto>))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [AdminFilter]
        [HttpGet("page/{pageNumber}/size/{pageSize}/status/{status}")]
        public async Task<ActionResult> GetAllWorkspace([FromRoute] int pageNumber, [FromRoute] int pageSize, [FromRoute] short status)
        {
            var workspaces = await _workspaceService.GetAllForAdminAsync(pageSize, pageNumber, status);
            return Ok(workspaces);
        }

        /// <summary>
        /// API update workspace status - cần đăng nhập
        /// Status: 1 - Active, 2 - Suspended
        /// </summary>
        /// <returns></returns>
        /// <response code="200">Update thành công</response>
        /// <response code="400">Có lỗi xảy ra</response>
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [AdminFilter]
        [HttpPut("{workspaceId}/status/{status}")]
        public async Task<ActionResult> UpdateWorkspaceStatus([FromRoute] Guid workspaceId, [FromRoute] short status)
        {
            await _workspaceService.UpdateWorkspaceStatusAsync(workspaceId, status);
            return Ok();
        }

        /// <summary>
        /// API get workspace by id - cần đăng nhập
        /// </summary>
        /// <returns></returns>
        /// <response code="200">Get thành công</response>
        /// <response code="400">Có lỗi xảy ra</response>
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(WorkspaceDto))]
        [AdminFilter]
        [HttpGet("{workspaceId}")]
        public async Task<ActionResult> GetWorkspaceById([FromRoute] Guid workspaceId)
        {
            var workspace = await _workspaceService.GetByIdForAdminAsync(workspaceId);
            return Ok(workspace);
        }

        /// <summary>
        /// API Search workspace - cần đăng nhập
        /// status = 0 - Get all, 1 - Active, 2 - Suspended
        /// </summary>
        /// <returns></returns>
        /// <response code="200">Get thành công</response>
        /// <response code="400">Có lỗi xảy ra</response>
        [HttpGet("search/{searchValue}/page/{pageNumber}/size/{pageSize}/status/{status}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PagedResult<AdminWorkspaceDto>))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [AdminFilter]
        public async Task<IActionResult> SearchWorkspaces([FromRoute] string searchValue, [FromRoute] int pageNumber, [FromRoute] int pageSize, [FromRoute] short status)
        {
            var workspaces = await _workspaceService.SearchForAdminAsync(searchValue, pageSize, pageNumber, status);
            return Ok(workspaces);
        }
    }
}