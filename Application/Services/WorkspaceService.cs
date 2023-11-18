using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PBL6.Application.Contract.Channels;
using PBL6.Application.Contract.Workspaces;
using PBL6.Application.Contract.Workspaces.Dtos;
using PBL6.Common.Consts;
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
                var userId = Guid.Parse(_currentUser.UserId ?? throw new UnauthorizedAccessException());

                if (workspaceDto.Avatar is not null)
                {
                    var imageUrl = await _fileService.UploadImageToImgbb(
                        workspaceDto.Avatar,
                        workspace.Id
                    );
                    workspace.AvatarUrl = imageUrl;
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

                workspace = await _unitOfwork.Workspaces.AddAsync(workspace);
                await _unitOfwork.SaveChangeAsync();
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
                var workspace = await _unitOfwork.Workspaces.Queryable().Include(x => x.Members.Where(m => !m.IsDeleted)).FirstOrDefaultAsync(x => x.Id == workspaceId);
                if (workspace is null)
                    throw new NotFoundException<Workspace>(workspaceId.ToString());

                var currentUserId = Guid.Parse(_currentUser.UserId ?? throw new UnauthorizedAccessException());

                if (!await _unitOfwork.Workspaces.CheckIsMemberAsync(workspaceId, currentUserId))
                    throw new ForbidException();


                await _unitOfwork.Workspaces.DeleteAsync(workspace);
                await _unitOfwork.SaveChangeAsync();
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
                var workspaces = await _unitOfwork.Workspaces
                    .Queryable()
                    .Include(x => x.Channels.Where(c => !c.IsDeleted))
                    .Include(x => x.Members.AsQueryable().Include(m => m.User).ThenInclude(u => u.Information).Where(m => !m.IsDeleted))
                    .ToListAsync();

                var userId = Guid.Parse(_currentUser.UserId ?? throw new Exception());
                workspaces = workspaces.Where(x => x.Members.Any(m => m.UserId == userId)).ToList();

                foreach (var workspace in workspaces)
                {
                    var channels = await _channelService.GetAllChannelsOfAWorkspaceAsync(workspace.Id);
                    foreach (var channel in channels)
                    {
                        if (!channel.ChannelMembers.Any(x => x == userId))
                        {
                            workspace.Channels = workspace.Channels.Where(x => x.Id != channel.Id).ToList();
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
                var workspace = await _unitOfwork.Workspaces
                    .Queryable()
                    .Include(x => x.Channels.Where(c => !c.IsDeleted))
                    .Include(x => x.Members.Where(m => !m.IsDeleted))
                    .FirstOrDefaultAsync(x => x.Id == workspaceId);

                if (workspace is null)
                    throw new NotFoundException<Workspace>(workspaceId.ToString());
                var userId = Guid.Parse(_currentUser.UserId ?? throw new Exception());

                if (!await _unitOfwork.Workspaces.CheckIsMemberAsync(workspaceId, userId))
                    throw new ForbidException();

                var channels = await _channelService.GetAllChannelsOfAWorkspaceAsync(workspace.Id);
                foreach (var channel in channels)
                {
                    if (!channel.ChannelMembers.Any(x => x == userId))
                    {
                        workspace.Channels = workspace.Channels.Where(x => x.Id != channel.Id).ToList();
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
                var workspaces = await _unitOfwork.Workspaces
                    .Queryable()
                    .Include(x => x.Channels.Where(c => !c.IsDeleted))
                    .Include(x => x.Members.Where(m => !m.IsDeleted))
                    .Where(x => x.Name.Contains(workspaceName))
                    .ToListAsync();

                var userId = Guid.Parse(_currentUser.UserId ?? throw new Exception());
                workspaces = workspaces.Where(x => x.Members.Any(m => m.UserId == userId)).ToList();

                foreach (var workspace in workspaces)
                {
                    var channels = await _channelService.GetAllChannelsOfAWorkspaceAsync(workspace.Id);
                    foreach (var channel in channels)
                    {
                        if (!channel.ChannelMembers.Any(x => x == userId))
                        {
                            workspace.Channels = workspace.Channels.Where(x => x.Id != channel.Id).ToList();
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
                var userId = Guid.Parse(_currentUser.UserId ?? throw new Exception());
                var workspace = await _unitOfwork.Workspaces.Queryable().Include(x => x.Members.Where(m => !m.IsDeleted)).FirstOrDefaultAsync(x => x.Id == workspaceId);

                if (workspace is null)
                    throw new NotFoundException<Workspace>(workspaceId.ToString());

                if (!await _unitOfwork.Workspaces.CheckIsMemberAsync(workspaceId, userId))
                    throw new ForbidException();

                if (updateWorkspaceDto.Description is null)
                {
                    updateWorkspaceDto.Description = string.Empty;
                }

                _mapper.Map(updateWorkspaceDto, workspace);
                await _unitOfwork.Workspaces.UpdateAsync(workspace);
                await _unitOfwork.SaveChangeAsync();

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
                var userId = Guid.Parse(_currentUser.UserId ?? throw new Exception());
                var workspace = await _unitOfwork.Workspaces.Queryable().Include(x => x.Members.Where(m => !m.IsDeleted)).FirstOrDefaultAsync(x => x.Id == workspaceId);

                if (workspace is null)
                    throw new NotFoundException<Workspace>(workspaceId.ToString());

                if (!await _unitOfwork.Workspaces.CheckIsMemberAsync(workspaceId, userId))
                    throw new ForbidException();

                if (updateAvatarWorkspaceDto.Avatar is not null)
                {
                    workspace.AvatarUrl = await _fileService.UploadImageToImgbb(
                        updateAvatarWorkspaceDto.Avatar,
                        workspace.Id
                    );
                }
                else
                {
                    workspace.AvatarUrl = CommonConsts.DEFAULT_WORKSPACE_AVATAR;
                }
                await _unitOfwork.Workspaces.UpdateAsync(workspace);
                await _unitOfwork.SaveChangeAsync();

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

        public async Task<Guid> AddMemberToWorkspaceAsync(Guid workspaceId, Guid userId)
        {
            var method = GetActualAsyncMethodName();
            try
            {
                _logger.LogInformation("[{_className}][{method}] Start", _className, method);
                var currentUserId = Guid.Parse(_currentUser.UserId ?? throw new Exception());
                var workspace = await _unitOfwork.Workspaces
                    .Queryable()
                    .Include(x => x.Members.Where(m => !m.IsDeleted))
                    .FirstOrDefaultAsync(x => x.Id == workspaceId);

                if (workspace is null)
                {
                    throw new NotFoundException<Workspace>(workspaceId.ToString());
                }

                if (!await _unitOfwork.Workspaces.CheckIsMemberAsync(workspaceId, currentUserId))
                {
                    throw new ForbidException();
                }

                var user = await _unitOfwork.Users.FindAsync(userId);
                if (user is null)
                {
                    throw new NotFoundException<User>(userId.ToString());
                }

                if (!user.IsActive)
                {
                    throw new Exception("User is not active yet");
                }

                var member = workspace.Members.FirstOrDefault(x => x.UserId == userId);
                if (member is not null)
                {
                    throw new Exception("User is already a member of this workspace");
                }

                workspace.Members.Add(
                    new WorkspaceMember { UserId = userId, AddBy = currentUserId }
                );

                await _unitOfwork.Workspaces.UpdateAsync(workspace);
                await _unitOfwork.SaveChangeAsync();

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

        public async Task<Guid> RemoveMemberFromWorkspaceAsync(Guid workspaceId, Guid userId)
        {
            var method = GetActualAsyncMethodName();
            try
            {
                _logger.LogInformation("[{_className}][{method}] Start", _className, method);

                var workspace = await _unitOfwork.Workspaces
                    .Queryable()
                    .Include(x => x.Members.Where(m => !m.IsDeleted))
                    .FirstOrDefaultAsync(x => x.Id == workspaceId);

                if (workspace is null)
                {
                    throw new NotFoundException<Workspace>(workspaceId.ToString());
                }

                var currentUserId = Guid.Parse(_currentUser.UserId ?? throw new Exception());

                if (!await _unitOfwork.Workspaces.CheckIsMemberAsync(workspaceId, currentUserId))
                {
                    throw new ForbidException();
                }

                var member = workspace.Members.FirstOrDefault(x => x.UserId == userId);
                if (member is null)
                {
                    throw new Exception("User is not a member of this workspace");
                }

                await _unitOfwork.WorkspaceMembers.DeleteAsync(member);

                var channels = await _unitOfwork.Channels
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
                        await _unitOfwork.ChannelMembers.DeleteAsync(channelMember);
                    }
                }

                await _unitOfwork.SaveChangeAsync();

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
            var isMember = await _unitOfwork.Workspaces.CheckIsMemberAsync(workspaceId, userId);
            if (!isMember)
            {
                throw new ForbidException();
            }

            _logger.LogInformation("[{_className}][{method}] End", _className, method);

            return _mapper.Map<IEnumerable<WorkspaceRoleDto>>(
                await _unitOfwork.Workspaces.GetRoles(workspaceId)
            );
        }

        public async Task<Guid> AddRoleAsync(Guid workspaceId, CreateUpdateWorkspaceRoleDto input)
        {
            var method = GetActualAsyncMethodName();
            _logger.LogInformation("[{_className}][{method}] Start", _className, method);
            var userId = Guid.Parse(_currentUser.UserId ?? throw new UnauthorizedAccessException());
            var isMember = await _unitOfwork.Workspaces.CheckIsMemberAsync(workspaceId, userId);
            if (!isMember)
            {
                throw new ForbidException();
            }

            var role = _mapper.Map<WorkspaceRole>(input);
            role = await _unitOfwork.Workspaces.AddRoleAsync(workspaceId, role);

            await _unitOfwork.SaveChangeAsync();

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
            var isMember = await _unitOfwork.Workspaces.CheckIsMemberAsync(workspaceId, userId);
            if (!isMember)
            {
                throw new ForbidException();
            }
            var isExistRole = await _unitOfwork.Workspaces.CheckIsExistRole(workspaceId, roleId);
            if (!isExistRole)
            {
                throw new NotFoundException<WorkspaceRole>(roleId.ToString());
            }

            var role = await _unitOfwork.Workspaces.GetRoleById(workspaceId, roleId);
            _mapper.Map(input, role);
            await _unitOfwork.WorkspaceRoles.UpdateAsync(role);
            await _unitOfwork.SaveChangeAsync();

            _logger.LogInformation("[{_className}][{method}] End", _className, method);
        }

        public async Task<IEnumerable<PermissionDto>> GetPermissionsByWorkspaceRoleIdAsync(
            Guid workspaceId,
            Guid roleId
        )
        {
            var method = GetActualAsyncMethodName();
            _logger.LogInformation("[{_className}][{method}] Start", _className, method);
            var isMember = await _unitOfwork.Workspaces.CheckIsMemberAsync(
                workspaceId,
                Guid.Parse(_currentUser.UserId ?? throw new UnauthorizedAccessException())
            );
            var isExistRole = await _unitOfwork.Workspaces.CheckIsExistRole(workspaceId, roleId);
            if (!isExistRole)
            {
                throw new NotFoundException<WorkspaceRole>(roleId.ToString());
            }
            var workspaceRole = await _unitOfwork.Workspaces.GetRoleById(workspaceId, roleId);

            var permissions = workspaceRole.Permissions
                .Where(x => !x.IsDeleted && x.Permission.IsActive)
                .ToList();
            var permissionDtos = _mapper.Map<List<PermissionDto>>(permissions);
            permissionDtos.ForEach(x => x.IsEnabled = true);

            var activePermissions = _unitOfwork.WorkspacePermissions
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
            var isMember = await _unitOfwork.Workspaces.CheckIsMemberAsync(
                workspaceId,
                Guid.Parse(_currentUser.UserId ?? throw new UnauthorizedAccessException())
            );
            if (!isMember)
            {
                throw new ForbidException();
            }
            var isExistRole = await _unitOfwork.Workspaces.CheckIsExistRole(workspaceId, roleId);
            if (!isExistRole)
            {
                throw new NotFoundException<WorkspaceRole>(roleId.ToString());
            }
            var workspaceRole = await _unitOfwork.Workspaces.GetRoleById(workspaceId, roleId);
            await _unitOfwork.WorkspaceRoles.DeleteAsync(workspaceRole, isHardDelete: true);
            await _unitOfwork.SaveChangeAsync();
            _logger.LogInformation("[{_className}][{method}] End", _className, method);
        }

        public async Task SetRoleAsync(Guid workspaceId, Guid userId, Guid roleId)
        {
            var method = GetActualAsyncMethodName();
            _logger.LogInformation("[{_className}][{method}] Start", _className, method);
            var isMember = await _unitOfwork.Workspaces.CheckIsMemberAsync(
                workspaceId,
                Guid.Parse(_currentUser.UserId ?? throw new UnauthorizedAccessException())
            );
            if (!isMember)
            {
                throw new ForbidException();
            }
            var isExistRole = await _unitOfwork.Workspaces.CheckIsExistRole(workspaceId, roleId);
            if (!isExistRole)
            {
                throw new NotFoundException<WorkspaceRole>(roleId.ToString());
            }
            isMember = await _unitOfwork.Workspaces.CheckIsMemberAsync(workspaceId, userId);
            if (!isMember)
            {
                throw new NotFoundException<WorkspaceMember>(userId.ToString());
            }
            var member = await _unitOfwork.Workspaces.GetMemberByUserId(workspaceId, userId);
            member.RoleId = roleId;
            await _unitOfwork.WorkspaceMembers.UpdateAsync(member);
            await _unitOfwork.SaveChangeAsync();
            _logger.LogInformation("[{_className}][{method}] End", _className, method);
        }

        public async Task<IEnumerable<PermissionDto>> GetPermissions()
        {
            var method = GetActualAsyncMethodName();
            _logger.LogInformation("[{_className}][{method}] Start", _className, method);

            var permissions = _unitOfwork.WorkspacePermissions
                .Queryable()
                .Where(x => !x.IsDeleted && x.IsActive);
            await Task.CompletedTask;
            _logger.LogInformation("[{_className}][{method}] End", _className, method);

            return _mapper.Map<IEnumerable<PermissionDto>>(permissions);
        }
    }
}
