using Microsoft.AspNetCore.Mvc;
using PBL6.Application.Contract.ExternalServices.Meetings.Dtos;
using PBL6.Application.ExternalServices;

namespace PBL6.API.Controllers.Messages;

[ApiController]
[Route("api/[controller]")]
public class MeetingController : ControllerBase
{
    private readonly IMeetingService _meetingService;

    public MeetingController(IMeetingService meetingService)
    {
        _meetingService = meetingService;
    }

    /// <summary>
    /// create session
    /// </summary>
    /// <param name="session"></param>
    /// <returns></returns>
    /// <response code="200">Returns session</response>
    /// <response code="400">If the request is invalid</response>
    /// <response code="500">If there was an internal server error</response>
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(string))]
    [HttpPost("session")]
    public async Task<IActionResult> CreateSession([FromQuery] Session session)
    {
        var sessionId = await _meetingService.CreateSession(session);
        return Ok(sessionId);
    }

    /// <summary>
    /// create token
    /// </summary>
    /// <param name="sessionId"></param>
    /// <returns></returns>
    /// <response code="200">Returns token</response>
    /// <response code="400">If the request is invalid</response>
    /// <response code="500">If there was an internal server error</response>
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(string))]
    [HttpPost("token")]
    public async Task<IActionResult> CreateToken([FromQuery] string sessionId)
    {
        var token = await _meetingService.CreateToken(sessionId);
        return Ok(token);
    }
}
