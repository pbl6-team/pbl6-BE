using Application.Contract.Admins;
using Application.Contract.Admins.Dtos;
using Microsoft.AspNetCore.Mvc;
using PBL6.Admin.Filters;
using Microsoft.AspNetCore.Http;

namespace Admin.Controllers.Admin;

[ApiController]
[Route("api/[controller]")]
public class AdminController : ControllerBase
{

    private readonly IAdminService _adminService;

    public AdminController(IAdminService adminService)
    {
        _adminService = adminService;
    }

    /// <summary>
    /// API Get all admins - cần đăng nhập
    /// </summary>
    /// <returns></returns>
    /// <response code="200">Get thành công</response>
    /// <response code="400">Có lỗi xảy ra</response>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<AdminDto>))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [AdminFilter]
    public async Task<IActionResult> GetAll()
    {
        return Ok(await _adminService.GetAllAsync());
    }

    /// <summary>
    /// API Search admin - cần đăng nhập
    /// searchtype : 1 - username, 2 - email, 3 - phone, 4 - fullname, 5 - status
    /// </summary>
    /// <param name="searchType"></param>
    /// <param name="searchValue"></param>
    /// <param name="numberOfResults"></param>
    /// <returns></returns>
    /// <response code="200">Get thành công</response>
    /// <response code="400">Có lỗi xảy ra</response>
    [HttpGet("search/{searchType}/{searchValue}/{numberOfResults}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<AdminDto>))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [AdminFilter]
    public async Task<IActionResult> SearchAdmin([FromRoute] short searchType, [FromRoute] string searchValue, [FromRoute] int numberOfResults)
    {
        return Ok(await _adminService.SearchAsync(searchType, searchValue, numberOfResults));
    }

    /// <summary>
    /// API Update admin - cần đăng nhập
    /// </summary>
    /// <param name="adminId"></param>
    /// <param name="input"></param>
    /// <returns></returns>
    /// <response code="200">Cập nhật thành công</response>
    /// <response code="400">Có lỗi xảy ra</response>
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [AdminFilter]
    [HttpPut("{adminId}")]
    public async Task<ActionResult> UpdateAdmin([FromRoute] Guid adminId, [FromForm] UpdateAdminDto input)
    {
        await _adminService.UpdateAsync(adminId, input);
        return Ok(new { Id = adminId });
    }

    /// <summary>
    /// API Get admin by id - cần đăng nhập
    /// </summary>
    /// <param name="adminId"></param>
    /// <returns></returns>
    /// <response code="200">Get thành công</response>
    /// <response code="400">Có lỗi xảy ra</response>
    [HttpGet("{adminId}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(AdminDetailDto))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [AdminFilter]
    public async Task<IActionResult> GetById([FromRoute] Guid adminId)
    {
        return Ok(await _adminService.GetByIdAsync(adminId));
    }

    /// <summary>
    /// API để tạo admin - cần đăng nhập
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    /// <response code="200">Tạo thành công</response>
    /// <response code="400">Có lỗi xảy ra</response>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesDefaultResponseType]
    [AdminFilter]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Create(CreateAdminDto input)
    {
        return Ok(new { Id = await _adminService.CreateAsync(input) });
    }

    /// <summary>
    /// API update admin status - cần đăng nhập
    /// Status: 1 - Active, 2 - Blocked
    /// </summary>
    /// <returns></returns>
    /// <response code="200">Update thành công</response>
    /// <response code="400">Có lỗi xảy ra</response>
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [AdminFilter]
    [HttpPut("{adminId}/status/{status}")]
    public async Task<ActionResult> UpdateWorkspaceStatus([FromRoute] Guid adminId, [FromRoute] short status)
    {
        await _adminService.UpdateAdminStatusAsync(adminId, status);
        return Ok();
    }
}
