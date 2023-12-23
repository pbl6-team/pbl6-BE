using System.Collections.Concurrent;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using PBL6.Application.Contract.Channels;
using PBL6.Application.Contract.Chats;
using PBL6.Application.Contract.Chats.Dtos;
using PBL6.Application.Contract.Common;
using PBL6.Application.Hubs.Schemas;
using PBL6.Common.Exceptions;
using PBL6.Common.Functions;

namespace PBL6.Application.Hubs
{
    public class ChatHub : Hub
    {
        private readonly IHubContext<ChatHub> _hubContext;
        private readonly ICurrentUserService _currentUserService;
        private readonly IChannelService _channelService;
        private readonly IChatService _chatService;

        // private readonly ILogger _logger;

        public static readonly ConcurrentDictionary<Guid, HubUser> Users = new();

        public const string ERROR = "Error";
        public const string RECEIVE_MESSAGE = "receive_message";
        public const string UPDATE_MESSAGE = "update_message";
        public const string DELETE_MESSAGE = "delete_message";
        public const string ADD_USER_TO_CHANNEL = "add_user_to_channel";
        public const string REMOVE_USER_FROM_CHANNEL = "remove_user_from_channel";
        public const string ADD_USER_TO_WORKSPACE = "add_user_to_workspace";
        public const string REMOVE_USER_FROM_WORKSPACE = "remove_user_from_workspace";

        public ChatHub(
            IHubContext<ChatHub> hubContext,
            ICurrentUserService currentUserService,
            IChannelService channelService,
            IChatService chatService
        // ILogger logger
        )
        {
            _hubContext = hubContext;
            _currentUserService = currentUserService;
            _channelService = channelService;
            _chatService = chatService;
            // _logger = logger;
        }

        public override async Task OnConnectedAsync()
        {
            try
            {
                var httpContext = Context.GetHttpContext();
                var configuration = httpContext.RequestServices.GetService<IConfiguration>();
                var token = httpContext.Request.Query["access_token"].ToString();
                if (string.IsNullOrEmpty(token))
                {
                    throw new UnauthorizedException("Token is required");
                }

                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.ASCII.GetBytes(configuration["Jwt:SecretKey"]);
                tokenHandler.ValidateToken(
                    token,
                    new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(key),
                        ValidateIssuer = false,
                        ValidateAudience = false,
                    },
                    out SecurityToken validatedToken
                );

                var jwtToken =
                    (JwtSecurityToken)validatedToken ?? throw new UnauthorizedAccessException();
                var claims = jwtToken.Claims;
                var userId = Guid.Parse(
                    claims.FirstOrDefault(x => x.Type == CustomClaimTypes.UserId)?.Value
                );
                var email = claims.FirstOrDefault(x => x.Type == CustomClaimTypes.Email)?.Value;
                var isVerified = claims
                    .FirstOrDefault(x => x.Type == CustomClaimTypes.IsActive)
                    ?.Value;
                if (isVerified is not null && isVerified.Equals("False"))
                {
                    throw new UnauthorizedException("User is not verified");
                }
                var identity = new ClaimsIdentity(httpContext.User.Identity);
                identity.AddClaim(new Claim(CustomClaimTypes.UserId, userId.ToString()));
                identity.AddClaim(new Claim(CustomClaimTypes.Email, email));

                var principal = new ClaimsPrincipal(identity);
                httpContext.User = principal;

                var connectionId = Context.ConnectionId;
                var userConnection = Users.GetOrAdd(
                    userId,
                    new HubUser { UserId = userId, ConnectionIds = new HashSet<string>() }
                );
                lock (userConnection.ConnectionIds)
                {
                    userConnection.ConnectionIds.Add(connectionId);
                }

                var channelIds = await _channelService.GetChannelsOfUserAsync(userId);
                foreach (var channelId in channelIds)
                {
                    await Groups.AddToGroupAsync(connectionId, channelId.ToString());
                }

                await base.OnConnectedAsync();
            }
            catch (Exception exception)
            {
                HandleException(exception);
            }
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            try
            {
                if (_currentUserService.UserId is null)
                {
                    throw new UnauthorizedException("User is not authorized");
                }
                var userId = Guid.Parse(_currentUserService.UserId);
                var connectionId = Context.ConnectionId;
                var userConnection = Users.GetOrAdd(
                    userId,
                    new HubUser { UserId = userId, ConnectionIds = new HashSet<string>() }
                );
                lock (userConnection.ConnectionIds)
                {
                    userConnection.ConnectionIds.Remove(connectionId);
                }

                var channelIds = await _channelService.GetChannelsOfUserAsync(userId);
                foreach (var channelId in channelIds)
                {
                    await Groups.RemoveFromGroupAsync(connectionId, channelId.ToString());
                }

                HandleException(exception);
                await base.OnDisconnectedAsync(exception);
            }
            catch (Exception e)
            {
                HandleException(e);
            }
        }

        public async Task<MessageDto> SendMessageAsync(SendMessageDto input)
        {
            try
            {
                if (_currentUserService.UserId is null)
                {
                    throw new UnauthorizedException("User is not authorized");
                }
                MessageDto messageDto = await _chatService.SendMessageAsync(input);

                if (input.IsChannel)
                {
                    await _hubContext.Clients
                        .Group(input.ReceiverId.ToString())
                        .SendAsync(RECEIVE_MESSAGE, messageDto);
                }
                else
                {
                    Users.TryGetValue(input.ReceiverId, out var hubUser);
                    if (hubUser is not null && hubUser.ConnectionIds.Any())
                    {
                        await _hubContext.Clients
                            .Clients(hubUser.ConnectionIds.ToList())
                            .SendAsync(RECEIVE_MESSAGE, messageDto);
                    }
                }
                return messageDto;
            }
            catch (Exception exception)
            {
                HandleException(exception);
                return null;
            }
        }

        public async Task ReadMessage(Guid messageId)
        {
            try
            {
                if (_currentUserService.UserId is null)
                {
                    throw new UnauthorizedException("User is not authorized");
                }
                MessageDto messageDto = await _chatService.ReadMessageAsync(messageId);

                if (messageDto.IsChannel)
                {
                    await _hubContext.Clients
                        .Group(messageDto.ReceiverId.ToString())
                        .SendAsync(UPDATE_MESSAGE, messageDto, messageDto.ReceiverId);
                }
                else
                {
                    Users.TryGetValue(messageDto.ReceiverId, out var hubUser);
                    if (hubUser is not null && hubUser.ConnectionIds.Any())
                    {
                        await _hubContext.Clients
                            .Clients(hubUser.ConnectionIds.ToList())
                            .SendAsync(UPDATE_MESSAGE, messageDto);
                    }

                    Users.TryGetValue(messageDto.SenderId, out hubUser);
                    if (hubUser is not null && hubUser.ConnectionIds.Any())
                    {
                        await _hubContext.Clients
                            .Clients(hubUser.ConnectionIds.ToList())
                            .SendAsync(UPDATE_MESSAGE, messageDto);
                    }
                }
            }
            catch (Exception exception)
            {
                HandleException(exception);
            }
        }

        public async Task DeleteFileAsync(IEnumerable<Guid> ids)
        {
            try
            {
                if (_currentUserService.UserId is null)
                {
                    throw new UnauthorizedException("User is not authorized");
                }
                MessageDto messageDto = await _chatService.DeleteFile(ids);
                if (messageDto is not null)
                {
                    if (messageDto.IsChannel)
                    {
                        await _hubContext.Clients
                            .Group(messageDto.ReceiverId.ToString())
                            .SendAsync(UPDATE_MESSAGE, messageDto, messageDto.ReceiverId);
                    }
                    else
                    {
                        Users.TryGetValue(messageDto.ReceiverId, out var hubUser);
                        if (hubUser is not null && hubUser.ConnectionIds.Any())
                        {
                            await _hubContext.Clients
                                .Clients(hubUser.ConnectionIds.ToList())
                                .SendAsync(UPDATE_MESSAGE, messageDto);
                        }
                        Users.TryGetValue(messageDto.SenderId, out var hubUser);
                        if (hubUser is not null && hubUser.ConnectionIds.Any())
                        {
                            await _hubContext.Clients
                                .Clients(hubUser.ConnectionIds.ToList())
                                .SendAsync(UPDATE_MESSAGE, messageDto);
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                HandleException(exception);
            }
        }

        public async Task ReactMessageAsync(ReactMessageDto input)
        {
            try
            {
                if (_currentUserService.UserId is null)
                {
                    throw new UnauthorizedException("User is not authorized");
                }
                MessageDto messageDto = await _chatService.ReactMessageAsync(input);

                if (messageDto.IsChannel)
                {
                    await _hubContext.Clients
                        .Group(messageDto.ReceiverId.ToString())
                        .SendAsync(UPDATE_MESSAGE, messageDto, messageDto.ReceiverId);
                }
                else
                {
                    Users.TryGetValue(messageDto.ReceiverId, out var hubUser);
                    if (hubUser is not null && hubUser.ConnectionIds.Any())
                    {
                        await _hubContext.Clients
                            .Clients(hubUser.ConnectionIds.ToList())
                            .SendAsync(UPDATE_MESSAGE, messageDto);
                    }

                    Users.TryGetValue(messageDto.SenderId, out hubUser);
                    if (hubUser is not null && hubUser.ConnectionIds.Any())
                    {
                        await _hubContext.Clients
                            .Clients(hubUser.ConnectionIds.ToList())
                            .SendAsync(UPDATE_MESSAGE, messageDto);
                    }
                }
            }
            catch (Exception exception)
            {
                HandleException(exception);
            }
        }

        public async Task<Guid?> UpdateMessageAsync(UpdateMessageDto input)
        {
            try
            {
                if (_currentUserService.UserId is null)
                {
                    throw new UnauthorizedException("User is not authorized");
                }
                var messageDto = await _chatService.UpdateMessageAsync(input);

                if (input.IsChannel)
                {
                    await _hubContext.Clients
                        .Group(messageDto.ReceiverId.ToString())
                        .SendAsync(UPDATE_MESSAGE, messageDto, messageDto.ReceiverId);
                }
                else
                {
                    Users.TryGetValue(messageDto.ReceiverId, out var hubUser);
                    if (hubUser is not null && hubUser.ConnectionIds.Any())
                    {
                        await _hubContext.Clients
                            .Clients(hubUser.ConnectionIds.ToList())
                            .SendAsync(UPDATE_MESSAGE, messageDto);
                    }

                    Users.TryGetValue(messageDto.SenderId, out hubUser);
                    if (hubUser is not null && hubUser.ConnectionIds.Any())
                    {
                        await _hubContext.Clients
                            .Clients(hubUser.ConnectionIds.ToList())
                            .SendAsync(UPDATE_MESSAGE, messageDto);
                    }
                }
                return messageDto.Id;
            }
            catch (Exception exception)
            {
                HandleException(exception);
                return null;
            }
        }

        public async Task<Guid?> DeleteMessageAsync(Guid id, bool isDeleteEveryone = false)
        {
            try
            {
                if (_currentUserService.UserId is null)
                {
                    throw new UnauthorizedException("User is not authorized");
                }
                var messageDto = await _chatService.DeleteMessageAsync(id, isDeleteEveryone);
                if (isDeleteEveryone)
                {
                    if (messageDto.IsChannel)
                    {
                        await _hubContext.Clients
                            .Group(messageDto.ReceiverId.ToString())
                            .SendAsync(DELETE_MESSAGE, messageDto, messageDto.ReceiverId);
                    }
                    else
                    {
                        Users.TryGetValue(messageDto.ReceiverId, out var hubUser);
                        if (hubUser is not null && hubUser.ConnectionIds.Any())
                        {
                            await _hubContext.Clients
                                .Clients(hubUser.ConnectionIds.ToList())
                                .SendAsync(DELETE_MESSAGE, messageDto);
                        }
                    }
                }
                else
                {
                    Users.TryGetValue(Guid.Parse(_currentUserService.UserId), out var hubUser);
                    if (hubUser is not null && hubUser.ConnectionIds.Any())
                    {
                        await _hubContext.Clients
                            .Clients(hubUser.ConnectionIds.ToList())
                            .SendAsync(DELETE_MESSAGE, messageDto);
                    }
                }
                return messageDto.Id;
            }
            catch (Exception exception)
            {
                HandleException(exception);
                return null;
            }
        }

        void HandleException(Exception exception)
        {
            // _logger.LogError($"Error: {exception.Message}");
            // _logger.LogError(exception.StackTrace);

            ProblemDetails problemDetails = new();
            if (exception is CustomException customException)
            {
                problemDetails.Title = customException.Message;
                problemDetails.Status = customException.StatusCode;
            }
            else
            {
                problemDetails.Title = exception.Message;
                problemDetails.Status = 500;
            }
            Clients.Caller.SendAsync(ERROR, problemDetails);
        }

        public async Task AddUserToChannelAsync(Guid channelId, List<Guid> userIds)
        {
            try
            {
                foreach (var userId in userIds)
                {
                    Users.TryGetValue(userId, out var hubUser);
                    if (hubUser is not null && hubUser.ConnectionIds.Any())
                    {
                        await Clients
                            .Clients(hubUser.ConnectionIds.ToList())
                            .SendAsync(ADD_USER_TO_CHANNEL, channelId);

                        foreach (var connectionId in hubUser.ConnectionIds)
                        {
                            await _hubContext.Groups.AddToGroupAsync(
                                connectionId,
                                channelId.ToString()
                            );
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                HandleException(exception);
            }
        }

        public async Task RemoveUserFromChannelAsync(Guid channelId, List<Guid> userIds)
        {
            try
            {
                if (_currentUserService.UserId is null)
                {
                    throw new UnauthorizedException("User is not authorized");
                }

                foreach (var userId in userIds)
                {
                    Users.TryGetValue(userId, out var hubUser);
                    if (hubUser is not null && hubUser.ConnectionIds.Any())
                    {
                        await Clients
                            .Clients(hubUser.ConnectionIds.ToList())
                            .SendAsync(REMOVE_USER_FROM_CHANNEL, channelId);

                        foreach (var connectionId in hubUser.ConnectionIds)
                        {
                            await _hubContext.Groups.RemoveFromGroupAsync(
                                connectionId,
                                channelId.ToString()
                            );
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                HandleException(exception);
            }
        }

        public async Task AddUserToWorkspaceAsync(Guid workspaceId, List<Guid> userIds)
        {
            try
            {
                if (_currentUserService.UserId is null)
                {
                    throw new UnauthorizedException("User is not authorized");
                }
                foreach (var userId in userIds)
                {
                    Users.TryGetValue(userId, out var hubUser);
                    if (hubUser is not null && hubUser.ConnectionIds.Any())
                    {
                        await Clients
                            .Clients(hubUser.ConnectionIds.ToList())
                            .SendAsync(ADD_USER_TO_WORKSPACE, workspaceId);
                    }
                }
            }
            catch (Exception exception)
            {
                HandleException(exception);
            }
        }

        public async Task RemoveUserFromWorkspaceAsync(Guid workspaceId, List<Guid> userIds)
        {
            try
            {
                if (_currentUserService.UserId is null)
                {
                    throw new UnauthorizedException("User is not authorized");
                }
                foreach (var userId in userIds)
                {
                    Users.TryGetValue(userId, out var hubUser);
                    if (hubUser is not null && hubUser.ConnectionIds.Any())
                    {
                        await Clients
                            .Clients(hubUser.ConnectionIds.ToList())
                            .SendAsync(REMOVE_USER_FROM_WORKSPACE, workspaceId);
                    }
                }
            }
            catch (Exception exception)
            {
                HandleException(exception);
            }
        }

        public async Task PinMessage(Guid messageId, bool isPin = true)
        {
            try
            {
                if (_currentUserService.UserId is null)
                {
                    throw new UnauthorizedException("User is not authorized");
                }
                MessageDto messageDto = await _chatService.PinMessage(messageId, isPin);

                if (messageDto.IsChannel)
                {
                    await _hubContext.Clients
                        .Group(messageDto.ReceiverId.ToString())
                        .SendAsync(UPDATE_MESSAGE, messageDto, messageDto.ReceiverId);
                }
                else
                {
                    Users.TryGetValue(messageDto.ReceiverId, out var hubUser);
                    if (hubUser is not null && hubUser.ConnectionIds.Any())
                    {
                        await _hubContext.Clients
                            .Clients(hubUser.ConnectionIds.ToList())
                            .SendAsync(UPDATE_MESSAGE, messageDto);
                    }

                    Users.TryGetValue(messageDto.SenderId, out hubUser);
                    if (hubUser is not null && hubUser.ConnectionIds.Any())
                    {
                        await _hubContext.Clients
                            .Clients(hubUser.ConnectionIds.ToList())
                            .SendAsync(UPDATE_MESSAGE, messageDto);
                    }
                }
            }
            catch (Exception exception)
            {
                HandleException(exception);
            }
        }

        public static Task<IEnumerable<string>> GetConnectionsByUserId(Guid userId)
        {
            Users.TryGetValue(userId, out var hubUser);
            return Task.FromResult(
                hubUser?.ConnectionIds.AsEnumerable() ?? Enumerable.Empty<string>()
            );
        }

        public static Task<bool> IsUserOnline(Guid userId)
        {
            Users.TryGetValue(userId, out var hubUser);
            return Task.FromResult(hubUser?.ConnectionIds.Any() ?? false);
        }
    }
}
