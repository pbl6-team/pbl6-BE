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
    /// status = 1 - Active, 2 - Block
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
    /// status = 1 - Active, 2 - Suspended
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
}