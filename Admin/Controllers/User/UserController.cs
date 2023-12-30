using Application.Contract.Users.Dtos;
using Microsoft.AspNetCore.Mvc;
using PBL6.Admin.Filters;
using PBL6.Application.Contract.Users;
using PBL6.Application.Contract.Users.Dtos;

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
    /// Status = 0 - Get all, 1 - Active, 2 - Block
    /// </summary>
    /// <returns></returns>
    /// <response code="200">Get thành công</response>
    /// <response code="400">Có lỗi xảy ra</response>
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PagedResult<AdminUserDto>))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [AdminFilter]
    [HttpGet("page/{pageNumber}/size/{pageSize}/status/{status}")]
    public async Task<ActionResult> GetAllUser([FromRoute] int pageNumber, [FromRoute] int pageSize, [FromRoute] short status)
    {
        var users = await _userService.GetAllAsync(pageSize, pageNumber, status);
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

    /// <summary>
    /// API get user by id - cần đăng nhập
    /// </summary>
    /// <returns></returns>
    /// <response code="200">Get thành công</response>
    /// <response code="400">Có lỗi xảy ra</response>
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(AdminUserDto))]
    [AdminFilter]
    [HttpGet("{userId}")]
    public async Task<ActionResult> GetUserById([FromRoute] Guid userId)
    {
        var user = await _userService.GetByIdForAdminAsync(userId);
        return Ok(user);
    }

    /// <summary>
    /// API Search user - cần đăng nhập
    /// status = 0 - Get all, 1 - Active, 2 - Block
    /// </summary>
    /// <returns></returns>
    /// <response code="200">Get thành công</response>
    /// <response code="400">Có lỗi xảy ra</response>
    [HttpGet("search/{searchValue}/page/{pageNumber}/size/{pageSize}/status/{status}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PagedResult<AdminUserDto>))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [AdminFilter]
    public async Task<IActionResult> SearchUser([FromRoute] string searchValue, [FromRoute] int pageNumber, [FromRoute] int pageSize, [FromRoute] short status)
    {
        return Ok(await _userService.SearchUserForAdminAsync(searchValue, pageSize, pageNumber, status));
    }
}