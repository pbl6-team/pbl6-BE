using Microsoft.AspNetCore.Mvc;
using PBL6.Application.Filters;
using PBL6.Application.Contract.Channels;
using PBL6.Application.Contract.Channels.Dtos;
using PBL6.Application.Contract.Workspaces.Dtos;
using PBL6.Common.Consts;
using PBL6.API.Filters;
using PBL6.Application.Contract.Users.Dtos;

namespace PBL6.API.Controllers.Channels
{
    [Produces("application/json")]
    [Route("api/[controller]")]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public class ChannelController : Controller
    {
        private readonly IChannelService _channelService;

        public ChannelController(IChannelService channelService) =>
            _channelService = channelService;

        /// <summary>
        /// API để tạo channel - cần đăng nhập
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
        [WorkspaceFilter(WorkSpacePolicy.CREATE_UPDATE_CHANNEL)]
        public async Task<IActionResult> Create([FromBody] CreateChannelDto input)
        {
            return Ok(new { Id = await _channelService.AddAsync(input) });
        }

        /// <summary>
        /// API Update channel - cần đăng nhập
        /// </summary>
        /// <param name="channelId"></param>
        /// <param name="input"></param>
        /// <returns></returns>
        /// <response code="200">Cập nhật thành công</response>
        /// <response code="400">Có lỗi xảy ra</response>
        [HttpPut("{channelId}")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [AuthorizeFilter]
        [WorkspaceFilter(WorkSpacePolicy.CREATE_UPDATE_CHANNEL)]
        public async Task<IActionResult> Update(
            [FromRoute] Guid channelId,
            [FromBody] UpdateChannelDto input
        )
        {
            return Ok(new { Id = await _channelService.UpdateAsync(channelId, input) });
        }

        /// <summary>
        /// API delete channel - cần đăng nhập
        /// </summary>
        /// <param name="channelId"></param>
        /// <returns></returns>
        /// <response code="200">Xoá thành công</response>
        /// <response code="400">Có lỗi xảy ra</response>
        [HttpDelete("{channelId}")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [AuthorizeFilter]
        [WorkspaceFilter(WorkSpacePolicy.DELETE_CHANNEL)]
        public async Task<IActionResult> Delete([FromRoute] Guid channelId)
        {
            return Ok(new { Id = await _channelService.DeleteAsync(channelId) });
        }

        /// <summary>
        /// API Get all channels of a workspace - cần đăng nhập
        /// </summary>
        /// <param name="workspaceId"></param>
        /// <returns></returns>
        /// <response code="200">Get thành công</response>
        /// <response code="400">Có lỗi xảy ra</response>
        [HttpGet("workspace/{workspaceId}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<ChannelDto>))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [AuthorizeFilter]
        public async Task<IActionResult> GetAllChannelsOfAWorkspace([FromRoute] Guid workspaceId)
        {
            return Ok(await _channelService.GetAllChannelsOfAWorkspaceAsync(workspaceId));
        }

        /// <summary>
        /// API Get channel by id - cần đăng nhập
        /// </summary>
        /// <param name="channelId"></param>
        /// <returns></returns>
        /// <response code="200">Get thành công</response>
        /// <response code="400">Có lỗi xảy ra</response>
        [HttpGet("{channelId}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ChannelDto))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [AuthorizeFilter]
        public async Task<IActionResult> GetById([FromRoute] Guid channelId)
        {
            return Ok(await _channelService.GetByIdAsync(channelId));
        }

        /// <summary>
        /// API Get channel by name - cần đăng nhập
        /// </summary>
        /// <param name="channelName"></param>
        /// <returns></returns>
        /// <response code="200">Get thành công</response>
        /// <response code="400">Có lỗi xảy ra</response>
        [HttpGet("byname/{channelName}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<ChannelDto>))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [AuthorizeFilter]
        public async Task<IActionResult> GetByName([FromRoute] string channelName)
        {
            return Ok(await _channelService.GetByNameAsync(channelName));
        }

        /// <summary>
        /// API Add member to a channel - cần đăng nhập
        /// </summary>
        /// <param name="channelId"></param>
        /// <param name="userIds"></param>
        /// <returns></returns>
        /// <response code="200">Thêm thành công</response>
        /// <response code="400">Có lỗi xảy ra</response>
        [HttpPost("{channelId}/members")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ChannelDto))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [AuthorizeFilter]
        [ChannelFilter(ChannelPolicy.INVITE_MEMBER)]
        public async Task<IActionResult> AddMemberToChannel(
            [FromRoute] Guid channelId,
            [FromBody] List<Guid> userIds
        )
        {
            return Ok(
                new { Id = await _channelService.AddMemberToChannelAsync(channelId, userIds) }
            );
        }

        /// <summary>
        /// API remove member from channel - cần đăng nhập
        /// </summary>
        /// <param name="channelId"></param>
        /// <param name="userIds"></param>
        /// <returns></returns>
        /// <response code="200">Xoá thành công</response>
        /// <response code="400">Có lỗi xảy ra</response>
        [HttpDelete("{channelId}/members")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ChannelDto))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [AuthorizeFilter]
        [ChannelFilter(ChannelPolicy.DELETE_MEMBER)]
        public async Task<IActionResult> RemoveMemberFromChannel(
            [FromRoute] Guid channelId,
            [FromBody] List<Guid> userIds
        )
        {
            return Ok(
                new { Id = await _channelService.RemoveMemberFromChannelAsync(channelId, userIds) }
            );
        }

        /// <summary>
        /// API Get list roles of channel theo id - cần đăng nhập
        /// </summary>
        /// <returns></returns>
        /// <param name="channelId">Id của channel</param>
        /// <response code="200">Get thành công</response>
        /// <response code="400">Có lỗi xảy ra</response>
        [HttpGet("{channelId}/roles")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<ChannelRoleDto>))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [AuthorizeFilter]
        public async Task<IActionResult> GetRoles([FromRoute] Guid channelId)
        {
            return Ok(await _channelService.GetRolesAsync(channelId));
        }

        /// <summary>
        /// API Get list members theo role id - cần đăng nhập
        /// </summary>
        /// <returns></returns>
        /// <param name="channelId">Id của channel</param>
        /// <param name="roleId">Id của role</param>
        /// <response code="200">Get thành công</response>
        /// <response code="400">Có lỗi xảy ra</response>
        [HttpGet("{channelId}/roles/{roleId}/members")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<UserDetailDto>))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [AuthorizeFilter]
        public async Task<IActionResult> GetMembersByRoleId([FromRoute] Guid channelId, [FromRoute] Guid roleId)
        {
            return Ok(await _channelService.GetMembersByRoleIdAsync(channelId, roleId));
        }

        /// <summary>
        /// API Get list members chưa có role - cần đăng nhập
        /// </summary>
        /// <returns></returns>
        /// <param name="channelId">Id của channel</param>
        /// <response code="200">Get thành công</response>
        /// <response code="400">Có lỗi xảy ra</response>
        [HttpGet("{channelId}/members/withoutrole")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<UserDetailDto>))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [AuthorizeFilter]
        public async Task<IActionResult> GetMembersWithoutRole([FromRoute] Guid channelId)
        {
            return Ok(await _channelService.GetMembersWithoutRoleAsync(channelId));
        }

        /// <summary>
        /// API Get list permission of channel by id - cần đăng nhập
        /// </summary>
        /// <returns></returns>
        /// <param name="channelId">Id của channel</param>
        /// <param name="roleId">Id của channel</param>
        /// <response code="200">Get thành công</response>
        /// <response code="400">Có lỗi xảy ra</response>
        [HttpGet("{channelId}/roles/{roleId}/permissions")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<PermissionDto>))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [AuthorizeFilter]
        public async Task<IActionResult> GetPermissions(
            [FromRoute] Guid channelId,
            [FromRoute] Guid roleId
        )
        {
            return Ok(await _channelService.GetPermissionsAsync(channelId, roleId));
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
            return Ok(await _channelService.GetPermissions());
        }

        /// <summary>
        /// API Get role infor - cần đăng nhập
        /// </summary>
        /// <param name="channelId"></param>
        /// <param name="roleId"></param>
        /// <returns></returns>
        /// <response code="200">Get thành công</response>
        /// <response code="400">Có lỗi xảy ra</response>
        [HttpGet("{channelId}/roles/{roleId}")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ChannelRoleDto))]
        [AuthorizeFilter]
        public async Task<IActionResult> GetRole(
            [FromRoute] Guid channelId,
            [FromRoute] Guid roleId
        )
        {
            return Ok(await _channelService.GetRoleAsync(channelId, roleId));
        }

        /// <summary>
        /// API Update role of channel - cần đăng nhập
        /// </summary>
        /// <param name="channelId"></param>
        /// <param name="roleId"></param>
        /// <param name="input"></param>
        /// <returns></returns>
        /// <response code="200">Update thành công</response>
        /// <response code="400">Có lỗi xảy ra</response>
        [HttpPut("{channelId}/roles/{roleId}")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [AuthorizeFilter]
        public async Task<IActionResult> UpdateRole(
            [FromRoute] Guid channelId,
            [FromRoute] Guid roleId,
            [FromBody] CreateUpdateChannelRoleDto input
        )
        {
            await _channelService.UpdateRoleAsync(channelId, roleId, input);
            return NoContent();
        }

        /// <summary>
        /// API add role to channel - cần đăng nhập
        /// </summary>
        /// <param name="channelId"></param>
        /// <param name="input"></param>
        /// <returns></returns>
        /// <response code="200">Add thành công</response>
        /// <response code="400">Có lỗi xảy ra</response>
        [HttpPost("{channelId}/roles")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [AuthorizeFilter]
        public async Task<IActionResult> AddRole(
            [FromRoute] Guid channelId,
            [FromBody] CreateUpdateChannelRoleDto input
        )
        {
            return Ok(new { Id = await _channelService.AddRoleAsync(channelId, input) });
        }

        /// <summary>
        /// API delete role of channel - cần đăng nhập
        /// </summary>
        /// <param name="channelId"></param>
        /// <param name="roleId"></param>
        /// <returns></returns>
        /// <response code="200">Delete thành công</response>
        /// <response code="400">Có lỗi xảy ra</response>
        [HttpDelete("{channelId}/roles/{roleId}")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [AuthorizeFilter]
        public async Task<IActionResult> DeleteRole(
            [FromRoute] Guid channelId,
            [FromRoute] Guid roleId
        )
        {
            await _channelService.DeleteRoleAsync(channelId, roleId);
            return NoContent();
        }

        /// <summary>
        /// API set role to user - cần đăng nhập
        /// </summary>
        /// <param name="channelId"></param>
        /// <param name="userId"></param>
        /// <param name="role"></param>
        /// <returns></returns>
        /// <response code="200">Delete thành công</response>
        /// <response code="400">Có lỗi xảy ra</response>
        [HttpPut("{channelId}/users/{userId}/roles")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [AuthorizeFilter]
        public async Task<IActionResult> SetRoleToUser(
            [FromRoute] Guid channelId,
            [FromRoute] Guid userId,
            [FromBody] SetRoleDto role
        )
        {
            await _channelService.SetRoleToUserAsync(channelId, userId, role.Id);
            return NoContent();
        }

        /// <summary>
        /// API get user that is not in channel - cần đăng nhập
        /// </summary>
        /// <param name="workspaceId"></param>
        /// <param name="channelId"></param>
        /// <returns></returns>
        /// <response code="200">Get thành công</response>
        /// <response code="400">Có lỗi xảy ra</response>
        [HttpGet("workspace/{workspaceId}/channel/{channelId}/user-not-in-channel")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [AuthorizeFilter]
        public async Task<IActionResult> GetMembersThatNotInTheChannel(
            [FromRoute] Guid workspaceId,
            [FromRoute] Guid channelId
        )
        {
            return Ok(await _channelService.GetMembersThatNotInTheChannel(workspaceId, channelId));
        }

        /// <summary>
        /// API get users with their role by channelId - cần đăng nhập
        /// </summary>
        /// <param name="channelId"></param>
        /// <returns></returns>
        /// <response code="200">Get thành công</response>
        /// <response code="400">Có lỗi xảy ra</response>
        [HttpGet("{channelId}/users")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [AuthorizeFilter]
        public async Task<IActionResult> GetMembers([FromRoute] Guid channelId)
        {
            return Ok(await _channelService.GetMembersAsync(channelId));
        }

        /// <summary>
        /// Accept invitation - cần đăng nhập
        /// </summary>
        /// <param name="channelId"></param>
        /// <returns></returns>
        /// <response code="200">Accept thành công</response>
        /// <response code="400">Có lỗi xảy ra</response>
        [HttpPost("{channelId}/accept-invitation")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [AuthorizeFilter]
        public async Task<IActionResult> AcceptInvitation([FromRoute] Guid channelId)
        {
            await _channelService.AcceptInvitationAsync(channelId);
            return NoContent();
        }

        /// <summary>
        /// Decline invitation - cần đăng nhập
        /// </summary>
        /// <param name="channelId"></param>
        /// <returns></returns>
        /// <response code="200">Decline thành công</response>
        /// <response code="400">Có lỗi xảy ra</response>
        [HttpPost("{channelId}/decline-invitation")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [AuthorizeFilter]
        public async Task<IActionResult> DeclineInvitation([FromRoute] Guid channelId)
        {
            await _channelService.DeclineInvitationAsync(channelId);
            return NoContent();
        }
    }
}