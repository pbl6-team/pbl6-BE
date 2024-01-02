using Microsoft.AspNetCore.Mvc;
using PBL6.API.Filters;
using PBL6.Application.Contract.ExternalServices.Meetings.Dtos;
using PBL6.Application.Contract.Meetings.Dtos;
using PBL6.Application.ExternalServices;

namespace PBL6.API.Controllers.Messages;

[ApiController]
[Route("api/[controller]")]
public class MeetingController : ControllerBase
{
    private readonly IMeetingServiceEx _exMeetingService;
    private readonly IMeetingService _meetingService;

    public MeetingController(IMeetingServiceEx meetingService, IMeetingService exMeetingService)
    {
        _exMeetingService = meetingService;
        _meetingService = exMeetingService;
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
        var sessionId = await _exMeetingService.CreateSession(session);
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
        var token = await _exMeetingService.CreateToken(sessionId);
        return Ok(token);
    }

    /// <summary>
    /// CreateMeetingAsync
    /// </summary>
    /// <param name="meeting"></param>
    /// <returns></returns>
    /// <response code="200">Returns meeting</response>
    /// <response code="400">If the request is invalid</response>
    /// <response code="500">If there was an internal server error</response>
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(string))]
    [HttpPost("createMeeting")]
    [AuthorizeFilter]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesDefaultResponseType]
    public async Task<IActionResult> CreateMeetingAsync([FromBody] CreateMeetingDto meeting)
    {
        var meetingDto = await _meetingService.CreateMeetingAsync(meeting);
        return Created("api/meeting/createMeeting", meetingDto);
    }

    /// <summary>
    /// Update meeting
    /// </summary>
    /// <param name="id"></param>
    /// <param name="meeting"></param>
    /// <returns></returns>
    /// <response code="200">Returns meeting</response>
    /// <response code="400">If the request is invalid</response>
    /// <response code="500">If there was an internal server error</response>
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(string))]
    [HttpPut("update/{id}")]
    [AuthorizeFilter]
    public async Task<IActionResult> UpdateMeeting(Guid id, [FromBody] UpdateMeetingDto meeting)
    {
        var meetingDto = await _meetingService.UpdateMeetingAsync(id, meeting);
        return Ok(meetingDto);
    }

    /// <summary>
    /// Join meeting
    /// </summary>
    /// <param name="meeting"></param>
    /// <returns></returns>
    /// <response code="200">Returns meeting</response>
    /// <response code="400">If the request is invalid</response>
    /// <response code="500">If there was an internal server error</response>
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(string))]
    [HttpPost("join")]
    [AuthorizeFilter]
    public async Task<IActionResult> JoinMeeting([FromBody] JoinMeetingDto meeting)
    {
        var meetingDto = await _meetingService.JoinMeetingAsync(meeting);
        return Ok(meetingDto);
    }

    /// <summary>
    /// Make call
    /// </summary>
    /// <param name="call"></param>
    /// <returns></returns>
    /// <response code="200">Returns call</response>
    /// <response code="400">If the request is invalid</response>
    /// <response code="500">If there was an internal server error</response>
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(CallInfoDto))]
    [HttpPost("call")]
    [AuthorizeFilter]
    public async Task<IActionResult> MakeCall([FromBody] MakeCallDto call)
    {
        var callDto = await _meetingService.MakeCallAsync(call);
        return Ok(callDto);
    }

    /// <summary>
    /// End call
    /// </summary>
    /// <param name="meeting"></param>
    /// <returns></returns>
    /// <response code="200">Returns meeting</response>
    /// <response code="400">If the request is invalid</response>
    /// <response code="500">If there was an internal server error</response>
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(string))]
    [HttpPost("endCall")]
    [AuthorizeFilter]
    public async Task<IActionResult> EndCall([FromBody] JoinMeetingDto meeting)
    {
        await _meetingService.EndCallAsync(meeting);
        return Ok();
    }

    /// <summary>
    /// Join call
    /// </summary>
    /// <param name="meeting"></param>
    /// <returns></returns>
    /// <response code="200">Returns meeting</response>
    /// <response code="400">If the request is invalid</response>
    /// <response code="500">If there was an internal server error</response>
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(CallInfoDto))]
    [HttpPost("joinCall")]
    [AuthorizeFilter]
    public async Task<IActionResult> JoinCall([FromBody] JoinMeetingDto meeting)
    {
        var meetingDto = await _meetingService.JoinCallAsync(meeting);
        return Ok(meetingDto);
    }

    /// <summary>
    /// Get meetings
    /// </summary>
    /// <returns></returns>
    /// <response code="200">Returns meetings</response>
    /// <response code="400">If the request is invalid</response>
    /// <response code="500">If there was an internal server error</response>
    [HttpGet("getMeetings")]
    [AuthorizeFilter]
    public async Task<IActionResult> GetMeetings(Guid workspaceId)
    {
        var meetings = await _meetingService.GetMeetingsAsync(workspaceId);
        return Ok(meetings);
    }

    /// <summary>
    /// Get meeting
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    /// <response code="200">Returns meeting</response>
    /// <response code="400">If the request is invalid</response>
    /// <response code="500">If there was an internal server error</response>
    [HttpGet("getMeeting/{id}")]
    [AuthorizeFilter]
    public async Task<IActionResult> GetMeeting(Guid id)
    {
        var meeting = await _meetingService.GetMeetingAsync(id);

        return Ok(meeting);
    }

    /// <summary>
    /// Get meetings by channel id
    /// </summary>
    /// <param name="channelId"></param>
    /// <returns></returns>
    /// <response code="200">Returns meetings</response>
    /// <response code="400">If the request is invalid</response>
    /// <response code="500">If there was an internal server error</response>
    [HttpGet("getMeetingsByChannelId/{channelId}")]
    [AuthorizeFilter]
    public async Task<IActionResult> GetMeetingsByChannelId(Guid channelId)
    {
        var meetings = await _meetingService.GetMeetingsByChannelIdAsync(channelId);
        return Ok(meetings);
    }

    /// <summary>
    /// Delete meeting
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    /// <response code="200">Returns meeting</response>
    /// <response code="400">If the request is invalid</response>
    /// <response code="500">If there was an internal server error</response>
    [HttpDelete("delete/{id}")]
    [AuthorizeFilter]
    public async Task<IActionResult> DeleteMeeting(Guid id)
    {
        await _meetingService.DeleteMeetingAsync(id);
        return Ok();
    }

    /// <summary>
    /// End meeting
    /// </summary>
    /// <param name="meeting"></param>
    /// <returns></returns>
    /// <response code="200">Returns meeting</response>
    /// <response code="400">If the request is invalid</response>
    /// <response code="500">If there was an internal server error</response>
    [HttpPut("endMeeting")]
    [AuthorizeFilter]
    public async Task<IActionResult> EndMeeting([FromBody] JoinMeetingDto meeting)
    {
        await _meetingService.EndMeetingAsync(meeting);
        return Ok();
    }
}
