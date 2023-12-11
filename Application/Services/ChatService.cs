using System.Runtime.CompilerServices;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PBL6.Application.Contract.Chats;
using PBL6.Application.Contract.Chats.Dtos;
using PBL6.Application.Services;
using PBL6.Common.Consts;
using PBL6.Common.Exceptions;
using PBL6.Common.Functions;
using PBL6.Domain.Models.Users;

namespace workspace.PBL6.Application.Services
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
            var conversationDtos = await _unitOfWork.Messages.GetConversations(
                currentUserId,
                input.Search
            )
            .GroupBy(x => x.ToUserId == currentUserId ? x.CreatedBy : x.ToUserId)
            .OrderByDescending(x => x.Max(x => x.CreatedAt))
            .Select(x => new ConversationDto
            {
            Id = (Guid)x.Key,
            Name = currentUserId == x.First().CreatedBy
                ? x.First().Receiver.Information.FirstName + " " + x.First().Receiver.Information.LastName
                : x.First().Sender.Information.FirstName + " " + x.First().Sender.Information.LastName,
            LastMessage = x.First().Content,
            LastMessageTime = x.First().CreatedAt,
            LastMessageSender = currentUserId == x.First().CreatedBy
                ? "You"
                : x.First().Receiver.Information.FirstName + " " + x.First().Receiver.Information.LastName,
            LastMessageSenderAvatar = x.First().Sender.Information.Picture,
            IsRead = x.First().MessageTrackings.Any(x => x.UserId == currentUserId && !x.IsDeleted && x.IsRead),
            IsChannel = x.First().ToChannelId != null,
            Avatar = currentUserId == x.First().CreatedBy
                ? x.First().Receiver.Information.Picture
                : x.First().Sender.Information.Picture
            })
            .Skip(input.Offset)
            .Take(input.Limit)
            .ToListAsync();

            _logger.LogInformation("[{_className}][{method}] End", _className, method);

            return conversationDtos;
        }

        public async Task<List<MessageDto>> GetMessagesAsync(GetMessageDto input)
        {
            var method = GetActualAsyncMethodName();
            _logger.LogInformation("[{_className}][{method}] Start", _className, method);
            var currentUserId = Guid.Parse(
                _currentUser.UserId ?? throw new UnauthorizedException("User is not authorized")
            );
            List<MessageDto> messageDtos;
            IEnumerable<Message> messages;
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

                messages =
                    await _unitOfWork.Messages.GetMessagesOfChannelAsync(
                        input.ToChannelId.Value,
                        input.TimeCursor,
                        input.Count,
                        currentUserId
                    );
            }
            else
            {
                messages = await _unitOfWork.Messages.GetMessagesOfUserAsync(
                    currentUserId,
                    input.ToUserId.Value,
                    input.TimeCursor,
                    input.Count
                );
            }
            messageDtos = _mapper.Map<List<MessageDto>>(messages);
            foreach (var (message, messageDto) in messages.Zip(messageDtos))
            {
                var reactions = message.MessageTrackings.Where(x => !x.IsDeleted).Select(x => x.Reaction);
                messageDto.ReactionCount = CommonFunctions.GetReactionCount(reactions);
            } 

            _logger.LogInformation("[{_className}][{method}] End", _className, method);

            return messageDtos;
        }

        public async Task<MessageDto> ReactMessageAsync(ReactMessageDto input)
        {
            var method = GetActualAsyncMethodName();
            _logger.LogInformation("[{_className}][{method}] Start", _className, method);
            var currentUserId = Guid.Parse(
                _currentUser.UserId ?? throw new UnauthorizedException("User is not authorized")
            );
            var message = await
                _unitOfWork.Messages.Get(input.MessageId)
                ?? throw new NotFoundException<Message>(input.MessageId.ToString());
            var isInConversation = await  _unitOfWork.Messages.CheckUserInConversation(
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
                    new MessageTracking { UserId = currentUserId, IsDeleted = false, IsRead = true, Reaction = input.Emoji }
                );
            }
            else
            {
                messageTracking.IsRead = true;
                if (messageTracking.Reaction.Contains(input.Emoji))
                {
                    messageTracking.Reaction = messageTracking.Reaction.Replace(input.Emoji, "");
                }
                else
                {
                    messageTracking.Reaction += input.Emoji + " ";
                }
            }

            await _unitOfWork.Messages.UpdateAsync(message);
            await _unitOfWork.SaveChangeAsync();
            MessageDto messageDto = _mapper.Map<MessageDto>(message);
            var reactions = message.MessageTrackings.Where(x => !x.IsDeleted).Select(x => x.Reaction);
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
                        MessageTrackings = new List<MessageTracking>()
                    };
                await _unitOfWork.Messages.AddAsync(message);
                await _unitOfWork.SaveChangeAsync();
                message = await _unitOfWork.Messages.Get(message.Id);
                MessageDto messageDto = _mapper.Map<MessageDto>(message);
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
                        MessageTrackings = new List<MessageTracking>()
                    };
                message = await _unitOfWork.Messages.AddAsync(message);
                await _unitOfWork.SaveChangeAsync();
                message = await _unitOfWork.Messages.Get(message.Id);
                MessageDto messageDto = _mapper.Map<MessageDto>(message);
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
            await _unitOfWork.Messages.UpdateAsync(message);
            await _unitOfWork.SaveChangeAsync();
            MessageDto messageDto = _mapper.Map<MessageDto>(message);
            var reactions = message.MessageTrackings.Where(x => !x.IsDeleted).Select(x => x.Reaction);
            messageDto.ReactionCount = CommonFunctions.GetReactionCount(reactions);

            _logger.LogInformation("[{_className}][{method}] End", _className, method);

            return messageDto;
        }
    }
}
