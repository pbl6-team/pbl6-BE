using System.Runtime.CompilerServices;
using Hangfire;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using PBL6.Application.Contract.Chats;
using PBL6.Application.Contract.Chats.Dtos;
using PBL6.Application.Hubs;
using PBL6.Common.Consts;
using PBL6.Common.Enum;
using PBL6.Common.Exceptions;
using PBL6.Common.Functions;
using PBL6.Domain.Models.Users;

namespace PBL6.Application.Services
{
    public class ChatService : BaseService, IChatService
    {
        private readonly string _className;

        public ChatService(IServiceProvider serviceProvider)
            : base(serviceProvider)
        {
            _className = nameof(ChatService);
        }

        private static string GetActualAsyncMethodName([CallerMemberName] string name = null)
        {
            return name;
        }

        public async Task<int> CountUnreadMessage()
        {
            var method = GetActualAsyncMethodName();
            _logger.LogInformation("[{_className}][{method}] Start", _className, method);
            var currentUserId = Guid.Parse(_currentUser.UserId);
            var count = await _unitOfWork.Messages.CountUnreadMessage(currentUserId);
            _logger.LogInformation("[{_className}][{method}] End", _className, method);

            return count;
        }

        public async Task<MessageDto> DeleteFile(IEnumerable<Guid> ids)
        {
            var method = GetActualAsyncMethodName();
            _logger.LogInformation("[{_className}][{method}] Start", _className, method);
            if (!ids.Any())
            {
                return null;
            }
            var currentUserId = Guid.Parse(
                _currentUser.UserId ?? throw new UnauthorizedException("User is not authorized")
            );
            var currentUser = await _unitOfWork.Users.FindAsync(currentUserId);
            List<string> urls = new();
            var message =
                await _unitOfWork.Messages.GetMessageByFileIds(ids)
                ?? throw new NotFoundException<FileDomain>(ids.First().ToString());
            foreach (var id in ids)
            {
                if (message.CreatedBy != currentUserId)
                {
                    throw new ForbidException();
                }
                var file = message.Files.FirstOrDefault(x => x.Id == id);

                if (file is not null)
                {
                    urls.Add(file.Url);
                    message.Files.Remove(file);
                }
            }
            message.IsEdited = true;
            await _unitOfWork.Messages.UpdateAsync(message);
            await _fileService.DeleteFileUrlAsync(urls);
            await _unitOfWork.SaveChangeAsync();
            MessageDto messageDto = _mapper.Map<MessageDto>(message);
            _logger.LogInformation("[{_className}][{method}] End", _className, method);

            return messageDto;
        }

        public async Task<MessageDto> DeleteMessageAsync(Guid id, bool isDeleteEveryone = false)
        {
            var method = GetActualAsyncMethodName();
            _logger.LogInformation("[{_className}][{method}] Start", _className, method);
            var currentUserId = Guid.Parse(
                _currentUser.UserId ?? throw new UnauthorizedException("User is not authorized")
            );
            var message =
                await _unitOfWork.Messages.Get(id)
                ?? throw new NotFoundException<Message>(id.ToString());
            var isInConversation = await _unitOfWork.Messages.CheckUserInConversation(
                currentUserId,
                message.Id
            );
            if (!isInConversation)
            {
                throw new NotFoundException<Message>(id.ToString());
            }

            if (isDeleteEveryone)
            {
                if (message.ToChannelId is not null)
                {
                    var permissions = await _unitOfWork.Channels.GetPermissionsOfUser(
                        currentUserId,
                        message.ToChannelId.Value
                    );
                    if (
                        !permissions.Any(x => x.Code == ChannelPolicy.DELETE_OTHER_PEOPLE_S_MESSAGE)
                        && message.ToChannel.CreatedBy != currentUserId
                        && message.CreatedBy != currentUserId
                    )
                    {
                        throw new ForbidException();
                    }
                }
                else
                {
                    if (message.CreatedBy != currentUserId)
                    {
                        throw new ForbidException();
                    }
                }

                message.IsDeleted = true;
                await _unitOfWork.Messages.UpdateAsync(message);
                await _unitOfWork.SaveChangeAsync();
                MessageDto messageDto = _mapper.Map<MessageDto>(message);
                _logger.LogInformation("[{_className}][{method}] End", _className, method);

                return messageDto;
            }

            var messageTracking = message.MessageTrackings.FirstOrDefault(
                x => x.UserId == currentUserId
            );
            if (messageTracking is null)
            {
                message.MessageTrackings.Add(
                    new MessageTracking { UserId = currentUserId, IsDeleted = true }
                );
            }
            else
            {
                messageTracking.IsDeleted = true;
            }

            await _unitOfWork.Messages.UpdateAsync(message);
            await _unitOfWork.SaveChangeAsync();

            _logger.LogInformation("[{_className}][{method}] End", _className, method);

            return null;
        }

        public async Task<List<ConversationDto>> GetConversationsAsync(ConversationRequest input)
        {
            var method = GetActualAsyncMethodName();
            _logger.LogInformation("[{_className}][{method}] Start", _className, method);
            var currentUserId = Guid.Parse(
                _currentUser.UserId ?? throw new UnauthorizedException("User is not authorized")
            );
            var conversationDtos = await _unitOfWork.Messages
                .GetConversations(currentUserId, input.Search)
                .OrderByDescending(x => x.CreatedAt)
                .GroupBy(x => x.ToUserId == currentUserId ? x.CreatedBy : x.ToUserId)
                .OrderByDescending(x => x.First().CreatedAt)
                .Select(
                    x =>
                        new ConversationDto
                        {
                            Id = (Guid)x.Key,
                            Name =
                                currentUserId
                                == x.OrderByDescending(x => x.CreatedAt).First().CreatedBy
                                    ? x.First().Receiver.Information.FirstName
                                        + " "
                                        + x.OrderByDescending(x => x.CreatedAt)
                                            .First()
                                            .Receiver.Information.LastName
                                    : x.OrderByDescending(x => x.CreatedAt)
                                        .First()
                                        .Sender.Information.FirstName
                                        + " "
                                        + x.OrderByDescending(x => x.CreatedAt)
                                            .First()
                                            .Sender.Information.LastName,
                            LastMessage = x.OrderByDescending(x => x.CreatedAt).First().Content,
                            LastMessageTime = x.OrderByDescending(x => x.CreatedAt)
                                .First()
                                .CreatedAt,
                            LastMessageSender =
                                currentUserId
                                == x.OrderByDescending(x => x.CreatedAt).First().CreatedBy
                                    ? "You"
                                    : x.OrderByDescending(x => x.CreatedAt)
                                        .First()
                                        .Receiver.Information.FirstName
                                        + " "
                                        + x.OrderByDescending(x => x.CreatedAt)
                                            .First()
                                            .Receiver.Information.LastName,
                            LastMessageSenderAvatar = x.OrderByDescending(x => x.CreatedAt)
                                .First()
                                .Sender.Information.Picture,
                            IsRead =
                                x.OrderByDescending(x => x.CreatedAt).First().CreatedBy
                                    == currentUserId
                                || x.OrderByDescending(x => x.CreatedAt)
                                    .First()
                                    .MessageTrackings.Any(
                                        x => x.UserId == currentUserId && !x.IsDeleted && x.IsRead
                                    ),
                            IsChannel =
                                x.OrderByDescending(x => x.CreatedAt).First().ToChannelId != null,
                            Avatar =
                                currentUserId
                                == x.OrderByDescending(x => x.CreatedAt).First().CreatedBy
                                    ? x.OrderByDescending(x => x.CreatedAt)
                                        .First()
                                        .Receiver.Information.Picture
                                    : x.OrderByDescending(x => x.CreatedAt)
                                        .First()
                                        .Sender.Information.Picture,
                        }
                )
                .OrderByDescending(x => x.LastMessageTime)
                .Skip(input.Offset)
                .Take(input.Limit)
                .ToListAsync();

            foreach (var conversationDto in conversationDtos)
            {
                conversationDto.IsOnline = await ChatHub.IsUserOnline(conversationDto.Id);
            }

            _logger.LogInformation("[{_className}][{method}] End", _className, method);

            return conversationDtos;
        }

        public async Task<IEnumerable<FileInfoDto>> GetFilesAsync(GetFileDto input)
        {
            var method = GetActualAsyncMethodName();
            _logger.LogInformation("[{_className}][{method}] Start", _className, method);
            var currentUserId = Guid.Parse(
                _currentUser.UserId ?? throw new UnauthorizedException("User is not authorized")
            );
            IEnumerable<FileInfoDto> fileInfos = new List<FileInfoDto>();
            if (input.ToChannelId is not null)
            {
                var isMember = _unitOfWork.Channels.CheckIsMemberAsync(
                    input.ToChannelId.Value,
                    currentUserId
                );
                if (!isMember.Result)
                {
                    throw new NotFoundException<Channel>(input.ToChannelId.Value.ToString());
                }

                fileInfos = (
                    await _unitOfWork.Messages.GetMessagesOfChannelAsync(
                        currentUserId,
                        input.ToChannelId.Value,
                        null,
                        DateTimeOffset.UtcNow,
                        int.MaxValue
                    )
                )
                    .SelectMany(x => x.Files)
                    .Select(
                        x =>
                            new FileInfoDto
                            {
                                Id = x.Id,
                                Name = x.Name,
                                Type = x.Type,
                                Url = x.Url,
                                CreatedAt = x.CreatedAt
                            }
                    )
                    .OrderByDescending(x => x.CreatedAt)
                    .Skip(input.Offset)
                    .Take(input.Limit)
                    .ToList();
            }
            else if (input.ToUserId is not null)
            {
                var isUser = _unitOfWork.Users.CheckIsUserAsync(input.ToUserId.Value);
                if (!isUser.Result)
                {
                    throw new NotFoundException<User>(input.ToUserId.ToString());
                }

                fileInfos = (
                    await _unitOfWork.Messages.GetMessagesOfUserAsync(
                        currentUserId,
                        input.ToUserId.Value,
                        null,
                        DateTimeOffset.UtcNow,
                        int.MaxValue
                    )
                )
                    .SelectMany(x => x.Files)
                    .Select(
                        x =>
                            new FileInfoDto
                            {
                                Id = x.Id,
                                Name = x.Name,
                                Type = x.Type,
                                Url = x.Url,
                                CreatedAt = x.CreatedAt
                            }
                    )
                    .OrderByDescending(x => x.CreatedAt)
                    .Skip(input.Offset)
                    .Take(input.Limit)
                    .ToList();
            }

            _logger.LogInformation("[{_className}][{method}] End", _className, method);

            return fileInfos;
        }

        public async Task<List<MessageDto>> GetMessagesAsync(GetMessageDto input)
        {
            var method = GetActualAsyncMethodName();
            _logger.LogInformation("[{_className}][{method}] Start", _className, method);
            var currentUserId = Guid.Parse(
                _currentUser.UserId ?? throw new UnauthorizedException("User is not authorized")
            );
            List<MessageDto> messageDtos;
            IEnumerable<Message> messages = new List<Message>();
            if (input.ToChannelId is not null)
            {
                var isMember = await _unitOfWork.Channels.CheckIsMemberAsync(
                    input.ToChannelId.Value,
                    currentUserId
                );
                if (!isMember)
                {
                    throw new NotFoundException<Channel>(input.ToChannelId.Value.ToString());
                }

                messages = await _unitOfWork.Messages.GetMessagesOfChannelAsync(
                    currentUserId,
                    input.ToChannelId.Value,
                    input.ParentId,
                    input.TimeCursor,
                    input.Count,
                    input.IsBefore
                );
            }
            else if (input.ToUserId is not null)
            {
                messages = await _unitOfWork.Messages.GetMessagesOfUserAsync(
                    currentUserId,
                    input.ToUserId.Value,
                    input.ParentId,
                    input.TimeCursor,
                    input.Count,
                    input.IsBefore
                );
            }
            messageDtos = _mapper.Map<List<MessageDto>>(messages);
            foreach (var (message, messageDto) in messages.Zip(messageDtos))
            {
                var reactions = message.MessageTrackings
                    .Where(x => !x.IsDeleted)
                    .Select(x => x.Reaction);
                messageDto.ReactionCount = CommonFunctions.GetReactionCount(reactions);
            }

            _logger.LogInformation("[{_className}][{method}] End", _className, method);

            return messageDtos;
        }

        public async Task<List<MessageDto>> GetPinMessage(GetPinMessageDto input)
        {
            var method = GetActualAsyncMethodName();
            _logger.LogInformation("[{_className}][{method}] Start", _className, method);
            var currentUserId = Guid.Parse(
                _currentUser.UserId ?? throw new UnauthorizedException("User is not authorized")
            );
            var messageDtos = new List<MessageDto>();
            if (input.ToChannelId is not null)
            {
                var isMember = await _unitOfWork.Channels.CheckIsMemberAsync(
                    input.ToChannelId.Value,
                    currentUserId
                );
                if (!isMember)
                {
                    throw new NotFoundException<Channel>(input.ToChannelId.Value.ToString());
                }

                IEnumerable<Message> messages =
                    await _unitOfWork.Messages.GetPinMessagesOfChannelAsync(
                        currentUserId,
                        input.ToChannelId.Value,
                        input.Offset,
                        input.Limit
                    );
                messageDtos = _mapper.Map<List<MessageDto>>(messages);
            }
            else if (input.ToUserId is not null)
            {
                IEnumerable<Message> messages =
                    await _unitOfWork.Messages.GetPinMessagesOfUserAsync(
                        currentUserId,
                        input.ToUserId.Value,
                        input.Offset,
                        input.Limit
                    );
                messageDtos = _mapper.Map<List<MessageDto>>(messages);
            }

            _logger.LogInformation("[{_className}][{method}] End", _className, method);

            return messageDtos;
        }

        public async Task<List<MessageDto>> JumpToMessage(JumpToMessageDto input)
        {
            var method = GetActualAsyncMethodName();
            _logger.LogInformation("[{_className}][{method}] Start", _className, method);
            var currentUserId = Guid.Parse(
                _currentUser.UserId ?? throw new UnauthorizedException("User is not authorized")
            );

            var isInConversation = await _unitOfWork.Messages.CheckUserInConversation(
                currentUserId,
                input.MessageId
            );
            if (!isInConversation)
            {
                throw new NotFoundException<Message>(input.MessageId.ToString());
            }

            var message =
                await _unitOfWork.Messages.Get(input.MessageId)
                ?? throw new NotFoundException<Message>(input.MessageId.ToString());
            List<MessageDto> messageDtos = await GetMessagesAsync(
                new GetMessageDto
                {
                    ToChannelId = message.ToChannelId,
                    ToUserId = message.ToUserId,
                    ParentId = message.ParentId,
                    TimeCursor = message.CreatedAt,
                    Count = 10,
                    IsBefore = true
                }
            );
            var messageDto = _mapper.Map<MessageDto>(message);
            var reactions = message.MessageTrackings
                .Where(x => !x.IsDeleted)
                .Select(x => x.Reaction);
            messageDto.ReactionCount = CommonFunctions.GetReactionCount(reactions);

            messageDtos.Add(messageDto);
            messageDtos.AddRange(
                await GetMessagesAsync(
                    new GetMessageDto
                    {
                        ToChannelId = message.ToChannelId,
                        ToUserId = message.ToUserId,
                        ParentId = message.ParentId,
                        TimeCursor = message.CreatedAt,
                        Count = 10,
                        IsBefore = false
                    }
                )
            ); 

            _logger.LogInformation("[{_className}][{method}] End", _className, method);

            return messageDtos;
        }

        public async Task<MessageDto> PinMessage(Guid messageId, bool isPin = true)
        {
            var method = GetActualAsyncMethodName();
            _logger.LogInformation("[{_className}][{method}] Start", _className, method);
            var message =
                await _unitOfWork.Messages.Get(messageId)
                ?? throw new NotFoundException<Message>(messageId.ToString());
            var currentUserId = Guid.Parse(_currentUser.UserId);
            message.IsPined = isPin;
            await _unitOfWork.Messages.UpdateAsync(message);
            await _unitOfWork.SaveChangeAsync();
            message = await _unitOfWork.Messages.Get(messageId);
            MessageDto messageDto = _mapper.Map<MessageDto>(message);
            var reactions = message.MessageTrackings
                .Where(x => !x.IsDeleted)
                .Select(x => x.Reaction);
            messageDto.ReactionCount = CommonFunctions.GetReactionCount(reactions);

            _logger.LogInformation("[{_className}][{method}] End", _className, method);

            return messageDto;
        }

        public async Task<MessageDto> ReactMessageAsync(ReactMessageDto input)
        {
            var method = GetActualAsyncMethodName();
            _logger.LogInformation("[{_className}][{method}] Start", _className, method);
            var currentUserId = Guid.Parse(
                _currentUser.UserId ?? throw new UnauthorizedException("User is not authorized")
            );
            var message =
                await _unitOfWork.Messages.Get(input.MessageId)
                ?? throw new NotFoundException<Message>(input.MessageId.ToString());
            var isInConversation = await _unitOfWork.Messages.CheckUserInConversation(
                currentUserId,
                message.Id
            );

            if (!isInConversation)
            {
                throw new NotFoundException<Message>(input.MessageId.ToString());
            }

            var messageTracking = message.MessageTrackings.FirstOrDefault(
                x => x.UserId == currentUserId
            );

            if (messageTracking is null)
            {
                message.MessageTrackings.Add(
                    new MessageTracking
                    {
                        UserId = currentUserId,
                        IsDeleted = false,
                        IsRead = true,
                        Reaction = input.Emoji
                    }
                );
            }
            else
            {
                messageTracking.IsRead = true;
                messageTracking.Reaction ??= "";
                if (messageTracking.Reaction.Contains(input.Emoji))
                {
                    messageTracking.Reaction = messageTracking.Reaction.Replace(
                        $" {input.Emoji}",
                        ""
                    );
                }
                else
                {
                    messageTracking.Reaction += $" {input.Emoji}";
                }
            }

            await _unitOfWork.Messages.UpdateAsync(message);
            await _unitOfWork.SaveChangeAsync();
            MessageDto messageDto = _mapper.Map<MessageDto>(message);
            var reactions = message.MessageTrackings
                .Where(x => !x.IsDeleted)
                .Select(x => x.Reaction);
            messageDto.ReactionCount = CommonFunctions.GetReactionCount(reactions);

            _logger.LogInformation("[{_className}][{method}] End", _className, method);

            return messageDto;
        }

        public async Task<MessageDto> ReadMessageAsync(Guid messageId)
        {
            var method = GetActualAsyncMethodName();
            _logger.LogInformation("[{_className}][{method}] Start", _className, method);
            var message =
                await _unitOfWork.Messages.Get(messageId)
                ?? throw new NotFoundException<Message>(messageId.ToString());
            var currentUserId = Guid.Parse(_currentUser.UserId);
            var messageTracking = message.MessageTrackings.FirstOrDefault(
                x => x.UserId == currentUserId
            );
            if (messageTracking is null)
            {
                await _unitOfWork.MessageTrackings.AddAsync(
                    new MessageTracking
                    {
                        UserId = currentUserId,
                        IsDeleted = false,
                        IsRead = true,
                        Reaction = ""
                    }
                );
            }
            else
            {
                messageTracking.IsRead = true;
                await _unitOfWork.MessageTrackings.UpdateAsync(messageTracking);
            }

            await _unitOfWork.SaveChangeAsync();
            message = await _unitOfWork.Messages.Get(messageId);
            MessageDto messageDto = _mapper.Map<MessageDto>(message);
            var reactions = message.MessageTrackings
                .Where(x => !x.IsDeleted)
                .Select(x => x.Reaction);
            messageDto.ReactionCount = CommonFunctions.GetReactionCount(reactions);

            _logger.LogInformation("[{_className}][{method}] End", _className, method);

            return messageDto;
        }

        public async Task<MessageDto> SendMessageAsync(SendMessageDto input)
        {
            var method = GetActualAsyncMethodName();
            _logger.LogInformation("[{_className}][{method}] Start", _className, method);
            var currentUserId = Guid.Parse(
                _currentUser.UserId ?? throw new UnauthorizedException("User is not authorized")
            );
            var currentUser = await _unitOfWork.Users.GetUserByIdAsync(currentUserId);

            List<FileDomain> fileInfos = new();
            if (input.Files is not null)
            {
                foreach (var file in input.Files)
                {
                    var fileInfo = new FileDomain
                    {
                        Name = file.Name,
                        Type = file.Type,
                        Url = file.Url,
                    };
                    fileInfos.Add(fileInfo);
                }
            }

            var notification = new Notification
            {
                Title = "New message",
                Type = (short)NOTIFICATION_TYPE.NEW_MESSAGE,
                Status = (short)NOTIFICATION_STATUS.PENDING,
                TimeToSend = DateTime.UtcNow,
                UserNotifications = new List<UserNotification>()
            };
            if (input.IsChannel)
            {
                var isMember = await _unitOfWork.Channels.CheckIsMemberAsync(
                    input.ReceiverId,
                    currentUserId
                );
                if (!isMember)
                {
                    throw new NotFoundException<Channel>(input.ReceiverId.ToString());
                }

                Message message =
                    new()
                    {
                        Content = input.Content,
                        ToChannelId = input.ReceiverId,
                        ParentId = input.ReplyTo,
                        ToUserId = null,
                        MessageTrackings = new List<MessageTracking>(),
                        Files = fileInfos
                    };
                await _unitOfWork.Messages.AddAsync(message);
                await _unitOfWork.SaveChangeAsync();
                message = await _unitOfWork.Messages.Get(message.Id);
                MessageDto messageDto = _mapper.Map<MessageDto>(message);

                try
                {
                    var channel = await _unitOfWork.Channels.FindAsync(input.ReceiverId);
                    notification.Content =
                        $"{currentUser.Information.FirstName} {currentUser.Information.LastName} sent a message to {channel.Name}";
                    notification.Data = JsonConvert.SerializeObject(
                        new Dictionary<string, string>
                        {
                            { "Type", ((short)NOTIFICATION_TYPE.NEW_MESSAGE).ToString() },
                            {
                                "Detail",
                                JsonConvert.SerializeObject(
                                    new
                                    {
                                        ChannelId = input.ReceiverId,
                                        MessageId = message.Id,
                                        IsChannel = true
                                    }
                                )
                            },
                            {
                                "Url",
                                $"{_config["BaseUrl"]}/Workspace/{channel.WorkspaceId}/{input.ReceiverId}"
                            },
                            { "Avatar", $"{currentUser.Information.Picture}" }
                        }
                    );
                    var userIds = (await _unitOfWork.Channels.GetUserIds(input.ReceiverId))
                        .Where(x => x != currentUserId)
                        .ToList();
                    foreach (var userId in userIds)
                    {
                        if (await ChatHub.IsUserOnline(userId))
                            continue;

                        notification.UserNotifications.Add(
                            new UserNotification
                            {
                                UserId = userId,
                                Status = (short)NOTIFICATION_STATUS.PENDING,
                                SendAt = DateTimeOffset.UtcNow
                            }
                        );
                    }
                    if (notification.UserNotifications.Any())
                    {
                        notification = await _unitOfWork.Notifications.AddAsync(notification);
                        await _unitOfWork.SaveChangeAsync();
                        _backgroundJobClient.Enqueue(
                            () => _notificationService.SendNotificationAsync(notification.Id)
                        );
                    }
                }
                catch (Exception e)
                {
                    _logger.LogInformation(
                        "[{_className}][{method}] Error: {message}",
                        _className,
                        method,
                        e.Message
                    );
                }
                _logger.LogInformation("[{_className}][{method}] End", _className, method);

                return messageDto;
            }
            else
            {
                var isUser = await _unitOfWork.Users.CheckIsUserAsync(input.ReceiverId);
                if (!isUser)
                {
                    throw new NotFoundException<User>(input.ReceiverId.ToString());
                }

                Message message =
                    new()
                    {
                        Content = input.Content,
                        ToChannelId = null,
                        ParentId = input.ReplyTo,
                        ToUserId = input.ReceiverId,
                        MessageTrackings = new List<MessageTracking>(),
                        Files = fileInfos
                    };
                message = await _unitOfWork.Messages.AddAsync(message);
                await _unitOfWork.SaveChangeAsync();
                message = await _unitOfWork.Messages.Get(message.Id);
                MessageDto messageDto = _mapper.Map<MessageDto>(message);

                try
                {
                    if (!await ChatHub.IsUserOnline(input.ReceiverId))
                    {
                        notification.Content =
                            $"{currentUser.Information.FirstName} {currentUser.Information.LastName} sent a message to you";
                        notification.Data = JsonConvert.SerializeObject(
                            new Dictionary<string, string>
                            {
                                { "Type", ((short)NOTIFICATION_TYPE.NEW_MESSAGE).ToString() },
                                {
                                    "Detail",
                                    JsonConvert.SerializeObject(
                                        new
                                        {
                                            UserId = input.ReceiverId,
                                            MessageId = message.Id,
                                            IsChannel = false
                                        }
                                    )
                                },
                                { "Url", $"{_config["BaseUrl"]}/colleague-chat/{currentUserId}" },
                                { "Avatar", $"{currentUser.Information.Picture}" }
                            }
                        );
                        notification.UserNotifications.Add(
                            new UserNotification
                            {
                                UserId = input.ReceiverId,
                                Status = (short)NOTIFICATION_STATUS.PENDING,
                                SendAt = DateTimeOffset.UtcNow
                            }
                        );
                        notification = await _unitOfWork.Notifications.AddAsync(notification);
                        await _unitOfWork.SaveChangeAsync();
                        _backgroundJobClient.Enqueue(
                            () => _notificationService.SendNotificationAsync(notification.Id)
                        );
                    }
                }
                catch (Exception e)
                {
                    _logger.LogInformation(
                        "[{_className}][{method}] Error: {message}",
                        _className,
                        method,
                        e.Message
                    );
                }

                _logger.LogInformation("[{_className}][{method}] End", _className, method);

                return messageDto;
            }
        }

        public async Task<MessageDto> UpdateMessageAsync(UpdateMessageDto input)
        {
            var method = GetActualAsyncMethodName();
            _logger.LogInformation("[{_className}][{method}] Start", _className, method);
            var currentUserId = Guid.Parse(
                _currentUser.UserId ?? throw new UnauthorizedException("User is not authorized")
            );

            var message =
                await _unitOfWork.Messages.Get(input.Id)
                ?? throw new NotFoundException<Message>(input.Id.ToString());
            if (message.CreatedBy != currentUserId)
            {
                throw new ForbidException();
            }

            message.Content = input.Content;
            message.IsEdited = true;
            await _unitOfWork.Messages.UpdateAsync(message);
            await _unitOfWork.SaveChangeAsync();
            MessageDto messageDto = _mapper.Map<MessageDto>(message);
            var reactions = message.MessageTrackings
                .Where(x => !x.IsDeleted)
                .Select(x => x.Reaction);
            messageDto.ReactionCount = CommonFunctions.GetReactionCount(reactions);

            _logger.LogInformation("[{_className}][{method}] End", _className, method);

            return messageDto;
        }
    }
}
