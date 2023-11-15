using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PBL6.Application.Contract.Channels;
using PBL6.Application.Contract.Channels.Dtos;
using PBL6.Application.Contract.Users.Dtos;
using PBL6.Application.Contract.Workspaces;
using PBL6.Application.Contract.Workspaces.Dtos;

namespace PBL6.API.Controllers.Channels
{
    [Produces("application/json")]
    [Route("api/[controller]")]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public class ChannelController : Controller
    {
        private readonly IChannelService _channelService;

        public ChannelController(IChannelService channelService) => _channelService = channelService;

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
        [Authorize]
        public async Task<IActionResult> Create([FromBody] CreateChannelDto input)
        {
            return Ok(new {Id = await _channelService.AddAsync(input)});
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
        [Authorize]
        public async Task<IActionResult> Update([FromRoute] Guid channelId, [FromBody] UpdateChannelDto input)
        {
            return Ok(new {Id = await _channelService.UpdateAsync(channelId, input)});
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
        [Authorize]
        public async Task<IActionResult> Delete([FromRoute] Guid channelId)
        {
            return Ok(new {Id = await _channelService.DeleteAsync(channelId)});
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
        [Authorize]
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
        [Authorize]
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
        [Authorize]
        public async Task<IActionResult> GetByName([FromRoute] string channelName)
        {
            return Ok(await _channelService.GetByNameAsync(channelName));
        }

        /// <summary>
        /// API Add member to a channel - cần đăng nhập
        /// </summary>
        /// <param name="channelId"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        /// <response code="200">Thêm thành công</response>
        /// <response code="400">Có lỗi xảy ra</response>
        [HttpPost("{channelId}/members/{userId}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ChannelDto))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Authorize]
        public async Task<IActionResult> AddMemberToChannel([FromRoute] Guid channelId, [FromRoute] Guid userId)
        {
            return Ok(new {Id = await _channelService.AddMemberToChannelAsync(channelId, userId)});
        }

        /// <summary>
        /// API remove member from channel - cần đăng nhập
        /// </summary>
        /// <param name="channelId"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        /// <response code="200">Xoá thành công</response>
        /// <response code="400">Có lỗi xảy ra</response>
        [HttpDelete("{channelId}/members/{userId}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ChannelDto))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Authorize]
        public async Task<IActionResult> RemoveMemberFromChannel([FromRoute] Guid channelId, [FromRoute] Guid userId)
        {
            return Ok(new {Id = await _channelService.RemoveMemberFromChannelAsync(channelId, userId)});
        }
    }
}