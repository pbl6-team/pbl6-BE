using Application.Contract.Users.Dtos;
using Microsoft.AspNetCore.Mvc;
using PBL6.Admin.Filters;
using PBL6.Application.Contract.Users;

namespace Admin.Controllers.User;

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
    /// API Get all users - cần đăng nhập
    /// </summary>
    /// <returns></returns>
    /// <response code="200">Get thành công</response>
    /// <response code="400">Có lỗi xảy ra</response>
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<AdminUserDto>))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [AdminFilter]
    [HttpGet]
    public async Task<ActionResult> GetAllUser()
    {
        var users = await _userService.GetAllAsync();
        return Ok(users);
    }

    /// <summary>
    /// API update user status - cần đăng nhập
    /// Status: 1 - Unblock, 2 - Block
    /// </summary>
    /// <returns></returns>
    /// <response code="200">Update thành công</response>
    /// <response code="400">Có lỗi xảy ra</response>
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [AdminFilter]
    [HttpPut("{userId}/status/{status}")]
    public async Task<ActionResult> UpdateUserStatus([FromRoute] Guid userId, [FromRoute] short status)
    {
        await _userService.UpdateUserStatusAsync(userId, status);
        return Ok();
    }
}