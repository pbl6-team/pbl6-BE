using System.Runtime.CompilerServices;
using System.Text.Json;
using Hangfire;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PBL6.Application.Contract.Channels;
using PBL6.Application.Contract.Channels.Dtos;
using PBL6.Application.Contract.ExternalServices.Notifications.Dtos;
using PBL6.Application.Contract.Users.Dtos;
using PBL6.Application.Contract.Workspaces.Dtos;
using PBL6.Common.Consts;
using PBL6.Common.Enum;
using PBL6.Common.Exceptions;
using PBL6.Domain.Models.Users;

namespace PBL6.Application.Services;

public class ChannelService : BaseService, IChannelService
{
    private readonly string _className;

    public ChannelService(IServiceProvider serviceProvider)
        : base(serviceProvider)
    {
        _className = typeof(ChannelService).Name;
    }

    static string GetActualAsyncMethodName([CallerMemberName] string name = null) => name;

    public async Task<Guid> AddAsync(CreateChannelDto createUpdateChannelDto)
    {
        var method = GetActualAsyncMethodName();
        try
        {
            _logger.LogInformation("[{_className}][{method}] Start", _className, method);
            var channel = _mapper.Map<Channel>(createUpdateChannelDto);
            var currentUserId = Guid.Parse(
                _currentUser.UserId ?? throw new UnauthorizedException("User is not logged in")
            );

            var workspace = await _unitOfWork.Workspaces
                .Queryable()
                .Include(x => x.Members.Where(m => !m.IsDeleted))
                .Where(x => x.Id == channel.WorkspaceId)
                .FirstOrDefaultAsync();

            if (workspace is null)
            {
                throw new NotFoundException<Workspace>(channel.WorkspaceId.ToString());
            }

            if (
                !await _unitOfWork.Workspaces.CheckIsMemberAsync(channel.WorkspaceId, currentUserId)
            )
            {
                throw new ForbidException();
            }

            if (channel.Description is null)
            {
                channel.Description = string.Empty;
            }

            channel.OwnerId = currentUserId;

            channel.ChannelMembers = new List<ChannelMember>
            {
                new() { UserId = currentUserId, AddBy = currentUserId }
            };

            if (currentUserId != workspace.OwnerId)
            {
                channel.ChannelMembers.Add(
                    new ChannelMember { UserId = workspace.OwnerId, AddBy = currentUserId }
                );
            }

            channel.CategoryId = Guid.Empty;
            channel = await _unitOfWork.Channels.AddAsync(channel);
            await _unitOfWork.SaveChangeAsync();
            _logger.LogInformation("[{_className}][{method}] End", _className, method);

            return channel.Id;
        }
        catch (Exception e)
        {
            _logger.LogInformation(
                "[{_className}][{method}] Error: {message}",
                _className,
                method,
                e.Message
            );

            throw;
        }
    }

    public async Task<Guid> AddMemberToChannelAsync(Guid channelId, List<Guid> userIds)
    {
        var method = GetActualAsyncMethodName();
        try
        {
            _logger.LogInformation("[{_className}][{method}] Start", _className, method);
            var channel = await _unitOfWork.Channels
                .Queryable()
                .Include(x => x.ChannelMembers.Where(c => !c.IsDeleted))
                .Where(x => x.Id == channelId)
                .FirstOrDefaultAsync();
            var currentUserId = Guid.Parse(
                _currentUser.UserId ?? throw new UnauthorizedException("User is not logged in")
            );
            var currentUser = await _unitOfWork.Users.GetUserByIdAsync(currentUserId);
            if (channel is null)
            {
                throw new NotFoundException<Channel>(channelId.ToString());
            }

            if (!await _unitOfWork.Channels.CheckIsMemberAsync(channelId, currentUserId))
            {
                throw new ForbidException();
            }
            var notification = new Notification
            {
                Title = "You have been added to channel",
                Content =
                    $"You have been added to channel {channel.Name} by {currentUser.Information.FirstName} {currentUser.Information.LastName}",
                TimeToSend = DateTime.UtcNow,
                Status = (short)NOTIFICATION_STATUS.PENDING,
                Type = (short)NOTIFICATION_TYPE.CHANNEL_INVITATION,
                Data = JsonSerializer.Serialize(
                    new Dictionary<string, string>
                    {
                        { "Type", ((short)NOTIFICATION_TYPE.CHANNEL_INVITATION).ToString() },
                        {
                            "Detail",
                            JsonSerializer.Serialize(
                                new InvitedToNewGroup
                                {
                                    GroupId = channelId,
                                    GroupName = channel.Name,
                                    InviterId = currentUserId,
                                    InviterName =
                                        currentUser.Information.FirstName
                                        + " "
                                        + currentUser.Information.LastName,
                                    InviterAvatar = currentUser.Information.Picture,
                                    GroupAvatar = CommonConsts.DEFAULT_CHANNEL_AVATAR
                                }
                            )
                        },
                        {
                            "Url",
                            $"{_config["BaseUrl"]}/Workspace/{channel.WorkspaceId}/{channelId}"
                        },
                        { "Avatar", $"{currentUser.Information.Picture}" }
                    }
                ),
                UserNotifications = new List<UserNotification>()
            };
            foreach (var userId in userIds)
            {
                var workspace = await _unitOfWork.Workspaces
                    .Queryable()
                    .Include(x => x.Members.Where(m => !m.IsDeleted))
                    .Where(w => w.Id == channel.WorkspaceId)
                    .FirstOrDefaultAsync();
                if (!workspace.Members.Any(x => x.UserId == userId))
                {
                    throw new BadRequestException(
                        $"User {userId} is not in the workspace of this channel"
                    );
                }

                var user = await _unitOfWork.Users.FindAsync(userId);
                if (user is null)
                {
                    throw new NotFoundException<User>(userId.ToString());
                }

                var member = channel.ChannelMembers.FirstOrDefault(x => x.UserId == userId);
                if (member is not null)
                {
                    throw new BadRequestException($"User {userId} is already in this channel");
                }

                var channelMember = new ChannelMember { UserId = userId, AddBy = currentUserId };

                channel.ChannelMembers.Add(channelMember);
                notification.UserNotifications.Add(
                    new UserNotification
                    {
                        UserId = userId,
                        Status = (short)NOTIFICATION_STATUS.PENDING,
                        SendAt = DateTimeOffset.UtcNow
                    }
                );
            }
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

            _logger.LogInformation("[{_className}][{method}] End", _className, method);

            return channelId;
        }
        catch (Exception e)
        {
            _logger.LogInformation(
                "[{_className}][{method}] Error: {message}",
                _className,
                method,
                e.Message
            );

            throw;
        }
    }

    public async Task<Guid> AddRoleAsync(Guid channelId, CreateUpdateChannelRoleDto input)
    {
        var method = GetActualAsyncMethodName();
        _logger.LogInformation("[{_className}][{method}] Start", _className, method);
        var userId = Guid.Parse(_currentUser.UserId ?? throw new UnauthorizedAccessException());
        var isMember = await _unitOfWork.Channels.CheckIsMemberAsync(channelId, userId);
        if (!isMember)
        {
            throw new ForbidException();
        }

        var role = _mapper.Map<ChannelRole>(input);
        role = await _unitOfWork.Channels.AddRoleAsync(channelId, role);

        await _unitOfWork.SaveChangeAsync();

        _logger.LogInformation("[{_className}][{method}] End", _className, method);

        return role.Id;
    }

    public async Task<Guid> DeleteAsync(Guid channelId)
    {
        var method = GetActualAsyncMethodName();
        try
        {
            _logger.LogInformation("[{_className}][{method}] Start", _className, method);
            var channel = await _unitOfWork.Channels
                .Queryable()
                .Include(x => x.ChannelMembers.Where(c => !c.IsDeleted))
                .Where(x => x.Id == channelId)
                .FirstOrDefaultAsync();

            if (channel is null)
                throw new NotFoundException<Channel>(channelId.ToString());

            var currentUserId = Guid.Parse(
                _currentUser.UserId ?? throw new UnauthorizedException("User is not logged in")
            );

            if (!await _unitOfWork.Channels.CheckIsMemberAsync(channelId, currentUserId))
            {
                throw new ForbidException();
            }

            await _unitOfWork.Channels.DeleteAsync(channel);
            await _unitOfWork.SaveChangeAsync();
            _logger.LogInformation("[{_className}][{method}] End", _className, method);

            return channel.Id;
        }
        catch (Exception e)
        {
            _logger.LogInformation(
                "[{_className}][{method}] Error: {message}",
                _className,
                method,
                e.Message
            );

            throw;
        }
    }

    public async Task DeleteRoleAsync(Guid channelId, Guid roleId)
    {
        var method = GetActualAsyncMethodName();
        _logger.LogInformation("[{_className}][{method}] Start", _className, method);
        var isMember = await _unitOfWork.Channels.CheckIsMemberAsync(
            channelId,
            Guid.Parse(_currentUser.UserId ?? throw new UnauthorizedAccessException())
        );
        if (!isMember)
        {
            throw new ForbidException();
        }
        var isExistRole = await _unitOfWork.Channels.CheckIsExistRole(channelId, roleId);
        if (!isExistRole)
        {
            throw new NotFoundException<ChannelRole>(roleId.ToString());
        }
        var channelRole = await _unitOfWork.Channels.GetRoleById(channelId, roleId);
        foreach (var permission in channelRole.Permissions)
        {
            await _unitOfWork.PermissionsOfChannelRoles.DeleteAsync(permission, isHardDelete: true);
        }
        await _unitOfWork.ChannelRoles.DeleteAsync(channelRole, isHardDelete: true);
        await _unitOfWork.SaveChangeAsync();
        _logger.LogInformation("[{_className}][{method}] End", _className, method);
    }

    public async Task<IEnumerable<ChannelDto>> GetAllChannelsOfAWorkspaceAsync(Guid workspaceId)
    {
        var method = GetActualAsyncMethodName();
        try
        {
            _logger.LogInformation("[{_className}][{method}] Start", _className, method);
            var currentUserId = Guid.Parse(
                _currentUser.UserId ?? throw new UnauthorizedException("User is not logged in")
            );

            var isMember = await _unitOfWork.Workspaces.CheckIsMemberAsync(
                workspaceId,
                currentUserId
            );
            if (!isMember)
            {
                throw new ForbidException();
            }

            var channels = await _unitOfWork.Channels
                .Queryable()
                .Include(x => x.ChannelMembers.Where(c => !c.IsDeleted))
                .Where(
                    x =>
                        x.WorkspaceId == workspaceId
                        && x.ChannelMembers.Any(c => c.UserId == currentUserId)
                )
                .ToListAsync();
            _logger.LogInformation("[{_className}][{method}] End", _className, method);
            return _mapper.Map<IEnumerable<ChannelDto>>(channels);
        }
        catch (Exception e)
        {
            _logger.LogInformation(
                "[{_className}][{method}] Error: {message}",
                _className,
                method,
                e.Message
            );

            throw;
        }
    }

    public async Task<ChannelDto> GetByIdAsync(Guid channelId)
    {
        var method = GetActualAsyncMethodName();
        try
        {
            _logger.LogInformation("[{_className}][{method}] Start", _className, method);
            var channel = await _unitOfWork.Channels
                .Queryable()
                .Include(x => x.ChannelMembers.Where(c => !c.IsDeleted))
                .FirstOrDefaultAsync(x => x.Id == channelId);

            if (channel is null)
                throw new NotFoundException<Channel>(channelId.ToString());

            var currentUserId = Guid.Parse(
                _currentUser.UserId ?? throw new UnauthorizedException("User is not logged in")
            );

            if (!await _unitOfWork.Channels.CheckIsMemberAsync(channelId, currentUserId))
            {
                throw new ForbidException();
            }

            _logger.LogInformation("[{_className}][{method}] End", _className, method);
            return _mapper.Map<ChannelDto>(channel);
        }
        catch (Exception e)
        {
            _logger.LogInformation(
                "[{_className}][{method}] Error: {message}",
                _className,
                method,
                e.Message
            );

            throw;
        }
    }

    public async Task<IEnumerable<ChannelDto>> GetByNameAsync(string channelName)
    {
        var method = GetActualAsyncMethodName();
        try
        {
            _logger.LogInformation("[{_className}][{method}] Start", _className, method);
            var channels = await _unitOfWork.Channels
                .Queryable()
                .Include(x => x.ChannelMembers.Where(c => !c.IsDeleted))
                .Where(x => x.Name.Contains(channelName))
                .ToListAsync();

            var currentUserId = Guid.Parse(
                _currentUser.UserId ?? throw new UnauthorizedException("User is not logged in")
            );
            channels = channels
                .Where(x => x.ChannelMembers.Any(c => c.UserId == currentUserId))
                .ToList();

            _logger.LogInformation("[{_className}][{method}] End", _className, method);
            return _mapper.Map<IEnumerable<ChannelDto>>(channels);
        }
        catch (Exception e)
        {
            _logger.LogInformation(
                "[{_className}][{method}] Error: {message}",
                _className,
                method,
                e.Message
            );

            throw;
        }
    }

    public async Task<IEnumerable<Guid>> GetChannelsOfUserAsync(Guid userId)
    {
        var method = GetActualAsyncMethodName();
        _logger.LogInformation("[{_className}][{method}] Start", _className, method);
        var channels = await _unitOfWork.Channels
            .Queryable()
            .Include(x => x.ChannelMembers.Where(c => !c.IsDeleted))
            .Where(x => x.ChannelMembers.Any(c => c.UserId == userId) && !x.IsDeleted)
            .ToListAsync();

        _logger.LogInformation("[{_className}][{method}] End", _className, method);
        return channels.Select(x => x.Id);
    }

    public async Task<IEnumerable<PermissionDto>> GetPermissionOfUser(Guid channelId, Guid userId)
    {
        var method = GetActualAsyncMethodName();
        _logger.LogInformation("[{_className}][{method}] Start", _className, method);
        var isExist = await _unitOfWork.Channels.CheckIsExistAsync(channelId);
        if (!isExist)
        {
            throw new NotFoundException<Channel>(channelId.ToString());
        }
        var isMember = _unitOfWork.Channels.CheckIsMemberAsync(channelId, userId);
        if (!isMember.Result)
        {
            throw new ForbidException();
        }
        var isOwner = await _unitOfWork.Channels.CheckIsOwnerAsync(channelId, userId);

        var permissions = await _unitOfWork.ChannelMembers.GetPermissionOfUser(channelId, userId);
        if (isOwner)
        {
            permissions = permissions.Concat(
                await _unitOfWork.ChannelPermissions
                    .Queryable()
                    .Where(x => !x.IsDeleted && x.IsActive)
                    .ToListAsync()
            );
        }
        _logger.LogInformation("[{_className}][{method}] End", _className, method);
        return _mapper.Map<IEnumerable<PermissionDto>>(permissions);
    }

    public async Task<IEnumerable<PermissionDto>> GetPermissions()
    {
        var method = GetActualAsyncMethodName();
        _logger.LogInformation("[{_className}][{method}] Start", _className, method);

        var permissions = _unitOfWork.ChannelPermissions
            .Queryable()
            .Where(x => !x.IsDeleted && x.IsActive);
        await Task.CompletedTask;
        _logger.LogInformation("[{_className}][{method}] End", _className, method);

        return _mapper.Map<IEnumerable<PermissionDto>>(permissions);
    }

    public async Task<IEnumerable<PermissionDto>> GetPermissionsAsync(Guid channelId, Guid roleId)
    {
        var method = GetActualAsyncMethodName();
        _logger.LogInformation("[{_className}][{method}] Start", _className, method);
        var isMember = await _unitOfWork.Channels.CheckIsMemberAsync(
            channelId,
            Guid.Parse(_currentUser.UserId ?? throw new UnauthorizedAccessException())
        );
        if (!isMember)
        {
            throw new ForbidException();
        }
        var isExistRole = await _unitOfWork.Channels.CheckIsExistRole(channelId, roleId);
        if (!isExistRole)
        {
            throw new NotFoundException<ChannelRole>(roleId.ToString());
        }
        var channelRole = await _unitOfWork.Channels.GetRoleById(channelId, roleId);

        var permissions = channelRole.Permissions
            .Where(x => !x.IsDeleted && x.Permission.IsActive)
            .ToList();
        var permissionDtos = _mapper.Map<List<PermissionDto>>(permissions);
        permissionDtos.ForEach(x => x.IsEnabled = true);

        var activePermissions = _unitOfWork.ChannelPermissions
            .Queryable()
            .Where(x => !x.IsDeleted && x.IsActive);

        permissionDtos.AddRange(
            _mapper.Map<List<PermissionDto>>(
                activePermissions.Where(x => !permissionDtos.Select(x => x.Id).Contains(x.Id))
            )
        );

        _logger.LogInformation("[{_className}][{method}] End", _className, method);

        return permissionDtos;
    }

    public async Task<ChannelRoleDto> GetRoleAsync(Guid channelId, Guid roleId)
    {
        var method = GetActualAsyncMethodName();
        _logger.LogInformation("[{_className}][{method}] Start", _className, method);
        var isMember = await _unitOfWork.Channels.CheckIsMemberAsync(
            channelId,
            Guid.Parse(_currentUser.UserId ?? throw new UnauthorizedAccessException())
        );
        if (!isMember)
        {
            throw new ForbidException();
        }

        var isExistRole = await _unitOfWork.Channels.CheckIsExistRole(channelId, roleId);
        if (!isExistRole)
        {
            throw new NotFoundException<ChannelRole>(roleId.ToString());
        }

        var channelRole = await _unitOfWork.Channels.GetRoleById(channelId, roleId);
        var channelRoleDto = _mapper.Map<ChannelRoleDto>(channelRole);
        channelRoleDto.Permissions.ForEach(x => x.IsEnabled = true);

        var activePermissions = _unitOfWork.ChannelPermissions
            .Queryable()
            .Where(x => !x.IsDeleted && x.IsActive);

        channelRoleDto.Permissions.AddRange(
            _mapper.Map<List<PermissionDto>>(
                activePermissions.Where(
                    x => !channelRoleDto.Permissions.Select(x => x.Id).Contains(x.Id)
                )
            )
        );

        _logger.LogInformation("[{_className}][{method}] End", _className, method);

        return channelRoleDto;
    }

    public async Task<IEnumerable<ChannelRoleDto>> GetRolesAsync(Guid channelId)
    {
        var method = GetActualAsyncMethodName();
        var userId = Guid.Parse(_currentUser.UserId ?? throw new UnauthorizedAccessException());
        _logger.LogInformation("[{_className}][{method}] Start", _className, method);
        var isMember = await _unitOfWork.Channels.CheckIsMemberAsync(channelId, userId);
        if (!isMember)
        {
            throw new ForbidException();
        }

        _logger.LogInformation("[{_className}][{method}] End", _className, method);

        return _mapper.Map<IEnumerable<ChannelRoleDto>>(
            await _unitOfWork.Channels.GetRoles(channelId)
        );
    }

    public async Task<Guid> RemoveMemberFromChannelAsync(Guid channelId, List<Guid> userIds)
    {
        var method = GetActualAsyncMethodName();
        try
        {
            _logger.LogInformation("[{_className}][{method}] Start", _className, method);
            var channel = await _unitOfWork.Channels
                .Queryable()
                .Include(x => x.ChannelMembers.Where(c => !c.IsDeleted))
                .Where(x => x.Id == channelId)
                .FirstOrDefaultAsync();

            var currentUserId = Guid.Parse(
                _currentUser.UserId ?? throw new UnauthorizedException("User is not logged in")
            );
            var currentUser = await _unitOfWork.Users.GetUserByIdAsync(currentUserId);
            if (channel is null)
            {
                throw new NotFoundException<Channel>(channelId.ToString());
            }

            if (!await _unitOfWork.Channels.CheckIsMemberAsync(channelId, currentUserId))
            {
                throw new ForbidException();
            }
            var notification = new Notification
            {
                Title = "You have been removed from channel",
                Content = $"You have been removed from channel {channel.Name}",
                TimeToSend = DateTime.UtcNow,
                Status = (short)NOTIFICATION_STATUS.PENDING,
                Type = (short)NOTIFICATION_TYPE.CHANNEL_REMOVED,
                Data = JsonSerializer.Serialize(
                    new Dictionary<string, string>
                    {
                        { "Type", ((short)NOTIFICATION_TYPE.CHANNEL_REMOVED).ToString() },
                        {
                            "Detail",
                            JsonSerializer.Serialize(
                                new RemovedFromGroup
                                {
                                    GroupId = channelId,
                                    GroupName = channel.Name,
                                    RemoverId = currentUserId,
                                    RemoverName =
                                        currentUser.Information.FirstName
                                        + " "
                                        + currentUser.Information.LastName,
                                    RemoverAvatar = currentUser.Information.Picture,
                                    GroupAvatar = CommonConsts.DEFAULT_CHANNEL_AVATAR
                                }
                            )
                        },
                        {
                            "Url",
                            $"{_config["BaseUrl"]}/Workspace/{channel.WorkspaceId}/{channelId}"
                        },
                        { "Avatar", $"{currentUser.Information.Picture}" }
                    }
                ),
                UserNotifications = new List<UserNotification>()
            };

            foreach (var userId in userIds)
            {
                var member = channel.ChannelMembers.FirstOrDefault(x => x.UserId == userId);
                if (member is null)
                {
                    throw new BadRequestException($"User {userId} is not in this channel");
                }
                notification.UserNotifications.Add(
                    new UserNotification
                    {
                        UserId = userId,
                        Status = (short)NOTIFICATION_STATUS.PENDING,
                        SendAt = DateTime.UtcNow
                    }
                );

                await _unitOfWork.ChannelMembers.DeleteAsync(member);
            }
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
                    _backgroundJobClient.Enqueue(
                        () => _hubService.RemoveUsersFromChannelHub(channelId, userIds)
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

            return channel.Id;
        }
        catch (Exception e)
        {
            _logger.LogInformation(
                "[{_className}][{method}] Error: {message}",
                _className,
                method,
                e.Message
            );

            throw;
        }
    }

    public async Task SetRoleToUserAsync(Guid channelId, Guid userId, Guid? roleId)
    {
        var method = GetActualAsyncMethodName();
        _logger.LogInformation("[{_className}][{method}] Start", _className, method);
        var isMember = await _unitOfWork.Channels.CheckIsMemberAsync(
            channelId,
            Guid.Parse(_currentUser.UserId ?? throw new UnauthorizedAccessException())
        );
        if (!isMember)
        {
            throw new ForbidException();
        }
        var isExistRole =
            roleId is null || await _unitOfWork.Channels.CheckIsExistRole(channelId, roleId.Value);
        if (!isExistRole)
        {
            throw new NotFoundException<ChannelRole>(roleId.ToString());
        }

        isMember = await _unitOfWork.Channels.CheckIsMemberAsync(channelId, userId);
        if (!isMember)
        {
            throw new NotAMemberOfWorkspaceException();
        }
        var member = await _unitOfWork.Channels.GetMemberByUserId(channelId, userId);
        member.RoleId = roleId;
        await _unitOfWork.ChannelMembers.UpdateAsync(member);
        await _unitOfWork.SaveChangeAsync();
        _logger.LogInformation("[{_className}][{method}] End", _className, method);
    }

    public async Task<Guid> UpdateAsync(Guid channelId, UpdateChannelDto updateChannelDto)
    {
        var method = GetActualAsyncMethodName();
        try
        {
            _logger.LogInformation("[{_className}][{method}] Start", _className, method);
            var channel = await _unitOfWork.Channels
                .Queryable()
                .Include(x => x.ChannelMembers.Where(c => !c.IsDeleted))
                .Where(x => x.Id == channelId)
                .FirstOrDefaultAsync();
            var currentUserId = Guid.Parse(
                _currentUser.UserId ?? throw new UnauthorizedException("User is not logged in")
            );

            if (channel is null)
            {
                throw new NotFoundException<Channel>(channelId.ToString());
            }

            if (!await _unitOfWork.Channels.CheckIsMemberAsync(channelId, currentUserId))
            {
                throw new ForbidException();
            }

            if (updateChannelDto.Description is null)
            {
                updateChannelDto.Description = string.Empty;
            }

            if (updateChannelDto.CategoryId is null)
            {
                updateChannelDto.CategoryId = Guid.Empty;
            }

            _mapper.Map(updateChannelDto, channel);
            await _unitOfWork.Channels.UpdateAsync(channel);
            await _unitOfWork.SaveChangeAsync();

            _logger.LogInformation("[{_className}][{method}] End", _className, method);
            return channel.Id;
        }
        catch (Exception e)
        {
            _logger.LogInformation(
                "[{_className}][{method}] Error: {message}",
                _className,
                method,
                e.Message
            );

            throw;
        }
    }

    public async Task UpdateRoleAsync(Guid channelId, Guid roleId, CreateUpdateChannelRoleDto input)
    {
        var method = GetActualAsyncMethodName();
        _logger.LogInformation("[{_className}][{method}] Start", _className, method);
        var userId = Guid.Parse(_currentUser.UserId ?? throw new UnauthorizedAccessException());
        var isMember = await _unitOfWork.Channels.CheckIsMemberAsync(channelId, userId);
        if (!isMember)
        {
            throw new ForbidException();
        }
        var isExistRole = await _unitOfWork.Channels.CheckIsExistRole(channelId, roleId);
        if (!isExistRole)
        {
            throw new NotFoundException<WorkspaceRole>(roleId.ToString());
        }

        var role = await _unitOfWork.Channels.GetRoleById(channelId, roleId);
        foreach (var permission in role.Permissions)
        {
            await _unitOfWork.PermissionsOfChannelRoles.DeleteAsync(permission, isHardDelete: true);
        }
        _mapper.Map(input, role);
        await _unitOfWork.ChannelRoles.UpdateAsync(role);
        await _unitOfWork.SaveChangeAsync();

        _logger.LogInformation("[{_className}][{method}] End", _className, method);
    }

    public async Task<IEnumerable<UserDetailDto>> GetMembersByRoleIdAsync(Guid channelId, Guid roleId)
    {
        var method = GetActualAsyncMethodName();
        _logger.LogInformation("[{_className}][{method}] Start", _className, method);
        var isRoleExist = await _unitOfWork.Channels.CheckIsExistRole(channelId, roleId);
        var currentUserId = Guid.Parse(
            _currentUser.UserId ?? throw new UnauthorizedAccessException()
        );
        if (!isRoleExist)
        {
            throw new NotFoundException<ChannelRole>(roleId.ToString());
        }
        var isMember = await _unitOfWork.Channels.CheckIsMemberAsync(channelId, currentUserId);

        if (!isMember)
        {
            throw new ForbidException();
        }

        var members = await _unitOfWork.ChannelMembers
            .Queryable()
            .Include(x => x.User)
            .ThenInclude(x => x.Information)
            .Where(x => x.ChannelId == channelId && x.RoleId == roleId)
            .ToListAsync();
        _logger.LogInformation("[{_className}][{method}] End", _className, method);
        return _mapper.Map<IEnumerable<UserDetailDto>>(members.Select(x => x.User));
    }

    public async Task<IEnumerable<UserDetailDto>> GetMembersWithoutRoleAsync(Guid channelId)
    {
        var method = GetActualAsyncMethodName();
        _logger.LogInformation("[{_className}][{method}] Start", _className, method);
        var isExist = await _unitOfWork.Channels.CheckIsExistAsync(channelId);
        var currentUserId = Guid.Parse(
            _currentUser.UserId ?? throw new UnauthorizedAccessException()
        );
        if (!isExist)
        {
            throw new NotFoundException<Channel>(channelId.ToString());
        }

        var isMember = await _unitOfWork.Channels.CheckIsMemberAsync(channelId, currentUserId);
        if (!isMember)
        {
            throw new ForbidException();
        }

        var members = await _unitOfWork.ChannelMembers
            .Queryable()
            .Include(x => x.User)
            .ThenInclude(x => x.Information)
            .Where(x => x.ChannelId == channelId && x.RoleId == null)
            .ToListAsync();
        _logger.LogInformation("[{_className}][{method}] End", _className, method);
        return _mapper.Map<IEnumerable<UserDetailDto>>(members.Select(x => x.User));
    }

    public async Task<IEnumerable<UserDetailDto>> GetMembersThatNotInTheChannel(
        Guid workspaceId,
        Guid channelId
    )
    {
        var method = GetActualAsyncMethodName();
        _logger.LogInformation("[{_className}][{method}] Start", _className, method);
        var isWorkspaceExist = await _unitOfWork.Workspaces.CheckIsExistAsync(workspaceId);

        if (!isWorkspaceExist)
        {
            throw new NotFoundException<Workspace>(workspaceId.ToString());
        }

        var isChannelExist = await _unitOfWork.Channels.CheckIsExistAsync(channelId);

        if (!isChannelExist)
        {
            throw new NotFoundException<Channel>(channelId.ToString());
        }

        var currentUserId = Guid.Parse(
            _currentUser.UserId ?? throw new UnauthorizedAccessException()
        );

        var isMember = await _unitOfWork.Workspaces.CheckIsMemberAsync(workspaceId, currentUserId);

        if (!isMember)
        {
            throw new ForbidException();
        }

        var members = await _unitOfWork.WorkspaceMembers
            .Queryable()
            .Include(x => x.User)
            .ThenInclude(x => x.Information)
            .Where(x => x.WorkspaceId == workspaceId && !x.IsDeleted)
            .ToListAsync();

        var channelMembers = await _unitOfWork.ChannelMembers
            .Queryable()
            .Include(x => x.User)
            .ThenInclude(x => x.Information)
            .Where(x => x.ChannelId == channelId && !x.IsDeleted)
            .ToListAsync();

        var result = members.Where(x => !channelMembers.Select(x => x.UserId).Contains(x.UserId));

        _logger.LogInformation("[{_className}][{method}] End", _className, method);
        return _mapper.Map<IEnumerable<UserDetailDto>>(result.Select(x => x.User));
    }

    public async Task<IEnumerable<ChannelUserDto>> GetMembersAsync(Guid channelId)
    {
        var method = GetActualAsyncMethodName();
        _logger.LogInformation("[{_className}][{method}] Start", _className, method);
        var isExist = await _unitOfWork.Channels.CheckIsExistAsync(channelId);
        var currentUserId = Guid.Parse(
            _currentUser.UserId ?? throw new UnauthorizedAccessException()
        );
        if (!isExist)
        {
            throw new NotFoundException<Channel>(channelId.ToString());
        }

        var isMember = await _unitOfWork.Channels.CheckIsMemberAsync(channelId, currentUserId);
        if (!isMember)
        {
            throw new ForbidException();
        }

        var members = await _unitOfWork.ChannelMembers
            .Queryable()
            .Include(x => x.User)
            .ThenInclude(x => x.Information)
            .Include(x => x.ChannelRole)
            .Where(x => x.ChannelId == channelId)
            .ToListAsync();
        _logger.LogInformation("[{_className}][{method}] End", _className, method);
        return _mapper.Map<IEnumerable<ChannelUserDto>>(members);
    }

    public async Task AcceptInvitationAsync(Guid channelId)
    {
        var method = GetActualAsyncMethodName();
        _logger.LogInformation("[{_className}][{method}] Start", _className, method);
        var userId = Guid.Parse(_currentUser.UserId ?? throw new UnauthorizedAccessException());
        var isInvited = await _unitOfWork.Channels.CheckIsInvitedAsync(channelId, userId);
        if (!isInvited)
        {
            throw new BadRequestException("Invitation is not exist");
        }
        var member = await _unitOfWork.Channels.GetMemberByUserId(channelId, userId);
        member.Status = (short)CHANNEL_MEMBER_STATUS.ACTIVE;
        await _unitOfWork.ChannelMembers.UpdateAsync(member);
        await _unitOfWork.SaveChangeAsync();

         try
        {
            _backgroundJobClient.Enqueue(
                () => _hubService.AddUsersToChannelHub(channelId, new List<Guid> { userId })
            );
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
    }

    public async Task DeclineInvitationAsync(Guid channelId)
    {
        var method = GetActualAsyncMethodName();
        _logger.LogInformation("[{_className}][{method}] Start", _className, method);
        var userId = Guid.Parse(_currentUser.UserId ?? throw new UnauthorizedAccessException());
        var isInvited = await _unitOfWork.Channels.CheckIsInvitedAsync(channelId, userId);
        if (!isInvited)
        {
            throw new BadRequestException("Invitation is not exist");
        }
        var member = await _unitOfWork.Channels.GetMemberByUserId(channelId, userId);
        await _unitOfWork.ChannelMembers.DeleteAsync(member);
        await _unitOfWork.SaveChangeAsync();
       
        _logger.LogInformation("[{_className}][{method}] End", _className, method);
    }

    public async Task<Guid> LeaveChannelAsync(Guid channelId)
    {
        var method = GetActualAsyncMethodName();
        _logger.LogInformation("[{_className}][{method}] Start", _className, method);
        var userId = Guid.Parse(_currentUser.UserId ?? throw new UnauthorizedAccessException());
        var isMember = await _unitOfWork.Channels.CheckIsMemberAsync(channelId, userId);
        if (!isMember)
        {
            throw new ForbidException();
        }
        var member = await _unitOfWork.Channels.GetMemberByUserId(channelId, userId);
        member.Status = (short)CHANNEL_MEMBER_STATUS.REMOVED;
        await _unitOfWork.ChannelMembers.UpdateAsync(member);
        await _unitOfWork.SaveChangeAsync();
        _logger.LogInformation("[{_className}][{method}] End", _className, method);
        return channelId;
    }
}
