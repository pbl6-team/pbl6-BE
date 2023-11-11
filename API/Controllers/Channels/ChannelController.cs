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
        public async Task<IActionResult> Create(CreateChannelDto input)
        {
            try
            {
                return Ok(await _channelService.AddAsync(input));
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
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
        public async Task<IActionResult> Update(Guid channelId, UpdateChannelDto input)
        {
            try
            {
                return Ok(await _channelService.UpdateAsync(channelId, input));
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        /// <summary>
        /// API delete channel - cần đăng nhập
        /// </summary>
        /// <param name="channelId"></param>
        /// <returns></returns>
        /// <response code="200">Xoá thành công</response>
        /// <response code="400">Có lỗi xảy ra</response>
        [HttpDelete]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [Authorize]
        public async Task<IActionResult> Delete(Guid channelId)
        {
            try
            {
                return Ok(await _channelService.DeleteAsync(channelId));
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        /// <summary>
        /// API Get all channels of a workspace - cần đăng nhập
        /// </summary>
        /// <param name="workspaceId"></param>
        /// <returns></returns>
        /// <response code="200">Get thành công</response>
        /// <response code="400">Có lỗi xảy ra</response>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<ChannelDto>))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Authorize]
        public async Task<IActionResult> GetAllChannelsOfAWorkspace(Guid workspaceId)
        {
            try
            {
                return Ok(await _channelService.GetAllChannelsOfAWorkspaceAsync(workspaceId));
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
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
        public async Task<IActionResult> GetById(Guid channelId)
        {
            try
            {
                return Ok(await _channelService.GetByIdAsync(channelId));
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
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
        public async Task<IActionResult> GetByName(string channelName)
        {
            try
            {
                return Ok(await _channelService.GetByNameAsync(channelName));
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        /// <summary>
        /// API Add member to a channel - cần đăng nhập
        /// </summary>
        /// <param name="channelId"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        /// <response code="200">Thêm thành công</response>
        /// <response code="400">Có lỗi xảy ra</response>
        [HttpPost("addmember")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ChannelDto))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Authorize]
        public async Task<IActionResult> AddMemberToChannel(Guid channelId , Guid userId)
        {
            try
            {
                return Ok(await _channelService.AddMemberToChannelAsync(channelId, userId));
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        /// <summary>
        /// API remove member from channel - cần đăng nhập
        /// </summary>
        /// <param name="channelId"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        /// <response code="200">Xoá thành công</response>
        /// <response code="400">Có lỗi xảy ra</response>
        [HttpDelete("removemember")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ChannelDto))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Authorize]
        public async Task<IActionResult> RemoveMemberFromChannel(Guid channelId, Guid userId)
        {
            try
            {
                return Ok(await _channelService.RemoveMemberFromChannelAsync(channelId, userId));
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }
    }
}