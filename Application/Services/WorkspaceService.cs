using System.Runtime.CompilerServices;
using Application.Contract.Workspaces.Dtos;
using Hangfire;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using PBL6.Application.Contract.Channels;
using PBL6.Application.Contract.ExternalServices.Mails;
using PBL6.Application.Contract.ExternalServices.Notifications.Dtos;
using PBL6.Application.Contract.Users.Dtos;
using PBL6.Application.Contract.Workspaces;
using PBL6.Application.Contract.Workspaces.Dtos;
using PBL6.Common.Consts;
using PBL6.Common.Enum;
using PBL6.Common.Exceptions;
using PBL6.Domain.Models.Users;

namespace PBL6.Application.Services
{
    public class WorkspaceService : BaseService, IWorkspaceService
    {
        private readonly string _className;
        private readonly IChannelService _channelService;

        public WorkspaceService(IServiceProvider serviceProvider, IChannelService channelService)
            : base(serviceProvider)
        {
            _className = typeof(WorkspaceService).Name;
            _channelService = channelService;
        }

        static string GetActualAsyncMethodName([CallerMemberName] string name = null) => name;

        public async Task<Guid> AddAsync(CreateWorkspaceDto workspaceDto)
        {
            var method = GetActualAsyncMethodName();
            try
            {
                _logger.LogInformation("[{_className}][{method}] Start", _className, method);
                var workspace = _mapper.Map<Workspace>(workspaceDto);
                var userId = Guid.Parse(
                    _currentUser.UserId ?? throw new UnauthorizedAccessException()
                );

                if (workspaceDto.Avatar is not null)
                {
                    var filename = workspace.Id.ToString() + Path.GetExtension(
                        workspaceDto.Avatar.FileName
                    );
                    workspace.AvatarUrl = await _fileService.UploadFileGetUrlAsync(
                        filename,
                        workspaceDto.Avatar.OpenReadStream(),
                        "image/png"
                    );
                }
                else
                {
                    workspace.AvatarUrl = CommonConsts.DEFAULT_WORKSPACE_AVATAR;
                }

                if (workspaceDto.Description is null)
                {
                    workspace.Description = string.Empty;
                }

                workspace.OwnerId = userId;
                workspace.Channels = new List<Channel>
                {
                    new()
                    {
                        OwnerId = userId,
                        Name = CommonConsts.DEFAULT_CHANNEL,
                        Description = string.Empty,
                        ChannelMembers = new List<ChannelMember>
                        {
                            // TODO: Change roleId, status
                            new() { UserId = userId, AddBy = userId }
                        }
                    }
                };
                workspace.Members = new List<WorkspaceMember>
                {
                    // TODO: Change roleId, status
                    new() { UserId = userId, AddBy = userId }
                };
                workspace.Status = (short)WORKSPACE_STATUS.ACTIVE;
                workspace = await _unitOfWork.Workspaces.AddAsync(workspace);
                await _unitOfWork.SaveChangeAsync();
                _logger.LogInformation("[{_className}][{method}] End", _className, method);

                return workspace.Id;
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

        public async Task<Guid> DeleteAsync(Guid workspaceId)
        {
            var method = GetActualAsyncMethodName();
            try
            {
                _logger.LogInformation("[{_className}][{method}] Start", _className, method);
                var workspace = await _unitOfWork.Workspaces
                    .Queryable()
                    .Include(x => x.Members.Where(m => !m.IsDeleted))
                    .FirstOrDefaultAsync(x => x.Id == workspaceId);
                if (workspace is null)
                    throw new NotFoundException<Workspace>(workspaceId.ToString());

                var currentUserId = Guid.Parse(
                    _currentUser.UserId ?? throw new UnauthorizedAccessException()
                );

                if (!await _unitOfWork.Workspaces.CheckIsMemberAsync(workspaceId, currentUserId))
                    throw new ForbidException();

                await _unitOfWork.Workspaces.DeleteAsync(workspace);
                await _unitOfWork.SaveChangeAsync();
                _logger.LogInformation("[{_className}][{method}] End", _className, method);

                return workspace.Id;
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

        public async Task<IEnumerable<WorkspaceDto>> GetAllAsync()
        {
            var method = GetActualAsyncMethodName();
            try
            {
                _logger.LogInformation("[{_className}][{method}] Start", _className, method);
                var workspaces = await _unitOfWork.Workspaces
                    .Queryable()
                    .Include(x => x.Channels)
                    .Include(x => x.Members)
                    .ThenInclude(m => m.User)
                    .ThenInclude(u => u.Information)
                    .ToListAsync();

                workspaces.ForEach(w =>
                {
                    w.Channels = w.Channels.Where(c => !c.IsDeleted).ToList();
                    w.Members = w.Members.Where(m => !m.IsDeleted).ToList();
                });

                var userId = Guid.Parse(
                    _currentUser.UserId ?? throw new UnauthorizedException("User is not logged in")
                );
                workspaces = workspaces.Where(x => x.Members.Any(m => m.UserId == userId)).ToList();

                foreach (var workspace in workspaces)
                {
                    var channels = await _channelService.GetAllChannelsOfAWorkspaceAsync(
                        workspace.Id
                    );
                    foreach (var channel in channels)
                    {
                        if (!channel.ChannelMembers.Any(x => x == userId))
                        {
                            workspace.Channels = workspace.Channels
                                .Where(x => x.Id != channel.Id)
                                .ToList();
                        }
                    }
                }
                _logger.LogInformation("[{_className}][{method}] End", _className, method);
                return _mapper.Map<IEnumerable<WorkspaceDto>>(workspaces);
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

        public async Task<WorkspaceDto> GetByIdAsync(Guid workspaceId)
        {
            var method = GetActualAsyncMethodName();
            try
            {
                _logger.LogInformation("[{_className}][{method}] Start", _className, method);

                var workspace = await _unitOfWork.Workspaces.GetAsync(workspaceId);

                var userId = Guid.Parse(
                    _currentUser.UserId ?? throw new UnauthorizedException("User is not logged in")
                );

                if (!await _unitOfWork.Workspaces.CheckIsMemberAsync(workspaceId, userId))
                    throw new ForbidException();

                workspace.Channels = workspace.Channels.Where(c => !c.IsDeleted).ToList();
                workspace.Members = workspace.Members.Where(m => !m.IsDeleted).ToList();

                var channels = await _channelService.GetAllChannelsOfAWorkspaceAsync(workspace.Id);
                foreach (var channel in channels)
                {
                    if (!channel.ChannelMembers.Any(x => x == userId))
                    {
                        workspace.Channels = workspace.Channels
                            .Where(x => x.Id != channel.Id)
                            .ToList();
                    }
                }

                _logger.LogInformation("[{_className}][{method}] End", _className, method);
                return _mapper.Map<WorkspaceDto>(workspace);
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

        public async Task<IEnumerable<WorkspaceDto>> GetByNameAsync(string workspaceName)
        {
            var method = GetActualAsyncMethodName();
            try
            {
                _logger.LogInformation("[{_className}][{method}] Start", _className, method);
                var workspaces = await _unitOfWork.Workspaces
                    .Queryable()
                    .Include(x => x.Channels.Where(c => !c.IsDeleted))
                    .Include(x => x.Members.Where(m => !m.IsDeleted))
                    .ThenInclude(m => m.User)
                    .ThenInclude(u => u.Information)
                    .Where(x => x.Name.Contains(workspaceName))
                    .ToListAsync();

                var userId = Guid.Parse(
                    _currentUser.UserId ?? throw new UnauthorizedException("User is not logged in")
                );
                workspaces = workspaces.Where(x => x.Members.Any(m => m.UserId == userId)).ToList();

                foreach (var workspace in workspaces)
                {
                    var channels = await _channelService.GetAllChannelsOfAWorkspaceAsync(
                        workspace.Id
                    );
                    foreach (var channel in channels)
                    {
                        if (!channel.ChannelMembers.Any(x => x == userId))
                        {
                            workspace.Channels = workspace.Channels
                                .Where(x => x.Id != channel.Id)
                                .ToList();
                        }
                    }
                }

                _logger.LogInformation("[{_className}][{method}] End", _className, method);
                return _mapper.Map<IEnumerable<WorkspaceDto>>(workspaces);
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

        public async Task<Guid> UpdateAsync(Guid workspaceId, UpdateWorkspaceDto updateWorkspaceDto)
        {
            var method = GetActualAsyncMethodName();
            try
            {
                _logger.LogInformation("[{_className}][{method}] Start", _className, method);
                var userId = Guid.Parse(
                    _currentUser.UserId ?? throw new UnauthorizedException("User is not logged in")
                );
                var workspace = await _unitOfWork.Workspaces.GetAsync(workspaceId) ?? throw new NotFoundException<Workspace>(workspaceId.ToString());
                if (!await _unitOfWork.Workspaces.CheckIsMemberAsync(workspaceId, userId))
                    throw new ForbidException();

                updateWorkspaceDto.Description ??= string.Empty;

                _mapper.Map(updateWorkspaceDto, workspace);
                await _unitOfWork.Workspaces.UpdateAsync(workspace);
                await _unitOfWork.SaveChangeAsync();

                _logger.LogInformation("[{_className}][{method}] End", _className, method);
                return workspace.Id;
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

        public async Task<Guid> UpdateAvatarAsync(
            Guid workspaceId,
            UpdateAvatarWorkspaceDto updateAvatarWorkspaceDto
        )
        {
            var method = GetActualAsyncMethodName();
            try
            {
                _logger.LogInformation("[{_className}][{method}] Start", _className, method);
                var userId = Guid.Parse(
                    _currentUser.UserId ?? throw new UnauthorizedException("User is not logged in")
                );
                var workspace = await _unitOfWork.Workspaces.GetAsync(workspaceId) ?? throw new NotFoundException<Workspace>(workspaceId.ToString());

                if (!await _unitOfWork.Workspaces.CheckIsMemberAsync(workspaceId, userId))
                    throw new ForbidException();

                if (updateAvatarWorkspaceDto.Avatar is not null)
                {
                    var filename = workspace.Id.ToString() + Path.GetExtension(
                        updateAvatarWorkspaceDto.Avatar.FileName
                    );
                    workspace.AvatarUrl = await _fileService.UploadFileGetUrlAsync(
                        filename,
                        updateAvatarWorkspaceDto.Avatar.OpenReadStream(),
                        "image/png"
                    );
                }

                await _unitOfWork.Workspaces.UpdateAsync(workspace);
                await _unitOfWork.SaveChangeAsync();

                _logger.LogInformation("[{_className}][{method}] End", _className, method);

                return workspace.Id;
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

        public async Task<Guid> InviteMemberToWorkspaceAsync(Guid workspaceId, List<string> emails)
        {
            var method = GetActualAsyncMethodName();
            try
            {
                _logger.LogInformation("[{_className}][{method}] Start", _className, method);
                var currentUserId = Guid.Parse(
                    _currentUser.UserId ?? throw new UnauthorizedException("User is not logged in")
                );
                var currentUser = await _unitOfWork.Users.GetUserByIdAsync(currentUserId);
                var workspace = await _unitOfWork.Workspaces.GetAsync(workspaceId) ?? throw new NotFoundException<Workspace>(workspaceId.ToString());
                if (!await _unitOfWork.Workspaces.CheckIsMemberAsync(workspaceId, currentUserId))
                {
                    throw new ForbidException();
                }
                var notification = new Notification
                {
                    Title = "You have been invited to join a workspace",
                    Content =
                        $"You have been invited to join a workspace {workspace.Name} by {currentUser.Information.FirstName} {currentUser.Information.LastName}",
                    Type = (short)NOTIFICATION_TYPE.WORKSPACE_INVITATION,
                    TimeToSend = DateTime.UtcNow,
                    Status = (short)NOTIFICATION_STATUS.PENDING,
                    Data = JsonConvert.SerializeObject(
                        new Dictionary<string, string>
                        {
                            { "Type", ((short)NOTIFICATION_TYPE.WORKSPACE_INVITATION).ToString() },
                            {
                                "Detail",
                                JsonConvert.SerializeObject(
                                    new InvitedToNewGroup
                                    {
                                        GroupId = workspace.Id,
                                        GroupName = workspace.Name,
                                        InviterId = currentUserId,
                                        InviterName =
                                            $"{currentUser.Information.FirstName} {currentUser.Information.LastName}",
                                        InviterAvatar = currentUser.Information.Picture,
                                        GroupAvatar = workspace.AvatarUrl ?? CommonConsts.DEFAULT_WORKSPACE_AVATAR,
                                    }
                                )
                            },
                            { "Avatar", $"{currentUser.Information.Picture}" }
                        }
                    ),
                    UserNotifications = new List<UserNotification>()
                };
                workspace.Members ??= new List<WorkspaceMember>();
                foreach (var email in emails)
                {
                    var user = await _unitOfWork.Users.GetUserByEmailAsync(email);
                    if (user is null)
                    {
                            await InviteNewUserToWorkspaceAsync(workspaceId, email);
                    }

                    var member = workspace.Members.FirstOrDefault(
                        x => x.UserId == user.Id && !x.IsDeleted
                    );
                    if (member is not null)
                    {
                        if (member.Status == (short)WORKSPACE_MEMBER_STATUS.INVITED || member.Status == (short)WORKSPACE_MEMBER_STATUS.ACTIVE)
                        {
                            continue;
                        }

                        member.Status = (short)WORKSPACE_MEMBER_STATUS.INVITED;
                    }

                    workspace.Members.Add(
                        new WorkspaceMember { UserId = user.Id, AddBy = currentUserId, Status = (short)WORKSPACE_MEMBER_STATUS.INVITED }
                    );
                    notification.UserNotifications.Add(
                        new UserNotification
                        {
                            UserId = user.Id,
                            Status = (short)NOTIFICATION_STATUS.PENDING,
                            SendAt = DateTimeOffset.UtcNow
                        }
                    );
                }
                await _unitOfWork.Workspaces.UpdateAsync(workspace);
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

                return workspace.Id;
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

        public async Task<Guid> RemoveMemberFromWorkspaceAsync(Guid workspaceId, List<Guid> userIds)
        {
            var method = GetActualAsyncMethodName();
            try
            {
                _logger.LogInformation("[{_className}][{method}] Start", _className, method);
                var currentUserId = Guid.Parse(
                    _currentUser.UserId ?? throw new UnauthorizedException("User is not logged in")
                );
                var currentUser = await _unitOfWork.Users.GetUserByIdAsync(currentUserId);

                var workspace =
                    await _unitOfWork.Workspaces.GetAsync(workspaceId) ?? throw new NotFoundException<Workspace>(workspaceId.ToString());
                if (!await _unitOfWork.Workspaces.CheckIsMemberAsync(workspaceId, currentUserId))
                {
                    throw new ForbidException();
                }
                var notification = new Notification
                {
                    Title = "You have been removed from a workspace",
                    Content =
                        $"You have been removed from a workspace {workspace.Name} by {currentUser.Information.FirstName} {currentUser.Information.LastName}",
                    Type = (short)NOTIFICATION_TYPE.WORKSPACE_REMOVED,
                    Status = (short)NOTIFICATION_STATUS.PENDING,
                    TimeToSend = DateTime.UtcNow,
                    Data = JsonConvert.SerializeObject(
                        new Dictionary<string, string>
                        {
                            { "Type", ((short)NOTIFICATION_TYPE.WORKSPACE_REMOVED).ToString() },
                            {
                                "Detail",
                                JsonConvert.SerializeObject(
                                    new RemovedFromGroup
                                    {
                                        GroupId = workspace.Id,
                                        GroupName = workspace.Name,
                                        RemoverId = currentUserId,
                                        RemoverName =
                                            $"{currentUser.Information.FirstName} {currentUser.Information.LastName}",
                                        RemoverAvatar = currentUser.Information.Picture,
                                        GroupAvatar = workspace.AvatarUrl ?? CommonConsts.DEFAULT_WORKSPACE_AVATAR,
                                    }
                                )
                            },
                            { "Avatar", $"{currentUser.Information.Picture}" }
                        }
                    ),
                    UserNotifications = new List<UserNotification>()
                };
                foreach (var userId in userIds)
                {
                    var member =
                        workspace.Members.FirstOrDefault(x => x.UserId == userId)
                        ?? throw new BadRequestException(
                            $"User {userId} is not a member of this workspace"
                        );
                    // await _unitOfWork.WorkspaceMembers.DeleteAsync(member);
                    member.Status = (short)WORKSPACE_MEMBER_STATUS.REMOVED;

                    var channels = await _unitOfWork.Channels
                        .Queryable()
                        .Include(c => c.ChannelMembers.Where(m => !m.IsDeleted))
                        .Where(x => x.WorkspaceId == workspaceId)
                        .ToListAsync();
                    foreach (var channel in channels)
                    {
                        var channelMember = channel.ChannelMembers.FirstOrDefault(
                            x => x.UserId == userId
                        );
                        if (channelMember is not null)
                        {
                            channelMember.Status = (short)CHANNEL_MEMBER_STATUS.REMOVED;
                        }
                    }
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

                return workspace.Id;
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

        public async Task<IEnumerable<WorkspaceRoleDto>> GetRolesAsync(Guid workspaceId)
        {
            var method = GetActualAsyncMethodName();
            var userId = Guid.Parse(_currentUser.UserId ?? throw new UnauthorizedAccessException());
            _logger.LogInformation("[{_className}][{method}] Start", _className, method);
            var isMember = await _unitOfWork.Workspaces.CheckIsMemberAsync(workspaceId, userId);
            if (!isMember)
            {
                throw new ForbidException();
            }

            _logger.LogInformation("[{_className}][{method}] End", _className, method);

            return _mapper.Map<IEnumerable<WorkspaceRoleDto>>(
                await _unitOfWork.Workspaces.GetRoles(workspaceId)
            );
        }

        public async Task<WorkspaceRoleDto> GetRoleAsync(Guid workspaceId, Guid roleId)
        {
            var method = GetActualAsyncMethodName();
            var userId = Guid.Parse(_currentUser.UserId ?? throw new UnauthorizedAccessException());
            _logger.LogInformation("[{_className}][{method}] Start", _className, method);
            var isMember = await _unitOfWork.Workspaces.CheckIsMemberAsync(workspaceId, userId);
            if (!isMember)
            {
                throw new ForbidException();
            }

            var isExistRole = await _unitOfWork.Workspaces.CheckIsExistRole(workspaceId, roleId);
            if (!isExistRole)
            {
                throw new NotFoundException<WorkspaceRole>(roleId.ToString());
            }

            var role = await _unitOfWork.Workspaces.GetRoleById(workspaceId, roleId);
            var roleDto = _mapper.Map<WorkspaceRoleDto>(role);
            roleDto.Permissions.ForEach(x => x.IsEnabled = true);

            var activePermissions = _unitOfWork.WorkspacePermissions
                .Queryable()
                .Where(x => !x.IsDeleted && x.IsActive);

            roleDto.Permissions.AddRange(
                _mapper.Map<List<PermissionDto>>(
                    activePermissions.Where(
                        x => !roleDto.Permissions.Select(x => x.Id).Contains(x.Id)
                    )
                )
            );

            _logger.LogInformation("[{_className}][{method}] End", _className, method);

            return roleDto;
        }

        public async Task<Guid> AddRoleAsync(Guid workspaceId, CreateUpdateWorkspaceRoleDto input)
        {
            var method = GetActualAsyncMethodName();
            _logger.LogInformation("[{_className}][{method}] Start", _className, method);
            var userId = Guid.Parse(_currentUser.UserId ?? throw new UnauthorizedAccessException());
            var isMember = await _unitOfWork.Workspaces.CheckIsMemberAsync(workspaceId, userId);
            if (!isMember)
            {
                throw new ForbidException();
            }

            var role = _mapper.Map<WorkspaceRole>(input);
            role = await _unitOfWork.Workspaces.AddRoleAsync(workspaceId, role);

            await _unitOfWork.SaveChangeAsync();

            _logger.LogInformation("[{_className}][{method}] End", _className, method);

            return role.Id;
        }

        public async Task UpdateRoleAsync(
            Guid workspaceId,
            Guid roleId,
            CreateUpdateWorkspaceRoleDto input
        )
        {
            var method = GetActualAsyncMethodName();
            _logger.LogInformation("[{_className}][{method}] Start", _className, method);
            var userId = Guid.Parse(_currentUser.UserId ?? throw new UnauthorizedAccessException());
            var isMember = await _unitOfWork.Workspaces.CheckIsMemberAsync(workspaceId, userId);
            if (!isMember)
            {
                throw new ForbidException();
            }
            var isExistRole = await _unitOfWork.Workspaces.CheckIsExistRole(workspaceId, roleId);
            if (!isExistRole)
            {
                throw new NotFoundException<WorkspaceRole>(roleId.ToString());
            }

            var role = await _unitOfWork.Workspaces.GetRoleById(workspaceId, roleId);
            foreach (var permission in role.Permissions)
            {
                await _unitOfWork.PermissionsOfWorkspaceRoles.DeleteAsync(
                    permission,
                    isHardDelete: true
                );
            }
            _mapper.Map(input, role);
            await _unitOfWork.WorkspaceRoles.UpdateAsync(role);
            await _unitOfWork.SaveChangeAsync();

            _logger.LogInformation("[{_className}][{method}] End", _className, method);
        }

        public async Task<IEnumerable<PermissionDto>> GetPermissionsByWorkspaceRoleIdAsync(
            Guid workspaceId,
            Guid roleId
        )
        {
            var method = GetActualAsyncMethodName();
            _logger.LogInformation("[{_className}][{method}] Start", _className, method);
            var isMember = await _unitOfWork.Workspaces.CheckIsMemberAsync(
                workspaceId,
                Guid.Parse(_currentUser.UserId ?? throw new UnauthorizedAccessException())
            );
            var isExistRole = await _unitOfWork.Workspaces.CheckIsExistRole(workspaceId, roleId);
            if (!isExistRole)
            {
                throw new NotFoundException<WorkspaceRole>(roleId.ToString());
            }
            var workspaceRole = await _unitOfWork.Workspaces.GetRoleById(workspaceId, roleId);

            var permissions = workspaceRole.Permissions
                .Where(x => !x.IsDeleted && x.Permission.IsActive)
                .ToList();
            var permissionDtos = _mapper.Map<List<PermissionDto>>(permissions);
            permissionDtos.ForEach(x => x.IsEnabled = true);

            var activePermissions = _unitOfWork.WorkspacePermissions
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

        public async Task DeleteRoleAsync(Guid workspaceId, Guid roleId)
        {
            var method = GetActualAsyncMethodName();
            _logger.LogInformation("[{_className}][{method}] Start", _className, method);
            var isMember = await _unitOfWork.Workspaces.CheckIsMemberAsync(
                workspaceId,
                Guid.Parse(_currentUser.UserId ?? throw new UnauthorizedAccessException())
            );
            if (!isMember)
            {
                throw new ForbidException();
            }
            var isExistRole = await _unitOfWork.Workspaces.CheckIsExistRole(workspaceId, roleId);
            if (!isExistRole)
            {
                throw new NotFoundException<WorkspaceRole>(roleId.ToString());
            }
            var workspaceRole = await _unitOfWork.Workspaces.GetRoleById(workspaceId, roleId);
            foreach (var permission in workspaceRole.Permissions)
            {
                await _unitOfWork.PermissionsOfWorkspaceRoles.DeleteAsync(
                    permission,
                    isHardDelete: true
                );
            }
            await _unitOfWork.WorkspaceRoles.DeleteAsync(workspaceRole, isHardDelete: true);
            await _unitOfWork.SaveChangeAsync();
            _logger.LogInformation("[{_className}][{method}] End", _className, method);
        }

        public async Task SetRoleAsync(Guid workspaceId, Guid userId, Guid? roleId)
        {
            var method = GetActualAsyncMethodName();
            _logger.LogInformation("[{_className}][{method}] Start", _className, method);
            var isMember = await _unitOfWork.Workspaces.CheckIsMemberAsync(
                workspaceId,
                Guid.Parse(_currentUser.UserId ?? throw new UnauthorizedAccessException())
            );
            if (!isMember)
            {
                throw new ForbidException();
            }
            var isExistRole =
                roleId is null
                || await _unitOfWork.Workspaces.CheckIsExistRole(workspaceId, roleId.Value);
            if (!isExistRole)
            {
                throw new NotFoundException<WorkspaceRole>(roleId.ToString());
            }
            isMember = await _unitOfWork.Workspaces.CheckIsMemberAsync(workspaceId, userId);
            if (!isMember)
            {
                throw new NotFoundException<WorkspaceMember>(userId.ToString());
            }
            var member = await _unitOfWork.Workspaces.GetMemberByUserId(workspaceId, userId);
            member.RoleId = roleId;
            await _unitOfWork.WorkspaceMembers.UpdateAsync(member);
            await _unitOfWork.SaveChangeAsync();
            _logger.LogInformation("[{_className}][{method}] End", _className, method);
        }

        public async Task<IEnumerable<PermissionDto>> GetPermissions()
        {
            var method = GetActualAsyncMethodName();
            _logger.LogInformation("[{_className}][{method}] Start", _className, method);

            var permissions = _unitOfWork.WorkspacePermissions
                .Queryable()
                .Where(x => !x.IsDeleted && x.IsActive);
            await Task.CompletedTask;
            _logger.LogInformation("[{_className}][{method}] End", _className, method);

            return _mapper.Map<IEnumerable<PermissionDto>>(permissions);
        }

        public async Task<bool> IsExistAsync(Guid workspaceId)
        {
            var method = GetActualAsyncMethodName();
            _logger.LogInformation("[{_className}][{method}] Start", _className, method);
            var isExist = await _unitOfWork.Workspaces.CheckIsExistAsync(workspaceId);
            _logger.LogInformation("[{_className}][{method}] End", _className, method);
            return isExist;
        }

        public async Task<IEnumerable<PermissionDto>> GetPermissionOfUser(
            Guid workspaceId,
            Guid userId
        )
        {
            var method = GetActualAsyncMethodName();
            _logger.LogInformation("[{_className}][{method}] Start", _className, method);
            var isExist = await _unitOfWork.Workspaces.CheckIsExistAsync(workspaceId);
            if (!isExist)
            {
                throw new NotFoundException<Workspace>(workspaceId.ToString());
            }
            var isMember = await _unitOfWork.Workspaces.CheckIsMemberAsync(workspaceId, userId);
            if (!isMember)
            {
                throw new ForbidException();
            }
            var isOwner = await _unitOfWork.Workspaces.CheckIsOwnerAsync(workspaceId, userId);

            var permissions = await _unitOfWork.WorkspaceMembers.GetPermissionOfUser(
                workspaceId,
                userId
            );
            if (isOwner)
            {
                permissions = permissions.Concat(
                    await _unitOfWork.WorkspacePermissions
                        .Queryable()
                        .Where(x => !x.IsDeleted && x.IsActive)
                        .ToListAsync()
                );
            }
            _logger.LogInformation("[{_className}][{method}] End", _className, method);

            return _mapper.Map<IEnumerable<PermissionDto>>(permissions);
        }

        public async Task<IEnumerable<UserDetailDto>> GetMembersByRoleIdAsync(
            Guid workspaceId,
            Guid roleId
        )
        {
            var method = GetActualAsyncMethodName();
            _logger.LogInformation("[{_className}][{method}] Start", _className, method);
            var isRoleExist = await _unitOfWork.Workspaces.CheckIsExistRole(workspaceId, roleId);
            var currentUserId = Guid.Parse(
                _currentUser.UserId ?? throw new UnauthorizedAccessException()
            );

            if (!isRoleExist)
            {
                throw new NotFoundException<WorkspaceRole>(roleId.ToString());
            }

            var isMember = await _unitOfWork.Workspaces.CheckIsMemberAsync(
                workspaceId,
                currentUserId
            );
            if (!isMember)
            {
                throw new ForbidException();
            }

            var members = await _unitOfWork.WorkspaceMembers
                .Queryable()
                .Include(x => x.User)
                .ThenInclude(x => x.Information)
                .Where(x => x.WorkspaceId == workspaceId && x.RoleId == roleId)
                .ToListAsync();

            _logger.LogInformation("[{_className}][{method}] End", _className, method);

            return _mapper.Map<IEnumerable<UserDetailDto>>(members.Select(x => x.User));
        }

        public async Task<IEnumerable<UserDetailDto>> GetMembersWithoutRoleAsync(Guid workspaceId)
        {
            var method = GetActualAsyncMethodName();
            _logger.LogInformation("[{_className}][{method}] Start", _className, method);
            var isWorkspaceExist = await _unitOfWork.Workspaces.CheckIsExistAsync(workspaceId);
            var currentUserId = Guid.Parse(
                _currentUser.UserId ?? throw new UnauthorizedAccessException()
            );
            if (!isWorkspaceExist)
            {
                throw new NotFoundException<Workspace>(workspaceId.ToString());
            }

            var isMember = await _unitOfWork.Workspaces.CheckIsMemberAsync(
                workspaceId,
                currentUserId
            );
            if (!isMember)
            {
                throw new ForbidException();
            }

            var members = await _unitOfWork.WorkspaceMembers
                .Queryable()
                .Include(x => x.User)
                .ThenInclude(x => x.Information)
                .Where(x => x.WorkspaceId == workspaceId && x.RoleId == null)
                .ToListAsync();

            _logger.LogInformation("[{_className}][{method}] End", _className, method);

            return _mapper.Map<IEnumerable<UserDetailDto>>(members.Select(x => x.User));
        }

        public async Task<IEnumerable<WorkspaceUserDto>> GetMembersAsync(Guid workspaceId)
        {
            var method = GetActualAsyncMethodName();
            _logger.LogInformation("[{_className}][{method}] Start", _className, method);
            var isWorkspaceExist = await _unitOfWork.Workspaces.CheckIsExistAsync(workspaceId);
            var currentUserId = Guid.Parse(
                _currentUser.UserId ?? throw new UnauthorizedAccessException()
            );
            if (!isWorkspaceExist)
            {
                throw new NotFoundException<Workspace>(workspaceId.ToString());
            }

            var isMember = await _unitOfWork.Workspaces.CheckIsMemberAsync(
                workspaceId,
                currentUserId
            );
            if (!isMember)
            {
                throw new ForbidException();
            }

            var members = await _unitOfWork.WorkspaceMembers
                .Queryable()
                .Include(x => x.User)
                .ThenInclude(x => x.Information)
                .Include(x => x.WorkspaceRole)
                .Where(x => x.WorkspaceId == workspaceId)
                .ToListAsync();

            _logger.LogInformation("[{_className}][{method}] End", _className, method);

            return _mapper.Map<IEnumerable<WorkspaceUserDto>>(members);
        }

        public async Task<IEnumerable<AdminWorkspaceDto>> GetAllForAdminAsync()
        {
            var method = GetActualAsyncMethodName();
            _logger.LogInformation("[{_className}][{method}] Start", _className, method);

            var workspaces = await _unitOfWork.Workspaces
                .Queryable()
                .Include(x => x.Owner)
                .ThenInclude(o => o.Information)
                .ToListAsync();

            _logger.LogInformation("[{_className}][{method}] End", _className, method);

            return _mapper.Map<IEnumerable<AdminWorkspaceDto>>(workspaces);
        }

        public async Task<Guid> UpdateWorkspaceStatusAsync(Guid workspaceId, short status)
        {
            var method = GetActualAsyncMethodName();
            try
            {
                _logger.LogInformation("[{_className}][{method}] Start", _className, method);

                var workspace = await _unitOfWork.Workspaces.Queryable()
                    .FirstOrDefaultAsync(x => x.Id == workspaceId);
                workspace.Status = status switch
                {
                    (short)WORKSPACE_STATUS.SUSPENDED => (short)WORKSPACE_STATUS.SUSPENDED,
                    (short)WORKSPACE_STATUS.ACTIVE => (short)WORKSPACE_STATUS.ACTIVE,
                    _ => throw new BadRequestException("Status is not valid"),
                };
                await _unitOfWork.Workspaces.UpdateAsync(workspace);
                await _unitOfWork.SaveChangeAsync();

                _logger.LogInformation("[{_className}][{method}] End", _className, method);
                return workspace.Id;
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

        public async Task AcceptInvitationAsync(Guid workspaceId)
        {
            var method = GetActualAsyncMethodName();
            _logger.LogInformation("[{_className}][{method}] Start", _className, method);
            var userId = Guid.Parse(_currentUser.UserId ?? throw new UnauthorizedAccessException());
            var isInvited = await _unitOfWork.Workspaces.CheckIsInvitedAsync(workspaceId, userId);
            if (!isInvited)
            {
                throw new BadRequestException("You are not invited to this workspace");
            }
            var member = await _unitOfWork.Workspaces.GetMemberByUserId(workspaceId, userId);
            member.Status = (short)WORKSPACE_MEMBER_STATUS.ACTIVE;
            await _unitOfWork.WorkspaceMembers.UpdateAsync(member);
            await _unitOfWork.SaveChangeAsync();
            _logger.LogInformation("[{_className}][{method}] End", _className, method);
        }

        public async Task DeclineInvitationAsync(Guid workspaceId)
        {
            var method = GetActualAsyncMethodName();
            _logger.LogInformation("[{_className}][{method}] Start", _className, method);
            var userId = Guid.Parse(_currentUser.UserId ?? throw new UnauthorizedAccessException());
            var isInvited = await _unitOfWork.Workspaces.CheckIsInvitedAsync(workspaceId, userId);
            if (!isInvited)
            {
                throw new BadRequestException("You are not invited to this workspace");
            }
            var member = await _unitOfWork.Workspaces.GetMemberByUserId(workspaceId, userId);
            member.Status = (short)WORKSPACE_MEMBER_STATUS.DECLINED;
            await _unitOfWork.WorkspaceMembers.UpdateAsync(member);
            await _unitOfWork.SaveChangeAsync();
            _logger.LogInformation("[{_className}][{method}] End", _className, method);
        }

        private async Task InviteNewUserToWorkspaceAsync(Guid workspaceId, string email)
        {
            try
            {
                var workspace = await _unitOfWork.Workspaces.GetAsync(workspaceId);
                var currentUserId = Guid.Parse(
                    _currentUser.UserId ?? throw new UnauthorizedAccessException()
                );

                var mailData = new MailData 
                {
                    JsonData = JsonConvert.SerializeObject(
                                    new InvitedToNewGroup
                                    {
                                        GroupId = workspace.Id,
                                        GroupName = workspace.Name,
                                        InviterId = currentUserId,
                                        InviterName = currentUserId.ToString(),
                                        InviterAvatar = string.Empty,
                                        GroupAvatar = workspace.AvatarUrl ?? CommonConsts.DEFAULT_WORKSPACE_AVATAR,
                                    }
                                )
                };

                _backgroundJobClient.Enqueue(
                    () => _mailService.Send(
                            email,
                            "You have been invited to join a workspace",
                            MailConst.InviteToWorkspace.Template,
                            JsonConvert.SerializeObject(
                                new InvitedToNewGroup
                                {
                                    GroupId = workspace.Id,
                                    GroupName = workspace.Name,
                                    InviterId = currentUserId,
                                    InviterName = currentUserId.ToString(),
                                    InviterAvatar = string.Empty,
                                    GroupAvatar = workspace.AvatarUrl ?? CommonConsts.DEFAULT_WORKSPACE_AVATAR,
                                }
                            )
                        )
                );
            }
            catch (Exception e)
            {
                _logger.LogInformation(
                    "[{_className}][{method}] Error: {message}",
                    _className,
                    GetActualAsyncMethodName(),
                    e.Message
                );
            }
        }

        public async Task<Guid> LeaveWorkspaceAsync(Guid workspaceId)
        {
            var method = GetActualAsyncMethodName();
            try
            {
                _logger.LogInformation("[{_className}][{method}] Start", _className, method);

                var userId = Guid.Parse(
                    _currentUser.UserId ?? throw new UnauthorizedAccessException()
                );
                var workspace = await _unitOfWork.Workspaces.GetAsync(workspaceId);
                if (workspace is null)
                    throw new NotFoundException<Workspace>(workspaceId.ToString());

                var member = await _unitOfWork.Workspaces.GetMemberByUserId(workspaceId, userId);
                if (member is null)
                    throw new NotFoundException<WorkspaceMember>(userId.ToString());

                var channels = await _unitOfWork.Channels
                    .Queryable()
                    .Include(x => x.ChannelMembers)
                    .Where(x => x.WorkspaceId == workspaceId)
                    .ToListAsync();
                foreach (var channel in channels)
                {
                    var channelMember = channel.ChannelMembers.FirstOrDefault(x => x.UserId == userId);
                    if (channelMember is not null)
                    {
                        channelMember.Status = (short)CHANNEL_MEMBER_STATUS.REMOVED;
                        await _unitOfWork.ChannelMembers.DeleteAsync(channelMember);   
                    }
                }
                member.Status = (short)WORKSPACE_MEMBER_STATUS.REMOVED;
                await _unitOfWork.WorkspaceMembers.DeleteAsync(member);
                await _unitOfWork.SaveChangeAsync();

                _logger.LogInformation("[{_className}][{method}] End", _className, method);

                return workspace.Id;
            }
            catch (Exception e)
            {
                _logger.LogInformation(
                    "[{_className}][{method}] Error: {message}",
                    _className,
                    GetActualAsyncMethodName(),
                    e.Message
                );

                throw;
            }
        }

        public async Task<WorkspaceDto> GetByIdForAdminAsync(Guid workspaceId)
        {
            var method = GetActualAsyncMethodName();
            try
            {
                _logger.LogInformation("[{_className}][{method}] Start", _className, method);

                var workspace = await _unitOfWork.Workspaces.GetAsync(workspaceId);

                var userId = Guid.Parse(
                    _currentUser.UserId ?? throw new UnauthorizedException("User is not logged in")
                );

                _logger.LogInformation("[{_className}][{method}] End", _className, method);
                return _mapper.Map<WorkspaceDto>(workspace);
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
    }
}
