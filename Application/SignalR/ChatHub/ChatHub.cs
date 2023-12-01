using System.Collections.Concurrent;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using PBL6.Application.Contract.Channels;
using PBL6.Application.Contract.Chats;
using PBL6.Application.Contract.Chats.Dtos;
using PBL6.Application.Contract.Common;
using PBL6.Application.SignalR.ChatHub.Schemas;
using PBL6.Common.Exceptions;
using PBL6.Common.Functions;

namespace PBL6.Application.SignalR.ChatHub
{
    public class ChatHub : Hub
    {
        private readonly IHubContext<ChatHub> _hubContext;
        private readonly ICurrentUserService _currentUserService;
        private readonly IChannelService _channelService;
        private readonly IChatService _chatService;

        private static readonly ConcurrentDictionary<Guid, HubUser> Users = new();

        private const string ERROR = "Error";
        private const string SUCCESS = "Success";
        private const string RECEIVE_MESSAGE = "ReceiveMessage";
        private const string UPDATE_MESSAGE = "UpdateMessage";
        private const string DELETE_MESSAGE = "DeleteMessage";
        private const string ADD_USER_TO_CHANNEL = "AddUserToChannel";
        private const string REMOVE_USER_FROM_CHANNEL = "RemoveUserFromChannel";
        private const string ADD_USER_TO_WORKSPACE = "AddUserToWorkspace";
        private const string REMOVE_USER_FROM_WORKSPACE = "RemoveUserFromWorkspace";

        public ChatHub(
            IHubContext<ChatHub> hubContext,
            ICurrentUserService currentUserService,
            IChannelService channelService,
            IChatService chatService
        )
        {
            _hubContext = hubContext;
            _currentUserService = currentUserService;
            _channelService = channelService;
            _chatService = chatService;
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

        public async Task SendMessageAsync(SendMessageDto input)
        {
            try
            {
                if (_currentUserService.UserId is null)
                {
                    throw new UnauthorizedException("User is not authorized");
                }
                var messageDto = await _chatService.SendMessageAsync(input);
                await Clients.Caller.SendAsync(SUCCESS, messageDto);

                if (input.IsChannel)
                {
                    await _hubContext.Clients
                        .Group(input.ReceiverId.ToString())
                        .SendAsync(RECEIVE_MESSAGE, messageDto);
                    return;
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
            }
            catch (Exception exception)
            {
                HandleException(exception);
            }
        }

        public async Task UpdateMessageAsync(UpdateMessageDto input)
        {
            try
            {
                if (_currentUserService.UserId is null)
                {
                    throw new UnauthorizedException("User is not authorized");
                }
                var messageDto = await _chatService.UpdateMessageAsync(input);
                await Clients.Caller.SendAsync(SUCCESS, messageDto);

                if (input.IsChannel)
                {
                    await _hubContext.Clients
                        .Group(messageDto.ReceiverId.ToString())
                        .SendAsync(UPDATE_MESSAGE, messageDto, messageDto.ReceiverId);
                    return;
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

        public async Task DeleteMessageAsync(Guid id, bool isDeleteEveryone = false)
        {
            try
            {
                if (_currentUserService.UserId is null)
                {
                    await Clients.Caller.SendAsync(ERROR, "User is not authorized");
                    return;
                }
                var messageDto = await _chatService.DeleteMessageAsync(id, isDeleteEveryone);
                if (isDeleteEveryone)
                {
                    if (messageDto.IsChannel)
                    {
                        await _hubContext.Clients
                            .Group(messageDto.ReceiverId.ToString())
                            .SendAsync(DELETE_MESSAGE, messageDto, messageDto.ReceiverId);
                        return;
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
            }
            catch (Exception exception)
            {
                HandleException(exception);
            }
        }

        void HandleException(Exception exception)
        {
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
    }
}
