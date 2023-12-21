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
        /// </summary>
        /// <returns></returns>
        /// <response code="200">Get thành công</response>
        /// <response code="400">Có lỗi xảy ra</response>
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<AdminWorkspaceDto>))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [AdminFilter]
        [HttpGet]
        public async Task<ActionResult> GetAllWorkspace()
        {
            var workspaces = await _workspaceService.GetAllForAdminAsync();
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
    }
}