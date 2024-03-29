using Microsoft.AspNetCore.Mvc;
using PBL6.Application.Contract.Chats;
using PBL6.Application.Contract.Chats.Dtos;
using PBL6.API.Filters;

namespace PBL6.API.Controllers.Messages;

[ApiController]
[Route("api/[controller]")]
public class MessagesController : ControllerBase
{
    private readonly IChatService _chatService;

    public MessagesController(IChatService chatService)
    {
        _chatService = chatService;
    }

    /// <summary>
    /// get messages
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    /// <response code="200">Returns messages</response>
    /// <response code="400">If the request is invalid</response>
    /// <response code="404">If the messages is not found</response>
    /// <response code="403">If the user is not authorized</response>
    /// <response code="500">If there was an internal server error</response>
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<MessageDto>))]
    [HttpGet]
    [AuthorizeFilter]
    public async Task<IActionResult> GetMessages([FromQuery] GetMessageDto input)
    {
        var messages = await _chatService.GetMessagesAsync(input);
        return Ok(messages);
    }

    /// <summary>
    /// get list conversations
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    /// <response code="200">Returns conversations</response>
    /// <response code="400">If the request is invalid</response>
    /// <response code="500">If there was an internal server error</response>
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<ConversationDto>))]
    [HttpGet("conversations")]
    [AuthorizeFilter]
    public async Task<IActionResult> GetConversations([FromQuery] ConversationRequest input)
    {
        List<ConversationDto> conversations = await _chatService.GetConversationsAsync(input);

        return Ok(conversations);
    }

    /// <summary>
    /// get Pin messages
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    /// <response code="200">Returns Pin messages</response>
    /// <response code="400">If the request is invalid</response>
    /// <response code="500">If there was an internal server error</response>
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<MessageDto>))]
    [HttpGet("pin")]
    [AuthorizeFilter]
    public async Task<IActionResult> GetPinMessage([FromQuery] GetPinMessageDto input)
    {
        List<MessageDto> messages = await _chatService.GetPinMessage(input);

        return Ok(messages);
    }

    /// <summary>
    /// jum to message
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    /// <response code="200">Returns Pin messages</response>
    /// <response code="400">If the request is invalid</response>
    /// <response code="500">If there was an internal server error</response>
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<MessageDto>))]
    [HttpGet("jump")]
    [AuthorizeFilter]
    public async Task<IActionResult> JumpToMessage([FromQuery] JumpToMessageDto input)
    {
        List<MessageDto> messages = await _chatService.JumpToMessage(input);

        return Ok(messages);
    }

    /// <summary>
    /// Count unread messages
    /// </summary>
    /// <returns></returns>
    /// <response code="200">Returns Count unread messages</response>
    /// <response code="400">If the request is invalid</response>
    /// <response code="500">If there was an internal server error</response>
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(int))]
    [HttpGet("unread")]
    [AuthorizeFilter]
    public async Task<IActionResult> CountUnreadMessage()
    {
        int count = await _chatService.CountUnreadMessage();

        return Ok(count);
    }


}
