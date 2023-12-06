using AutoMapper;
using Hangfire;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PBL6.Application.Contract.Common;
using PBL6.Application.Hubs;
using PBL6.Domain.Data;

namespace PBL6.Application.Services
{
    public class BaseService
    {
        protected readonly IMailService _mailService;
        protected readonly IConfiguration _config;
        protected readonly ICurrentUserService _currentUser;
        protected readonly IMapper _mapper;
        protected readonly ILogger<BaseService> _logger;
        protected readonly IUnitOfWork _unitOfWork;
        protected readonly IWebHostEnvironment _hostingEnvironment;
        protected readonly IFileService _fileService;
        protected readonly IBackgroundJobClient _backgroundJobClient;
        protected readonly IHubContext<ChatHub> _chatHub;

        public BaseService(IServiceProvider serviceProvider)
        {
            _mailService = serviceProvider.GetService<IMailService>();
            _config = serviceProvider.GetService<IConfiguration>();
            _logger = serviceProvider.GetService<ILogger<BaseService>>();
            _currentUser = serviceProvider.GetService<ICurrentUserService>();
            _mapper = serviceProvider.GetService<IMapper>();
            _unitOfWork = serviceProvider.GetService<IUnitOfWork>();
            _hostingEnvironment = serviceProvider.GetService<IWebHostEnvironment>();
            _fileService = serviceProvider.GetService<IFileService>();
            _chatHub = serviceProvider.GetService<IHubContext<ChatHub>>();
            _backgroundJobClient = serviceProvider.GetService<IBackgroundJobClient>();
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
                        _backgroundJobClient.Enqueue(
                            () => AddToGroupAsync(connectionId, channelId.ToString())
                        );
                    }

                    _backgroundJobClient.Enqueue(
                        () => SendAsync(connectionIds, ChatHub.ADD_USER_TO_CHANNEL, channelId)
                    );
                }
            }
        }
    }
}
