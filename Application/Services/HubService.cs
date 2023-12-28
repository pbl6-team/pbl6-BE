using Microsoft.AspNetCore.SignalR;
using PBL6.Application.Contract.Chats.Dtos;
using PBL6.Application.Hubs;

namespace PBL6.Application.Services
{
    public interface IHubService
    {
        Task AddToGroupAsync(string connectionId, string groupName);
        Task RemoveFromGroupAsync(string connectionId, string groupName);
        Task SendAsync(IEnumerable<string> connectionIds, string method, object arg1);
        Task RemoveUsersFromChannelHub(Guid channelId, List<Guid> userIds);
        Task AddUsersToChannelHub(Guid channelId, List<Guid> userIds);
        Task SendMessage(MessageDto message);
    }

    public class HubService : IHubService
    {
        private readonly IHubContext<ChatHub> _chatHub;

        public HubService(IHubContext<ChatHub> chatHub)
        {
            _chatHub = chatHub;
        }

        public async Task AddToGroupAsync(string connectionId, string groupName)
        {
            await _chatHub.Groups.AddToGroupAsync(connectionId, groupName);
        }

        public async Task RemoveFromGroupAsync(string connectionId, string groupName)
        {
            await _chatHub.Groups.RemoveFromGroupAsync(connectionId, groupName);
        }

        public async Task SendAsync(IEnumerable<string> connectionIds, string method, object arg1)
        {
            await _chatHub.Clients.Clients(connectionIds).SendAsync(method, arg1);
        }

        public async Task RemoveUsersFromChannelHub(Guid channelId, List<Guid> userIds)
        {
            foreach (var userId in userIds)
            {
                var connectionIds = await ChatHub.GetConnectionsByUserId(userId);
                if (connectionIds is not null)
                {
                    await _chatHub.Clients
                        .Clients(connectionIds)
                        .SendAsync(ChatHub.REMOVE_USER_FROM_CHANNEL, channelId);
                    foreach (var connectionId in connectionIds)
                    {
                        await _chatHub.Groups.RemoveFromGroupAsync(
                            connectionId,
                            channelId.ToString()
                        );
                    }
                }
            }
        }

        public async Task AddUsersToChannelHub(Guid channelId, List<Guid> userIds)
        {
            foreach (var userId in userIds)
            {
                var connectionIds = await ChatHub.GetConnectionsByUserId(userId);
                if (connectionIds is not null)
                {
                    foreach (var connectionId in connectionIds)
                    {
                        await AddToGroupAsync(connectionId, channelId.ToString());
                    }

                    await SendAsync(connectionIds, ChatHub.ADD_USER_TO_CHANNEL, channelId);
                }
            }
        }

        public async Task SendMessage(MessageDto message)
        {
            try
            {
                if (message.IsChannel)
                {
                    await _chatHub.Clients
                        .Group(message.ReceiverId.ToString())
                        .SendAsync(ChatHub.RECEIVE_MESSAGE, message);
                }
                else
                {
                    var connectionIds = await ChatHub.GetConnectionsByUserId(message.ReceiverId);
                    if (connectionIds is not null)
                    {
                        await _chatHub.Clients
                            .Clients(connectionIds.ToList())
                            .SendAsync(ChatHub.RECEIVE_MESSAGE, message);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }
}
