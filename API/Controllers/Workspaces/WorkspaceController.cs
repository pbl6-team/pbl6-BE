using Microsoft.AspNetCore.Mvc;
using PBL6.Application.Filters;
using PBL6.Application.Contract.Workspaces;
using PBL6.Application.Contract.Workspaces.Dtos;
using PBL6.Common.Consts;
using PBL6.API.Filters;
using PBL6.Application.Contract.Users.Dtos;

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
        [AuthorizeFilter]
        public async Task<IActionResult> Create([FromForm] CreateWorkspaceDto input)
        {
            Response.Headers.Add("Access-Control-Allow-Origin", "*");
            return Ok(new { Id = await _workspaceService.AddAsync(input) });
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
        [AuthorizeFilter]
        [WorkspaceFilter]
        public async Task<IActionResult> Update(
            [FromRoute] Guid workspaceId,
            [FromBody] UpdateWorkspaceDto input
        )
        {
            return Ok(new { Id = await _workspaceService.UpdateAsync(workspaceId, input) });
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
        [AuthorizeFilter]
        [WorkspaceFilter]
        public async Task<IActionResult> UpdateAvatar(
            [FromRoute] Guid workspaceId,
            [FromForm] UpdateAvatarWorkspaceDto input
        )
        {
            return Ok(new { Id = await _workspaceService.UpdateAvatarAsync(workspaceId, input) });
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
        [AuthorizeFilter]
        [WorkspaceFilter]
        public async Task<IActionResult> Delete([FromRoute] Guid workspaceId)
        {
            return Ok(new { Id = await _workspaceService.DeleteAsync(workspaceId) });
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
        [AuthorizeFilter]
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
        [AuthorizeFilter]
        [WorkspaceFilter]
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
        [AuthorizeFilter]
        [WorkspaceFilter]
        public async Task<IActionResult> GetByName([FromRoute] string workspaceName)
        {
            return Ok(await _workspaceService.GetByNameAsync(workspaceName));
        }

        /// <summary>
        /// API Add member to workspace - cần đăng nhập
        /// </summary>
        /// <param name="workspaceId"></param>
        /// <param name="userIds"></param>
        /// <returns></returns>
        /// <response code="200">Add thành công</response>
        /// <response code="400">Có lỗi xảy ra</response>
        [HttpPost("{workspaceId}/members")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [AuthorizeFilter]
        [WorkspaceFilter(WorkSpacePolicy.INVITE_MEMBER)]
        public async Task<IActionResult> AddMember(
            [FromRoute] Guid workspaceId,
            [FromBody] List<string> emails
        )
        {
            return Ok(
                new { Id = await _workspaceService.InviteMemberToWorkspaceAsync(workspaceId, emails) }
            );
        }

        /// <summary>
        /// API Remove member from workspace - cần đăng nhập
        /// </summary>
        /// <param name="workspaceId"></param>
        /// <param name="userIds"></param>
        /// <returns></returns>
        /// <response code="200">Remove thành công</response>
        /// <response code="400">Có lỗi xảy ra</response>
        [HttpDelete("{workspaceId}/members")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [AuthorizeFilter]
        [WorkspaceFilter(WorkSpacePolicy.DELETE_MEMBER)]
        public async Task<IActionResult> RemoveMember(
            [FromRoute] Guid workspaceId,
            [FromBody] List<Guid> userIds
        )
        {
            return Ok(
                new
                {
                    Id = await _workspaceService.RemoveMemberFromWorkspaceAsync(workspaceId, userIds)
                }
            );
        }

        /// <summary>
        /// API Get list roles of workspace theo id - cần đăng nhập
        /// </summary>
        /// <returns></returns>
        /// <response code="200">Get thành công</response>
        /// <response code="400">Có lỗi xảy ra</response>
        [HttpGet("{workspaceId}/roles")]
        [ProducesResponseType(
            StatusCodes.Status200OK,
            Type = typeof(IEnumerable<WorkspaceRoleDto>)
        )]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [AuthorizeFilter]
        [WorkspaceFilter]
        public async Task<IActionResult> GetRoles([FromRoute] Guid workspaceId)
        {
            return Ok(await _workspaceService.GetRolesAsync(workspaceId));
        }

        /// <summary>
        /// API Get list members theo role id - cần đăng nhập
        /// </summary>
        /// <returns></returns>
        /// <param name="workspaceId">Id của workspace</param>
        /// <param name="roleId">Id của role</param>
        /// <response code="200">Get thành công</response>
        /// <response code="400">Có lỗi xảy ra</response>
        [HttpGet("{workspaceId}/roles/{roleId}/members")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<UserDetailDto>))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [AuthorizeFilter]
        [WorkspaceFilter]
        public async Task<IActionResult> GetMembersByRoleId([FromRoute] Guid workspaceId, [FromRoute] Guid roleId)
        {
            return Ok(await _workspaceService.GetMembersByRoleIdAsync(workspaceId, roleId));
        }

        /// <summary>
        /// API Get list members chưa có role - cần đăng nhập
        /// </summary>
        /// <returns></returns>
        /// <param name="workspaceId">Id của workspace</param>
        /// <response code="200">Get thành công</response>
        /// <response code="400">Có lỗi xảy ra</response>
        [HttpGet("{workspaceId}/members/withoutrole")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<UserDetailDto>))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [AuthorizeFilter]
        [WorkspaceFilter]
        public async Task<IActionResult> GetMembersWithoutRole([FromRoute] Guid workspaceId)
        {
            return Ok(await _workspaceService.GetMembersWithoutRoleAsync(workspaceId));
        }

        /// <summary>
        /// API Get list permission of workspace by id - cần đăng nhập
        /// </summary>
        /// <returns></returns>
        /// <param name="workspaceId">Id của workspace</param>
        /// <param name="roleId">Id của workspace</param>
        /// <response code="200">Get thành công</response>
        /// <response code="400">Có lỗi xảy ra</response>
        [HttpGet("{workspaceId}/roles/{roleId}/permissions")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<PermissionDto>))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [AuthorizeFilter]
        [WorkspaceFilter]
        public async Task<IActionResult> GetPermissionsByWorkspaceRoleId(
            [FromRoute] Guid workspaceId,
            [FromRoute] Guid roleId
        )
        {
            return Ok(
                await _workspaceService.GetPermissionsByWorkspaceRoleIdAsync(workspaceId, roleId)
            );
        }

        /// <summary>
        /// API Get list active permission - cần đăng nhập
        /// </summary>
        /// <returns></returns>
        /// <response code="200">Get thành công</response>
        /// <response code="400">Có lỗi xảy ra</response>
        [HttpGet("permissions")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<PermissionDto>))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [AuthorizeFilter]
        public async Task<IActionResult> GetPermissions()
        {
            return Ok(await _workspaceService.GetPermissions());
        }

        /// <summary>
        /// API Get role of workspace by id - cần đăng nhập
        /// </summary>
        /// <returns></returns>
        /// <param name="workspaceId">Id của workspace</param>
        /// <param name="roleId">Id của workspace</param>
        /// <response code="200">Get thành công</response>
        /// <response code="400">Có lỗi xảy ra</response>
        [HttpGet("{workspaceId}/roles/{roleId}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(WorkspaceRoleDto))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [AuthorizeFilter]
        [WorkspaceFilter]
        public async Task<IActionResult> GetRole(
            [FromRoute] Guid workspaceId,
            [FromRoute] Guid roleId
        )
        {
            return Ok(await _workspaceService.GetRoleAsync(workspaceId, roleId));
        }

        /// <summary>
        /// API Update role of workspace - cần đăng nhập
        /// </summary>
        /// <param name="workspaceId"></param>
        /// <param name="roleId"></param>
        /// <param name="input"></param>
        /// <returns></returns>
        /// <response code="200">Update thành công</response>
        /// <response code="400">Có lỗi xảy ra</response>
        [HttpPut("{workspaceId}/roles/{roleId}")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [AuthorizeFilter]
        [WorkspaceFilter]
        public async Task<IActionResult> UpdateRole(
            [FromRoute] Guid workspaceId,
            [FromRoute] Guid roleId,
            [FromBody] CreateUpdateWorkspaceRoleDto input
        )
        {
            await _workspaceService.UpdateRoleAsync(workspaceId, roleId, input);
            return NoContent();
        }

        /// <summary>
        /// API add role to workspace - cần đăng nhập
        /// </summary>
        /// <param name="workspaceId"></param>
        /// <param name="input"></param>
        /// <returns></returns>
        /// <response code="200">Add thành công</response>
        /// <response code="400">Có lỗi xảy ra</response>
        [HttpPost("{workspaceId}/roles")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [AuthorizeFilter]
        [WorkspaceFilter]
        public async Task<IActionResult> AddRole(
            [FromRoute] Guid workspaceId,
            [FromBody] CreateUpdateWorkspaceRoleDto input
        )
        {
            return Created(
                "",
                new { Id = await _workspaceService.AddRoleAsync(workspaceId, input) }
            );
        }

        /// <summary>
        /// API delete role of workspace - cần đăng nhập
        /// </summary>
        /// <param name="workspaceId"></param>
        /// <param name="roleId"></param>
        /// <returns></returns>
        /// <response code="200">Delete thành công</response>
        /// <response code="400">Có lỗi xảy ra</response>
        [HttpDelete("{workspaceId}/roles/{roleId}")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [AuthorizeFilter]
        [WorkspaceFilter]
        public async Task<IActionResult> DeleteRole(
            [FromRoute] Guid workspaceId,
            [FromRoute] Guid roleId
        )
        {
            await _workspaceService.DeleteRoleAsync(workspaceId, roleId);
            return NoContent();
        }

        /// <summary>
        /// API set role to user - cần đăng nhập
        /// </summary>
        /// <param name="workspaceId"></param>
        /// <param name="userId"></param>
        /// <param name="role"></param>
        /// <returns></returns>
        /// <response code="200">Delete thành công</response>
        /// <response code="400">Có lỗi xảy ra</response>
        [HttpPut("{workspaceId}/users/{userId}/roles")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [AuthorizeFilter]
        [WorkspaceFilter]
        public async Task<IActionResult> SetRole(
            [FromRoute] Guid workspaceId,
            [FromRoute] Guid userId,
            [FromBody] SetRoleDto role
        )
        {
            await _workspaceService.SetRoleAsync(workspaceId, userId, role.Id);
            return NoContent();
        }

        /// <summary>
        /// API get users with their role by workspaceId - cần đăng nhập
        /// </summary>
        /// <param name="workspaceId"></param>
        /// <returns></returns>
        /// <response code="200">Get thành công</response>
        /// <response code="400">Có lỗi xảy ra</response>
        [HttpGet("{workspaceId}/users")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [AuthorizeFilter]
        [WorkspaceFilter]
        public async Task<IActionResult> GetMembers([FromRoute] Guid workspaceId)
        {
            return Ok(await _workspaceService.GetMembersAsync(workspaceId));
        }

        /// <summary>
        /// Accept invitation - cần đăng nhập
        /// </summary>
        /// <param name="workspaceId"></param>
        /// <returns></returns>
        /// <response code="200">Accept thành công</response>
        /// <response code="400">Có lỗi xảy ra</response>
        [HttpPost("{workspaceId}/accept")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [AuthorizeFilter]
        public async Task<IActionResult> AcceptInvitation([FromRoute] Guid workspaceId)
        {
            await _workspaceService.AcceptInvitationAsync(workspaceId);
            return NoContent();
        }

        /// <summary>
        /// Decline invitation - cần đăng nhập
        /// </summary>
        /// <param name="workspaceId"></param>
        /// <returns></returns>
        /// <response code="200">Decline thành công</response>
        /// <response code="400">Có lỗi xảy ra</response>
        [HttpPost("{workspaceId}/decline")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [AuthorizeFilter]
        public async Task<IActionResult> DeclineInvitation([FromRoute] Guid workspaceId)
        {
            await _workspaceService.DeclineInvitationAsync(workspaceId);
            return NoContent();
        }

        /// <summary>
        /// Leave workspace - cần đăng nhập
        /// </summary>
        /// <param name="workspaceId"></param>
        /// <returns></returns>
        /// <response code="200">Leave thành công</response>
        /// <response code="400">Có lỗi xảy ra</response>
        [HttpPost("{workspaceId}/leave")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [AuthorizeFilter]
        [WorkspaceFilter]
        public async Task<IActionResult> LeaveWorkspace([FromRoute] Guid workspaceId)
        {
            await _workspaceService.LeaveWorkspaceAsync(workspaceId);
            return NoContent();
        }
    }
}