using System.Runtime.CompilerServices;
using System.Text.Json;
using Hangfire;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PBL6.Application.Contract.Chats.Dtos;
using PBL6.Application.Contract.ExternalServices.Meetings.Dtos;
using PBL6.Application.Contract.Meetings.Dtos;
using PBL6.Application.ExternalServices;
using PBL6.Common.Enum;
using PBL6.Common.Exceptions;
using PBL6.Common.Functions;
using PBL6.Domain.Models.Users;

namespace PBL6.Application.Services
{
    public class MeetingService : BaseService, IMeetingService
    {
        private readonly string _className;
        private readonly IMeetingServiceEx _meetingServiceEx;

        public MeetingService(IServiceProvider serviceProvider, IMeetingServiceEx meetingServiceEx)
            : base(serviceProvider)
        {
            _className = GetActualAsyncMethodName();
            _meetingServiceEx = meetingServiceEx;
        }

        static string GetActualAsyncMethodName([CallerMemberName] string name = null) => name;

        public async Task<MeetingDto> CreateMeetingAsync(CreateMeetingDto input)
        {
            var method = GetActualAsyncMethodName();
            _logger.LogInformation("[{_className}][{method}] Start", _className, method);
            if (input.TimeStart > input.TimeEnd)
            {
                throw new BadRequestException("TimeStart must be less than TimeEnd");
            }

            var isExistSession =
                await _unitOfWork
                    .Repository<Meeting>()
                    .Queryable()
                    .AnyAsync(x => x.SessionId == input.SessionId)
                || _unitOfWork
                    .Repository<Call>()
                    .Queryable()
                    .Any(x => x.SessionId == input.SessionId);
            if (string.IsNullOrEmpty(input.SessionId))
            {
                input.SessionId = CommonFunctions.GenerateRandomCode(10);
            }
            if (string.IsNullOrEmpty(input.Password))
            {
                input.Password = CommonFunctions.GenerateRandomPassword(10);
            }

            if (isExistSession)
            {
                throw new BadRequestException("SessionId is already exist");
            }

            if (input.ChannelId is not null)
            {
                return await CreateChannelMeeting(input);
            }

            return await CreateMeetingForUsersAsync(input);
        }

        private async Task<MeetingDto> CreateChannelMeeting(CreateMeetingDto input)
        {
            var method = GetActualAsyncMethodName();
            var transaction = await _unitOfWork.BeginTransactionAsync();
            var currentUserId = Guid.Parse(
                _currentUser.UserId ?? throw new UnauthorizedException("User is not logged in")
            );
            var currentUser = await _unitOfWork.Users
                .Queryable()
                .Include(x => x.Information)
                .FirstOrDefaultAsync(x => x.Id == currentUserId && x.IsActive);

            var isInChannel = await _unitOfWork.Channels.CheckIsMemberAsync(
                input.ChannelId.Value,
                currentUserId
            );
            if (!isInChannel)
            {
                throw new BadRequestException("User is not in channel");
            }

            var channel = _unitOfWork.Channels
                .Queryable()
                .Include(
                    x =>
                        x.ChannelMembers.Where(x => x.Status == (short)CHANNEL_MEMBER_STATUS.ACTIVE)
                )
                .FirstOrDefault(x => x.Id == input.ChannelId);

            var meeting = new Meeting
            {
                Name = input.Name,
                TimeStart = input.TimeStart,
                TimeEnd = input.TimeEnd,
                Description = input.Description,
                Password = input.Password,
                SessionId = input.SessionId,
                Type = (short)input.Type,
                IsNotify = input.IsNotify,
                ChannelId = input.ChannelId.Value,
                Status = (short)MEETING_STATUS.SCHEDULED,
            };

            meeting = await _unitOfWork.Repository<Meeting>().AddAsync(meeting);

            await _unitOfWork.SaveChangeAsync();
            try
            {
                var message = new Message
                {
                    Content =
                        $"{currentUser.Information.FirstName} {currentUser.Information.LastName} has created a new meeting",
                    Type = (short)MESSAGE_TYPE.MEETING,
                    ToChannelId = input.ChannelId.Value,
                    Data = JsonSerializer.Serialize(
                        new MeetingDto
                        {
                            Id = meeting.Id,
                            Name = meeting.Name,
                            TimeStart = meeting.TimeStart,
                            TimeEnd = meeting.TimeEnd,
                            Description = meeting.Description,
                            SessionId = meeting.SessionId,
                            Password = meeting.Password,
                            ChannelId = meeting.ChannelId,
                        }
                    ),
                };
                message = await _unitOfWork.Messages.AddAsync(message);
                await _unitOfWork.SaveChangeAsync();
                message = await _unitOfWork.Messages.Get(message.Id);
                var messageDto = _mapper.Map<MessageDto>(message);
                _backgroundJobClient.Enqueue(() => _hubService.SendMessage(messageDto));
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
            if (input.IsNotify)
            {
                var notification = new Notification
                {
                    Title = "New meeting",
                    Content =
                        $"{currentUser.Information.FirstName + " " + currentUser.Information.LastName} has created a new meeting",
                    TimeToSend = DateTime.UtcNow,
                    Status = (short)NOTIFICATION_STATUS.PENDING,
                    Type = (short)NOTIFICATION_TYPE.MEETING,
                    Data = JsonSerializer.Serialize(
                        new Dictionary<string, string>
                        {
                            { "Type", ((short)NOTIFICATION_TYPE.MEETING).ToString() },
                            {
                                "Detail",
                                JsonSerializer.Serialize(
                                    new MeetingDto
                                    {
                                        Id = meeting.Id,
                                        Name = meeting.Name,
                                        TimeStart = meeting.TimeStart,
                                        TimeEnd = meeting.TimeEnd,
                                        Description = meeting.Description,
                                        SessionId = meeting.SessionId,
                                        Password = meeting.Password,
                                        ChannelId = meeting.ChannelId
                                    }
                                )
                            },
                            {
                                "Url",
                                $"{_config["BaseUrl"]}/Workspace/{channel.WorkspaceId}/{meeting.ChannelId}"
                            }
                        }
                    ),
                    UserNotifications = channel.ChannelMembers
                        .Where(x => x.UserId != currentUserId)
                        .Select(
                            x =>
                                new UserNotification
                                {
                                    UserId = x.UserId,
                                    Status = (short)NOTIFICATION_STATUS.PENDING,
                                }
                        )
                        .ToList(),
                };
                await _unitOfWork.SaveChangeAsync();

                try
                {
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
            }
            await _unitOfWork.CommitAsync(transaction);

            return new MeetingDto
            {
                Id = meeting.Id,
                Name = meeting.Name,
                TimeStart = meeting.TimeStart,
                TimeEnd = meeting.TimeEnd,
                Description = meeting.Description,
                SessionId = meeting.SessionId,
                Password = meeting.Password,
                ChannelId = meeting.ChannelId,
                Members = meeting.Channel.ChannelMembers
                    .Select(
                        x =>
                            new MemberOfMeetingDto
                            {
                                UserId = x.UserId,
                                FullName =
                                    x.User.Information.FirstName
                                    + " "
                                    + x.User.Information.LastName,
                                Email = x.User.Email,
                                IsHost = x.UserId == meeting.CreatedBy,
                            }
                    )
                    .ToList()
            };
        }

        private async Task<MeetingDto> CreateMeetingForUsersAsync(CreateMeetingDto input)
        {
            var method = GetActualAsyncMethodName();
            var transaction = await _unitOfWork.BeginTransactionAsync();
            var currentUserId = Guid.Parse(
                _currentUser.UserId ?? throw new UnauthorizedException("User is not logged in")
            );
            var currentUser = await _unitOfWork.Users
                .Queryable()
                .Include(x => x.Information)
                .FirstOrDefaultAsync(x => x.Id == currentUserId && x.IsActive);

            var workspace =
                await _unitOfWork.Workspaces.GetAsync(input.WorkspaceId)
                ?? throw new BadRequestException("Workspace not found");
            var isInWorkspace = await _unitOfWork.Workspaces.CheckIsMemberAsync(
                input.WorkspaceId,
                currentUserId
            );
            if (!isInWorkspace)
            {
                throw new BadRequestException("User is not in workspace");
            }

            if (string.IsNullOrEmpty(input.SessionId))
            {
                input.SessionId = CommonFunctions.GenerateRandomCode(10);
            }

            input.MemberIds ??= new List<Guid>();
            input.MemberIds.Add(currentUserId);

            var countMember = await _unitOfWork.WorkspaceMembers
                .Queryable()
                .Where(
                    x =>
                        x.WorkspaceId == input.WorkspaceId
                        && input.MemberIds.Contains(x.UserId)
                        && x.Status == (short)WORKSPACE_MEMBER_STATUS.ACTIVE
                )
                .CountAsync();
            if (countMember != input.MemberIds.Count)
            {
                throw new BadRequestException("Some members is not in workspace");
            }

            Channel channel =
                new()
                {
                    Name = input.Name,
                    Description = input.Description,
                    Category = (short)CHANNEL_CATEGORY.MEETING,
                    WorkspaceId = input.WorkspaceId,
                    Meetings = new List<Meeting>
                    {
                        new()
                        {
                            Name = input.Name,
                            TimeStart = input.TimeStart,
                            TimeEnd = input.TimeEnd,
                            Description = input.Description,
                            SessionId = input.SessionId,
                            Password = input.Password,
                            Type = (short)input.Type,
                            IsNotify = input.IsNotify,
                            Status = (short)MEETING_STATUS.SCHEDULED,
                        }
                    },
                    ChannelMembers = input.MemberIds
                        .Select(
                            x =>
                                new ChannelMember
                                {
                                    UserId = x,
                                    Status = (short)CHANNEL_MEMBER_STATUS.ACTIVE,
                                }
                        )
                        .ToList(),
                    OwnerId = currentUserId
                };

            channel = await _unitOfWork.Channels.AddAsync(channel);
            await _unitOfWork.SaveChangeAsync();
            var message = new Message
            {
                Content =
                    $"{currentUser.Information.FirstName} {currentUser.Information.LastName} has created a new meeting",
                Type = (short)MESSAGE_TYPE.MEETING,
                ToChannelId = channel.Id,
                Data = JsonSerializer.Serialize(
                    new MeetingDto
                    {
                        Id = channel.Meetings.FirstOrDefault().Id,
                        Name = channel.Meetings.FirstOrDefault().Name,
                        TimeStart = channel.Meetings.FirstOrDefault().TimeStart,
                        TimeEnd = channel.Meetings.FirstOrDefault().TimeEnd,
                        Description = channel.Meetings.FirstOrDefault().Description,
                        SessionId = channel.Meetings.FirstOrDefault().SessionId,
                        Password = channel.Meetings.FirstOrDefault().Password,
                        ChannelId = channel.Id
                    }
                ),
            };
            message = await _unitOfWork.Messages.AddAsync(message);
            await _unitOfWork.SaveChangeAsync();
            message = await _unitOfWork.Messages.Get(message.Id);
            var messageDto = _mapper.Map<MessageDto>(message);
            _backgroundJobClient.Enqueue(() => _hubService.SendMessage(messageDto));

            if (input.IsNotify)
            {
                var notification = new Notification
                {
                    Title = "New meeting",
                    Content =
                        $"{currentUser.Information.FirstName + " " + currentUser.Information.LastName} has created a new meeting",
                    TimeToSend = DateTime.UtcNow,
                    Status = (short)NOTIFICATION_STATUS.PENDING,
                    Type = (short)NOTIFICATION_TYPE.MEETING,
                    Data = JsonSerializer.Serialize(
                        new Dictionary<string, string>
                        {
                            { "Type", ((short)NOTIFICATION_TYPE.MEETING).ToString() },
                            {
                                "Detail",
                                JsonSerializer.Serialize(
                                    new MeetingDto
                                    {
                                        Id = channel.Meetings.FirstOrDefault().Id,
                                        Name = channel.Meetings.FirstOrDefault().Name,
                                        TimeStart = channel.Meetings.FirstOrDefault().TimeStart,
                                        TimeEnd = channel.Meetings.FirstOrDefault().TimeEnd,
                                        Description = channel.Meetings.FirstOrDefault().Description,
                                        SessionId = channel.Meetings.FirstOrDefault().SessionId,
                                        Password = channel.Meetings.FirstOrDefault().Password,
                                        ChannelId = channel.Id
                                    }
                                )
                            },
                            {
                                "Url",
                                $"{_config["BaseUrl"]}/Workspace/{channel.WorkspaceId}/{channel.Id}"
                            }
                        }
                    ),
                    UserNotifications = channel.ChannelMembers
                        .Where(x => x.UserId != currentUserId)
                        .Select(
                            x =>
                                new UserNotification
                                {
                                    UserId = x.UserId,
                                    Status = (short)NOTIFICATION_STATUS.PENDING,
                                }
                        )
                        .ToList(),
                };
                await _unitOfWork.SaveChangeAsync();

                try
                {
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
            }
            await _unitOfWork.CommitAsync(transaction);

            return new MeetingDto
            {
                Id = channel.Meetings.FirstOrDefault().Id,
                Name = channel.Meetings.FirstOrDefault().Name,
                TimeStart = channel.Meetings.FirstOrDefault().TimeStart,
                TimeEnd = channel.Meetings.FirstOrDefault().TimeEnd,
                Description = channel.Meetings.FirstOrDefault().Description,
                SessionId = channel.Meetings.FirstOrDefault().SessionId,
                Password = channel.Meetings.FirstOrDefault().Password,
                ChannelId = channel.Id,
                Members = channel.ChannelMembers
                    .Select(
                        x =>
                            new MemberOfMeetingDto
                            {
                                UserId = x.UserId,
                                FullName =
                                    x.User.Information.FirstName
                                    + " "
                                    + x.User.Information.LastName,
                                Email = x.User.Email,
                                IsHost = x.UserId == channel.CreatedBy
                            }
                    )
                    .ToList()
            };
        }

        public async Task<MeetingDto> UpdateMeetingAsync(Guid id, UpdateMeetingDto input)
        {
            var method = GetActualAsyncMethodName();
            _logger.LogInformation("[{_className}][{method}] Start", _className, method);
            var _currentUserId = Guid.Parse(
                _currentUser.UserId ?? throw new UnauthorizedException("User is not logged in")
            );
            var transaction = await _unitOfWork.BeginTransactionAsync();
            var current = await _unitOfWork.Users
                .Queryable()
                .Include(x => x.Information)
                .FirstOrDefaultAsync(x => x.Id == _currentUserId && x.IsActive);
            var meeting =
                await _unitOfWork
                    .Repository<Meeting>()
                    .Queryable()
                    .Include(x => x.Channel)
                    .ThenInclude(x => x.ChannelMembers)
                    .ThenInclude(x => x.User)
                    .ThenInclude(x => x.Information)
                    .FirstOrDefaultAsync(x => x.Id == id)
                ?? throw new BadRequestException("Meeting not found");
            var message = new Message
            {
                Content =
                    $"{current.Information.FirstName} {current.Information.LastName} has updated meeting {meeting.Name}",
                Type = (short)MESSAGE_TYPE.MEETING,
                ToChannelId = meeting.ChannelId,
            };

            meeting.Name = input.Name;
            meeting.TimeStart = input.TimeStart;
            meeting.TimeEnd = input.TimeEnd;
            meeting.Description = input.Description;
            meeting.Type = (short)input.Type;
            meeting.Password = input.Password;

            var countMember = await _unitOfWork.WorkspaceMembers
                .Queryable()
                .Where(
                    x =>
                        x.WorkspaceId == meeting.Channel.WorkspaceId
                        && input.MemberIds.Contains(x.UserId)
                        && x.Status == (short)WORKSPACE_MEMBER_STATUS.ACTIVE
                )
                .CountAsync();
            if (countMember != input.MemberIds.Count)
            {
                throw new BadRequestException("Some members is not in workspace");
            }

            foreach (var member in meeting.Channel.ChannelMembers)
            {
                if (input.MemberIds.Contains(member.UserId))
                {
                    member.Status = (short)CHANNEL_MEMBER_STATUS.ACTIVE;
                }
                else
                {
                    member.Status = (short)CHANNEL_MEMBER_STATUS.REMOVED;
                }
            }
            foreach (var member in input.MemberIds)
            {
                if (meeting.Channel.ChannelMembers.All(x => x.UserId != member))
                {
                    meeting.Channel.ChannelMembers.Add(
                        new ChannelMember
                        {
                            UserId = member,
                            Status = (short)CHANNEL_MEMBER_STATUS.ACTIVE,
                        }
                    );
                }
            }

            await _unitOfWork.Repository<Meeting>().UpdateAsync(meeting);
            await _unitOfWork.SaveChangeAsync();

            meeting = await _unitOfWork
                .Repository<Meeting>()
                .Queryable()
                .Include(x => x.Channel)
                .ThenInclude(x => x.ChannelMembers)
                .ThenInclude(x => x.User)
                .ThenInclude(x => x.Information)
                .FirstOrDefaultAsync(x => x.Id == id);

            message.Data = JsonSerializer.Serialize(
                JsonSerializer.Serialize(
                    new MeetingDto
                    {
                        Id = meeting.Id,
                        Name = meeting.Name,
                        TimeStart = meeting.TimeStart,
                        TimeEnd = meeting.TimeEnd,
                        Description = meeting.Description,
                        SessionId = meeting.SessionId,
                        Password = meeting.Password,
                        ChannelId = meeting.ChannelId,
                        Members = meeting.Channel.ChannelMembers
                            .Select(
                                x =>
                                    new MemberOfMeetingDto
                                    {
                                        UserId = x.UserId,
                                        FullName =
                                            x.User.Information.FirstName
                                            + " "
                                            + x.User.Information.LastName,
                                        Email = x.User.Email,
                                        IsHost = x.UserId == meeting.CreatedBy,
                                    }
                            )
                            .ToList()
                    }
                )
            );

            message = await _unitOfWork.Messages.AddAsync(message);
            await _unitOfWork.SaveChangeAsync();
            message = await _unitOfWork.Messages.Get(message.Id);
            var messageDto = _mapper.Map<MessageDto>(message);
            _backgroundJobClient.Enqueue(() => _hubService.SendMessage(messageDto));
            await _unitOfWork.CommitAsync(transaction);

            return new MeetingDto
            {
                Id = meeting.Id,
                Name = meeting.Name,
                TimeStart = meeting.TimeStart,
                TimeEnd = meeting.TimeEnd,
                Description = meeting.Description,
                SessionId = meeting.SessionId,
                Password = meeting.Password,
                ChannelId = meeting.ChannelId,
                Members = meeting.Channel.ChannelMembers
                    .Select(
                        x =>
                            new MemberOfMeetingDto
                            {
                                UserId = x.UserId,
                                FullName =
                                    x.User.Information.FirstName
                                    + " "
                                    + x.User.Information.LastName,
                                Email = x.User.Email,
                                IsHost = x.UserId == meeting.CreatedBy,
                            }
                    )
                    .ToList()
            };
        }

        public async Task DeleteMeetingAsync(Guid id)
        {
            var method = GetActualAsyncMethodName();
            _logger.LogInformation("[{_className}][{method}] Start", _className, method);
            var meeting =
                await _unitOfWork.Repository<Meeting>().FindAsync(id)
                ?? throw new BadRequestException("Meeting not found");
            var currentUserId = Guid.Parse(
                _currentUser.UserId ?? throw new UnauthorizedException("User is not logged in")
            );
            if (meeting.CreatedBy != currentUserId)
            {
                throw new BadRequestException("You are not owner of this meeting");
            }

            var message = new Message
            {
                Content = $"{currentUserId} has deleted meeting {meeting.Name}",
                Type = (short)MESSAGE_TYPE.MEETING,
                ToChannelId = meeting.ChannelId,
            };
            message = await _unitOfWork.Messages.AddAsync(message);
            meeting.Status = (short)MEETING_STATUS.CANCELED;
            await _unitOfWork.SaveChangeAsync();
            message = await _unitOfWork.Messages.Get(message.Id);
            var messageDto = _mapper.Map<MessageDto>(message);
            _backgroundJobClient.Enqueue(() => _hubService.SendMessage(messageDto));

            await _meetingServiceEx.CloseSession(meeting.SessionId);
        }

        public async Task<string> JoinMeetingAsync(JoinMeetingDto input)
        {
            var method = GetActualAsyncMethodName();
            _logger.LogInformation("[{_className}][{method}] Start", _className, method);
            var currentUserId = Guid.Parse(
                _currentUser.UserId ?? throw new UnauthorizedException("User is not logged in")
            );
            var currentUser = await _unitOfWork.Users
                .Queryable()
                .Include(x => x.Information)
                .FirstOrDefaultAsync(x => x.Id == currentUserId && x.IsActive);
            var meeting =
                await _unitOfWork
                    .Repository<Meeting>()
                    .Queryable()
                    .Include(x => x.Channel)
                    .ThenInclude(x => x.ChannelMembers)
                    .ThenInclude(x => x.User)
                    .FirstOrDefaultAsync(x => x.SessionId == input.SessionId)
                ?? throw new BadRequestException("Meeting not found");
            var isInChannel = await _unitOfWork.Channels.CheckIsMemberAsync(
                meeting.ChannelId,
                currentUserId
            );
            var isInWorkspace = await _unitOfWork.Workspaces.CheckIsMemberAsync(
                meeting.Channel.WorkspaceId,
                currentUserId
            );
            if (!isInWorkspace)
            {
                throw new BadRequestException("User is not in workspace");
            }

            if (!isInChannel)
            {
                if (meeting.Channel.Category != (short)CHANNEL_CATEGORY.MEETING)
                {
                    throw new BadRequestException("User is not in channel");
                }

                if (string.IsNullOrEmpty(input.Password))
                {
                    throw new BadRequestException("Password is required");
                }
                if (input.Password != meeting.Password)
                {
                    throw new BadRequestException("Password is incorrect");
                }

                meeting.Channel.ChannelMembers.Add(
                    new ChannelMember
                    {
                        UserId = currentUserId,
                        Status = (short)CHANNEL_MEMBER_STATUS.ACTIVE,
                    }
                );
            }
            await _meetingServiceEx.CreateSession(
                new Session { customSessionId = input.SessionId, }
            );
            var token = await _meetingServiceEx.CreateToken(input.SessionId);

            var message = new Message
            {
                Content =
                    $"{currentUser.Information.FirstName} {currentUser.Information.LastName} has joined meeting {meeting.Name}",
                Type = (short)MESSAGE_TYPE.SYSTEM,
                ToChannelId = meeting.ChannelId,
            };
            message = await _unitOfWork.Messages.AddAsync(message);
            await _unitOfWork.SaveChangeAsync();
            message = await _unitOfWork.Messages.Get(message.Id);
            var messageDto = _mapper.Map<MessageDto>(message);
            _backgroundJobClient.Enqueue(() => _hubService.SendMessage(messageDto));

            return token;
        }

        public async Task EndMeetingAsync(JoinMeetingDto input)
        {
            var method = GetActualAsyncMethodName();
            _logger.LogInformation("[{_className}][{method}] Start", _className, method);
            var currentUserId = Guid.Parse(
                _currentUser.UserId ?? throw new UnauthorizedException("User is not logged in")
            );
            var currentUser =
                await _unitOfWork.Users
                    .Queryable()
                    .Include(x => x.Information)
                    .FirstOrDefaultAsync(x => x.Id == currentUserId && x.IsActive)
                ?? throw new BadRequestException("User not found");

            var meeting =
                _unitOfWork
                    .Repository<Meeting>()
                    .Queryable()
                    .FirstOrDefault(
                        x => x.SessionId == input.SessionId && x.Password == input.Password
                    ) ?? throw new BadRequestException("Meeting not found");

            if (meeting.Status == (short)MEETING_STATUS.ENDED)
            {
                throw new BadRequestException("Meeting has ended");
            }

            if (meeting.CreatedBy != currentUserId)
            {
                throw new BadRequestException("You are not owner of this meeting");
            }

            meeting.Status = (short)MEETING_STATUS.ENDED;
            await _unitOfWork.Repository<Meeting>().UpdateAsync(meeting);
            await _unitOfWork.SaveChangeAsync();

            Message message =
                new()
                {
                    Content =
                        $"{currentUser.Information.FirstName} {currentUser.Information.LastName} has ended meeting {meeting.Name}",
                    Type = (short)MESSAGE_TYPE.SYSTEM,
                    ToChannelId = meeting.ChannelId,
                    Data = JsonSerializer.Serialize(
                        new MeetingDto
                        {
                            Id = meeting.Id,
                            Name = meeting.Name,
                            TimeStart = meeting.TimeStart,
                            TimeEnd = meeting.TimeEnd,
                            Description = meeting.Description,
                            SessionId = meeting.SessionId,
                            Password = meeting.Password,
                            ChannelId = meeting.ChannelId,
                        }
                    )
                };
            message = await _unitOfWork.Messages.AddAsync(message);
            await _unitOfWork.SaveChangeAsync();
            message = await _unitOfWork.Messages.Get(message.Id);
            var messageDto = _mapper.Map<MessageDto>(message);
            await _meetingServiceEx.CloseSession(input.SessionId);
            _backgroundJobClient.Enqueue(() => _hubService.SendMessage(messageDto));
        }

        public async Task EndCallAsync(JoinMeetingDto joinMeetingDto)
        {
            var method = GetActualAsyncMethodName();
            var currentUserId = Guid.Parse(
                _currentUser.UserId ?? throw new UnauthorizedException("User is not logged in")
            );
            var call =
                await _unitOfWork
                    .Repository<Call>()
                    .Queryable()
                    .FirstOrDefaultAsync(
                        x =>
                            x.SessionId == joinMeetingDto.SessionId
                            && x.Password == joinMeetingDto.Password
                            && x.Status == ((short)CALL_STATUS.ACTIVE)
                    ) ?? throw new BadRequestException("Call not found");
            call.Status = (short)CALL_STATUS.ENDED;
            await _unitOfWork.Repository<Call>().UpdateAsync(call);
            await _unitOfWork.SaveChangeAsync();
            await _meetingServiceEx.CloseSession(joinMeetingDto.SessionId);
        }

        public async Task<CallInfoDto> JoinCallAsync(JoinMeetingDto joinMeetingDto)
        {
            var method = GetActualAsyncMethodName();
            _logger.LogInformation("[{_className}][{method}] Start", _className, method);
            var currentUserId = Guid.Parse(
                _currentUser.UserId ?? throw new UnauthorizedException("User is not logged in")
            );

            var call =
                _unitOfWork
                    .Repository<Call>()
                    .Queryable()
                    .FirstOrDefault(
                        x =>
                            x.SessionId == joinMeetingDto.SessionId
                            && x.Password == joinMeetingDto.Password
                    ) ?? throw new BadRequestException("Call not found");
            if (call.Status == (short)CALL_STATUS.ENDED)
            {
                throw new BadRequestException("Call has ended");
            }
            return new CallInfoDto
            {
                SessionId = call.SessionId,
                Password = call.Password,
                Token = await _meetingServiceEx.CreateToken(call.SessionId),
            };
        }

        public async Task<CallInfoDto> MakeCallAsync(MakeCallDto input)
        {
            var method = GetActualAsyncMethodName();
            var transaction = await _unitOfWork.BeginTransactionAsync();
            _logger.LogInformation("[{_className}][{method}] Start", _className, method);
            var currentUserId = Guid.Parse(
                _currentUser.UserId ?? throw new UnauthorizedException("User is not logged in")
            );
            if (input.ChannelId is null && input.UserId is null)
            {
                throw new BadRequestException("ChannelId or UserId is required");
            }

            if (input.ChannelId is not null && input.UserId is not null)
            {
                throw new BadRequestException("ChannelId and UserId cannot be used together");
            }

            var isExistUser =
                input.UserId is null
                || await _unitOfWork.Users.CheckIsUserAsync(input.UserId.Value);
            var isExistChannel =
                input.ChannelId is null
                || await _unitOfWork.Channels.CheckIsMemberAsync(
                    input.ChannelId.Value,
                    currentUserId
                );

            if (!isExistUser || !isExistChannel)
            {
                throw new BadRequestException("User or Channel not found");
            }

            var sessionId = CommonFunctions.GenerateRandomCode(10);
            var password = CommonFunctions.GenerateRandomPassword(10);
            var call = new Call
            {
                SessionId = sessionId,
                Password = password,
                Status = (short)CALL_STATUS.ACTIVE,
            };
            await _meetingServiceEx.CreateSession(new Session { customSessionId = sessionId });
            call = await _unitOfWork.Repository<Call>().AddAsync(call);
            await _unitOfWork.SaveChangeAsync();

            var message = new Message
            {
                Content = $"{currentUserId} has created a new call",
                Type = (short)MESSAGE_TYPE.CALL,
                ToChannelId = input.ChannelId,
                ToUserId = input.UserId,
                Data = JsonSerializer.Serialize(
                    new CallDto
                    {
                        Id = call.Id,
                        SessionId = call.SessionId,
                        Password = call.Password,
                        Status = call.Status,
                    }
                ),
            };
            message = await _unitOfWork.Messages.AddAsync(message);
            await _unitOfWork.SaveChangeAsync();
            message = await _unitOfWork.Messages.Get(message.Id);
            var messageDto = _mapper.Map<MessageDto>(message);
            _backgroundJobClient.Enqueue(() => _hubService.SendMessage(messageDto));
            await _unitOfWork.CommitAsync(transaction);

            return new CallInfoDto
            {
                SessionId = call.SessionId,
                Password = call.Password,
                Token = await _meetingServiceEx.CreateToken(call.SessionId),
            };
        }

        public async Task<List<MeetingInfo>> GetMeetingsAsync(Guid? workspaceId)
        {
            var method = GetActualAsyncMethodName();
            _logger.LogInformation("[{_className}][{method}] Start", _className, method);
            var currentUserId = Guid.Parse(
                _currentUser.UserId ?? throw new UnauthorizedException("User is not logged in")
            );
            var meetings = await _unitOfWork
                .Repository<Meeting>()
                .Queryable()
                .Include(x => x.Channel)
                .ThenInclude(x => x.ChannelMembers)
                .ThenInclude(x => x.User)
                .ThenInclude(x => x.Information)
                .Include(x => x.Channel)
                .ThenInclude(x => x.Workspace)
                .Where(
                    x =>
                        x.Channel.ChannelMembers.Any(x => x.UserId == currentUserId && !x.IsDeleted)
                        && (workspaceId == null || x.Channel.WorkspaceId == workspaceId)
                        && !x.Channel.IsDeleted
                        && !x.IsDeleted
                        && !x.Channel.Workspace.IsDeleted
                )
                .ToListAsync();

            List<MeetingInfo> meetingDtos = _mapper.Map<List<MeetingInfo>>(meetings);

            return meetingDtos;
        }

        public async Task<MeetingInfo> GetMeetingAsync(Guid id)
        {
            var method = GetActualAsyncMethodName();
            _logger.LogInformation("[{_className}][{method}] Start", _className, method);
            var currentUserId = Guid.Parse(
                _currentUser.UserId ?? throw new UnauthorizedException("User is not logged in")
            );
            var meeting =
                await _unitOfWork
                    .Repository<Meeting>()
                    .Queryable()
                    .Include(x => x.Channel)
                    .ThenInclude(x => x.ChannelMembers)
                    .ThenInclude(x => x.User)
                    .ThenInclude(x => x.Information)
                    .Include(x => x.Channel)
                    .ThenInclude(x => x.Workspace)
                    .FirstOrDefaultAsync(
                        x =>
                            x.Id == id
                            && x.Channel.ChannelMembers.Any(x => x.UserId == currentUserId && !x.IsDeleted)
                            && !x.IsDeleted
                            && !x.Channel.IsDeleted
                            && !x.Channel.Workspace.IsDeleted
                    ) ?? throw new BadRequestException("Meeting not found");

            var meetingDto = _mapper.Map<MeetingInfo>(meeting);
            _logger.LogInformation("[{_className}][{method}] End", _className, method);

            return meetingDto;
        }

        public async Task<List<MeetingInfo>> GetMeetingsByChannelIdAsync(Guid channelId)
        {
            var method = GetActualAsyncMethodName();
            _logger.LogInformation("[{_className}][{method}] Start", _className, method);
            var currentUserId = Guid.Parse(
                _currentUser.UserId ?? throw new UnauthorizedException("User is not logged in")
            );
            var meetings = await _unitOfWork
                .Repository<Meeting>()
                .Queryable()
                .Include(x => x.Channel)
                .ThenInclude(
                    x =>
                        x.ChannelMembers.Where(x => x.Status == (short)CHANNEL_MEMBER_STATUS.ACTIVE)
                )
                .ThenInclude(x => x.User)
                .ThenInclude(x => x.Information)
                .Include(x => x.Channel)
                .ThenInclude(x => x.Workspace)
                .Where(
                    x =>
                        x.ChannelId == channelId
                        && x.Channel.ChannelMembers.Any(
                            x =>
                                x.UserId == currentUserId
                                && x.Status == (short)CHANNEL_MEMBER_STATUS.ACTIVE
                        )
                )
                .ToListAsync();

            var meetingDtos = _mapper.Map<List<MeetingInfo>>(meetings);

            return meetingDtos;
        }
    }
}
