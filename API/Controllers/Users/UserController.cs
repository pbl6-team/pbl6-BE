using Microsoft.AspNetCore.Mvc;
using PBL6.Application.Contract.Users;
using PBL6.API.Filters;
using PBL6.Application.Contract.Users.Dtos;

namespace API.Controllers.Users;

[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{

    private readonly IUserService _userService;

    public UserController(IUserService userService)
    {
        _userService = userService;
    }


    /// <summary>
    /// API Get users by workspace id - cần đăng nhập
    /// </summary>
    /// <returns></returns>
    /// <response code="200">Get thành công</response>
    /// <response code="400">Có lỗi xảy ra</response>
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<UserDetailDto>))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [AuthorizeFilter]
    [HttpGet("workspace/{workspaceId}")]
    public async Task<ActionResult> GetUsersByWorkspaceIds([FromRoute] Guid workspaceId)
    {
        var users = await _userService.GetByWorkspaceIdAsync(workspaceId);
        return Ok(users);
    }

    /// <summary>
    /// API Get users by channel ids - cần đăng nhập
    /// </summary>
    /// <returns></returns>
    /// <response code="200">Get thành công</response>
    /// <response code="400">Có lỗi xảy ra</response>
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<UserDetailDto>))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [AuthorizeFilter]
    [HttpGet("channel/{channelId}")]
    public async Task<ActionResult> GetUsersByChannelIds([FromRoute] Guid channelId)
    {
        var users = await _userService.GetByChannelIdAsync(channelId);
        return Ok(users);
    }

    /// <summary>
    /// API Update user picture - cần đăng nhập
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="input"></param>
    /// <returns></returns>
    /// <response code="200">Cập nhật thành công</response>
    /// <response code="400">Có lỗi xảy ra</response>
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [AuthorizeFilter]
    [HttpPut("{userId}/picture")]
    public async Task<ActionResult> UpdatePicture([FromRoute] Guid userId, [FromForm] UpdateUserPictureDto input)
    {
        await _userService.UpdateAvatarAsync(userId, input);
        return Ok(new { Id = userId });
    }

    /// <summary>
    /// API Update user - cần đăng nhập
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="input"></param>
    /// <returns></returns>
    /// <response code="200">Cập nhật thành công</response>
    /// <response code="400">Có lỗi xảy ra</response>
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [AuthorizeFilter]
    [HttpPut("{userId}")]
    public async Task<ActionResult> UpdateUser([FromRoute] Guid userId, [FromForm] UpdateUserDto input)
    {
        await _userService.UpdateAsync(userId, input);
        return Ok(new { Id = userId });
    }

    /// <summary>
    /// API Get user by id - cần đăng nhập
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    /// <response code="200">Get thành công</response>
    /// <response code="400">Có lỗi xảy ra</response>
    [HttpGet("{userId}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(UserDetailDto))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [AuthorizeFilter]
    public async Task<IActionResult> GetById([FromRoute] Guid userId)
    {
        return Ok(await _userService.GetByIdAsync(userId));
    }

    /// <summary>
    /// API Search user - cần đăng nhập
    /// </summary>
    /// <param name="searchValue"></param>
    /// <param name="numberOfResults"></param>
    /// <returns></returns>
    /// <response code="200">Get thành công</response>
    /// <response code="400">Có lỗi xảy ra</response>
    [HttpGet("search/{searchValue}/{numberOfResults}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<UserDetailDto>))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [AuthorizeFilter]
    public async Task<IActionResult> SearchUser([FromRoute] string searchValue, [FromRoute] int numberOfResults)
    {
        return Ok(await _userService.SearchUserAsync(searchValue, numberOfResults));
    }

    /// <summary>
    /// Get users not in a workspace - cần đăng nhập
    /// </summary>
    /// <param name="workspaceId"></param>
    /// <param name="searchValue"></param>
    /// <param name="numberOfResults"></param>
    /// <returns></returns>
    /// <response code="200">Get thành công</response>
    /// <response code="400">Có lỗi xảy ra</response>
    [HttpGet("workspace/{workspaceId}/search-user-not-in-workspace/{searchValue}/{numberOfResults}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<UserDetailDto>))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [AuthorizeFilter]
    public async Task<IActionResult> SearchUserThatNotInWorkspace([FromRoute] Guid workspaceId, [FromRoute] string searchValue, [FromRoute] int numberOfResults)
    {
        return Ok(await _userService.SearchUserThatNotInWorkspaceAsync(workspaceId, searchValue, numberOfResults));
    }

    /// <summary>
    /// Get users online - cần đăng nhập
    /// </summary>
    /// <returns></returns>
    /// <response code="200">Get thành công</response>
    /// <response code="400">Có lỗi xảy ra</response>
    /// <response code="401">Chưa đăng nhập</response>
    [HttpGet("total-online-users")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(int))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult TotalOnlineUsers()
    {
        return Ok(_userService.TotalOnlineUsers());
    }
}