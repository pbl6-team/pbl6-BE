using Application.Contract.Dashboard;
using Application.Contract.Users.Dtos;
using Microsoft.AspNetCore.Mvc;
using PBL6.Admin.Filters;

namespace Admin.Controllers.User;

[ApiController]
[Route("api/[controller]")]
public class DashboardController : ControllerBase
{
    private readonly IDashboardService _dashboardService;

    public DashboardController(IDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    /// <summary>
    /// API Get total number of users - cần đăng nhập
    /// status = 0 - Get all, 1 - Active, 2 - Block
    /// </summary>
    /// <returns></returns>
    /// <response code="200">Get thành công</response>
    /// <response code="400">Có lỗi xảy ra</response>
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(int))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [AdminFilter]
    [HttpGet("total-users/{status}")]
    public async Task<ActionResult> TotalUsers([FromRoute] short status)
    {
        var totalUsers = await _dashboardService.TotalUsersAsync(status);
        return Ok(totalUsers);
    }

    /// <summary>
    /// API Get total number of workspace - cần đăng nhập
    /// status = 0 - Get all, 1 - Active, 2 - Suspended
    /// </summary>
    /// <returns></returns>
    /// <response code="200">Get thành công</response>
    /// <response code="400">Có lỗi xảy ra</response>
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(int))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [AdminFilter]
    [HttpGet("total-workspaces/{status}")]
    public async Task<ActionResult> TotalWorkspaces([FromRoute] short status)
    {
        var TotalWorkspaces = await _dashboardService.TotalWorkspacesAsync(status);
        return Ok(TotalWorkspaces);
    }

    /// <summary>
    /// API get created date of all users - cần đăng nhập
    /// </summary>
    /// <returns></returns>
    /// <response code="200">Get thành công</response>
    /// <response code="400">Có lỗi xảy ra</response>
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<DateTimeOffset>))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [AdminFilter]
    [HttpGet("user-created-dates")]
    public async Task<ActionResult> GetAllUserCreatedDates()
    {
        var allUserCreatedDates = await _dashboardService.GetAllUserCreatedDatesAsync();
        return Ok(allUserCreatedDates);
    }

    /// <summary>
    /// API get created date of all workspaces - cần đăng nhập
    /// </summary>
    /// <returns></returns>
    /// <response code="200">Get thành công</response>
    /// <response code="400">Có lỗi xảy ra</response>
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<DateTimeOffset>))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [AdminFilter]
    [HttpGet("workspace-created-dates")]
    public async Task<ActionResult> GetAllWorkspaceCreatedDates()
    {
        var allWorkspaceCreatedDates = await _dashboardService.GetAllWorkspaceCreatedDatesAsync();
        return Ok(allWorkspaceCreatedDates);
    }
}