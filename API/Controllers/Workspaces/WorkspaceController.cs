using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PBL6.Application.Contract.Users.Dtos;
using PBL6.Application.Contract.Workspaces;
using PBL6.Application.Contract.Workspaces.Dtos;

namespace PBL6.API.Controllers.Workspaces
{
    [Produces("application/json")]
    [Route("api/[controller]")]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public class WorkspaceController : Controller
    {
        private readonly IWorkspaceService _workspaceService;

        public WorkspaceController(IWorkspaceService workspaceService) => _workspaceService = workspaceService;

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
        [Authorize]
        public async Task<IActionResult> Create(CreateWorkspaceDto input)
        {
            try
            {
                return Ok(await _workspaceService.AddAsync(input));
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
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
        [Authorize]
        public async Task<IActionResult> Update(Guid workspaceId, UpdateWorkspaceDto input)
        {
            try
            {
                return Ok(await _workspaceService.UpdateAsync(workspaceId, input));
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        /// <summary>
        /// API Update avatar workspace - cần đăng nhập
        /// </summary>
        /// <param name="workspaceId"></param>
        /// <param name="input"></param>
        /// <returns></returns>
        /// <response code="200">Cập nhật thành công</response>
        /// <response code="400">Có lỗi xảy ra</response>
        [HttpPut("avatar")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [Authorize]
        public async Task<IActionResult> UpdateAvatar(Guid workspaceId, UpdateAvatarWorkspaceDto input)
        {
            try
            {
                return Ok(await _workspaceService.UpdateAvatarAsync(workspaceId, input));
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        /// <summary>
        /// API delete workspace - cần đăng nhập
        /// </summary>
        /// <param name="workspaceId"></param>
        /// <returns></returns>
        /// <response code="200">Xoá thành công</response>
        /// <response code="400">Có lỗi xảy ra</response>
        [HttpDelete]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [Authorize]
        public async Task<IActionResult> Delete(Guid workspaceId)
        {
            try
            {
                return Ok(await _workspaceService.DeleteAsync(workspaceId));
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
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
        [Authorize]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                return Ok(await _workspaceService.GetAllAsync());
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
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
        [Authorize]
        public async Task<IActionResult> GetById(Guid workspaceId)
        {
            try
            {
                return Ok(await _workspaceService.GetByIdAsync(workspaceId));
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
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
        [Authorize]
        public async Task<IActionResult> GetByName(string workspaceName)
        {
            try
            {
                return Ok(await _workspaceService.GetByNameAsync(workspaceName));
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        /// <summary>
        /// API Add member to workspace - cần đăng nhập
        /// </summary>
        /// <param name="workspaceId"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        /// <response code="200">Add thành công</response>
        /// <response code="400">Có lỗi xảy ra</response>
        [HttpPost("addmember")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [Authorize]
        public async Task<IActionResult> AddMember(Guid workspaceId, Guid userId)
        {
            try
            {
                return Ok(await _workspaceService.AddMemberToWorkspaceAsync(workspaceId, userId));
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        /// <summary>
        /// API Remove member from workspace - cần đăng nhập
        /// </summary>
        /// <param name="workspaceId"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        /// <response code="200">Remove thành công</response>
        /// <response code="400">Có lỗi xảy ra</response>
        [HttpPost("removemember")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [Authorize]
        public async Task<IActionResult> RemoveMember(Guid workspaceId, Guid userId)
        {
            try
            {
                return Ok(await _workspaceService.RemoveMemberFromWorkspaceAsync(workspaceId, userId));
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }
    }
}