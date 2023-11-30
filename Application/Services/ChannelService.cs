using System.Runtime.CompilerServices;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PBL6.Application.Contract.Channels;
using PBL6.Application.Contract.Channels.Dtos;
using PBL6.Application.Contract.Workspaces.Dtos;
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
            var currentUserId = Guid.Parse(_currentUser.UserId ?? throw new Exception());

            var workspace = await _unitOfwork.Workspaces.Queryable()
                .Include(x => x.Members.Where(m => !m.IsDeleted))
                .Where(x => x.Id == channel.WorkspaceId)
                .FirstOrDefaultAsync();

            if (workspace is null)
            {
                throw new NotFoundException<Workspace>(channel.WorkspaceId.ToString());
            }

            if (!await _unitOfwork.Workspaces.CheckIsMemberAsync(channel.WorkspaceId, currentUserId))
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
                channel.ChannelMembers.Add(new ChannelMember { UserId = workspace.OwnerId, AddBy = currentUserId });
            }

            channel.CategoryId = Guid.Empty;
            channel = await _unitOfwork.Channels.AddAsync(channel);
            await _unitOfwork.SaveChangeAsync();
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
            var channel = await _unitOfwork.Channels
                .Queryable()
                .Include(x => x.ChannelMembers.Where(c => !c.IsDeleted))
                .Where(x => x.Id == channelId)
                .FirstOrDefaultAsync();
            var currentUserId = Guid.Parse(_currentUser.UserId ?? throw new Exception());
            if (channel is null)
            {
                throw new NotFoundException<Channel>(channelId.ToString());
            }

            if (!await _unitOfwork.Channels.CheckIsMemberAsync(channelId, currentUserId))
            {
                throw new ForbidException();
            }
            foreach (var userId in userIds)
            {
                var workspace = await _unitOfwork.Workspaces
                    .Queryable()
                    .Include(x => x.Members.Where(m => !m.IsDeleted))
                    .Where(w => w.Id == channel.WorkspaceId)
                    .FirstOrDefaultAsync();
                if (!workspace.Members.Any(x => x.UserId == userId))
                {
                    throw new Exception($"User {userId} is not in the workspace of this channel");
                }

                var user = await _unitOfwork.Users.FindAsync(userId);
                if (user is null)
                {
                    throw new NotFoundException<User>(userId.ToString());
                }

                var member = channel.ChannelMembers.FirstOrDefault(x => x.UserId == userId);
                if (member is not null)
                {
                    throw new Exception($"User {userId} is already in this channel");
                }

                var channelMember = new ChannelMember { UserId = userId, AddBy = currentUserId };

                channel.ChannelMembers.Add(channelMember);
            }
            await _unitOfwork.SaveChangeAsync();
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
        var isMember = await _unitOfwork.Channels.CheckIsMemberAsync(channelId, userId);
        if (!isMember)
        {
            throw new ForbidException();
        }

        var role = _mapper.Map<ChannelRole>(input);
        role = await _unitOfwork.Channels.AddRoleAsync(channelId, role);

        await _unitOfwork.SaveChangeAsync();

        _logger.LogInformation("[{_className}][{method}] End", _className, method);

        return role.Id;
    }

    public async Task<Guid> DeleteAsync(Guid channelId)
    {
        var method = GetActualAsyncMethodName();
        try
        {
            _logger.LogInformation("[{_className}][{method}] Start", _className, method);
            var channel = await _unitOfwork.Channels.Queryable()
                .Include(x => x.ChannelMembers.Where(c => !c.IsDeleted))
                .Where(x => x.Id == channelId)
                .FirstOrDefaultAsync();

            if (channel is null)
                throw new NotFoundException<Channel>(channelId.ToString());

            var currentUserId = Guid.Parse(_currentUser.UserId ?? throw new Exception());

            if (!await _unitOfwork.Channels.CheckIsMemberAsync(channelId, currentUserId))
            {
                throw new ForbidException();
            }

            await _unitOfwork.Channels.DeleteAsync(channel);
            await _unitOfwork.SaveChangeAsync();
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
        var isMember = await _unitOfwork.Channels.CheckIsMemberAsync(
            channelId,
            Guid.Parse(_currentUser.UserId ?? throw new UnauthorizedAccessException())
        );
        if (!isMember)
        {
            throw new ForbidException();
        }
        var isExistRole = await _unitOfwork.Channels.CheckIsExistRole(channelId, roleId);
        if (!isExistRole)
        {
            throw new NotFoundException<ChannelRole>(roleId.ToString());
        }
        var channelRole = await _unitOfwork.Channels.GetRoleById(channelId, roleId);
        await _unitOfwork.ChannelRoles.DeleteAsync(channelRole, isHardDelete: true);
        await _unitOfwork.SaveChangeAsync();
        _logger.LogInformation("[{_className}][{method}] End", _className, method);
    }

    public async Task<IEnumerable<ChannelDto>> GetAllChannelsOfAWorkspaceAsync(Guid workspaceId)
    {
        var method = GetActualAsyncMethodName();
        try
        {
            _logger.LogInformation("[{_className}][{method}] Start", _className, method);
            var currentUserId = Guid.Parse(_currentUser.UserId ?? throw new Exception());

            var isMember = await _unitOfwork.Workspaces.CheckIsMemberAsync(workspaceId, currentUserId);
            if (!isMember)
            {
                throw new ForbidException();
            }

            var channels = await _unitOfwork.Channels
                .Queryable()
                .Include(x => x.ChannelMembers.Where(c => !c.IsDeleted))
                .Where(x => x.WorkspaceId == workspaceId && x.ChannelMembers.Any(c => c.UserId == currentUserId))
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
            var channel = await _unitOfwork.Channels
                .Queryable()
                .Include(x => x.ChannelMembers.Where(c => !c.IsDeleted))
                .FirstOrDefaultAsync(x => x.Id == channelId);

            if (channel is null)
                throw new NotFoundException<Channel>(channelId.ToString());

            var currentUserId = Guid.Parse(_currentUser.UserId ?? throw new Exception());

            if (!await _unitOfwork.Channels.CheckIsMemberAsync(channelId, currentUserId))
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
            var channels = await _unitOfwork.Channels
                .Queryable()
                .Include(x => x.ChannelMembers.Where(c => !c.IsDeleted))
                .Where(x => x.Name.Contains(channelName))
                .ToListAsync();

            var currentUserId = Guid.Parse(_currentUser.UserId ?? throw new Exception());
            channels = channels.Where(x => x.ChannelMembers.Any(c => c.UserId == currentUserId)).ToList();

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

    public async Task<IEnumerable<PermissionDto>> GetPermissionOfUser(Guid channelId, Guid userId)
    {
        var method = GetActualAsyncMethodName();
        _logger.LogInformation("[{_className}][{method}] Start", _className, method);
        var isExist = await _unitOfwork.Channels.CheckIsExistAsync(channelId);
        if (!isExist)
        {
            throw new NotFoundException<Channel>(channelId.ToString());
        }
        var isMember = _unitOfwork.Channels.CheckIsMemberAsync(channelId, userId);
        if (!isMember.Result)
        {
            throw new ForbidException();
        }
        var isOwner = await _unitOfwork.Channels.CheckIsOwnerAsync(channelId, userId);

        var permissions = await _unitOfwork.ChannelMembers.GetPermissionOfUser(
            channelId,
            userId
        );
        if (isOwner)
        {
            permissions = permissions.Concat(
                await _unitOfwork.ChannelPermissions
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

        var permissions = _unitOfwork.ChannelPermissions
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
        var isMember = await _unitOfwork.Channels.CheckIsMemberAsync(
            channelId,
            Guid.Parse(_currentUser.UserId ?? throw new UnauthorizedAccessException())
        );
        if (!isMember)
        {
            throw new ForbidException();
        }
        var isExistRole = await _unitOfwork.Channels.CheckIsExistRole(channelId, roleId);
        if (!isExistRole)
        {
            throw new NotFoundException<ChannelRole>(roleId.ToString());
        }
        var channelRole = await _unitOfwork.Channels.GetRoleById(channelId, roleId);

        var permissions = channelRole.Permissions
            .Where(x => !x.IsDeleted && x.Permission.IsActive)
            .ToList();
        var permissionDtos = _mapper.Map<List<PermissionDto>>(permissions);
        permissionDtos.ForEach(x => x.IsEnabled = true);

        var activePermissions = _unitOfwork.ChannelPermissions
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

    public async Task<object> GetRolesAsync(Guid channelId)
    {
        var method = GetActualAsyncMethodName();
        var userId = Guid.Parse(_currentUser.UserId ?? throw new UnauthorizedAccessException());
        _logger.LogInformation("[{_className}][{method}] Start", _className, method);
        var isMember = await _unitOfwork.Channels.CheckIsMemberAsync(channelId, userId);
        if (!isMember)
        {
            throw new ForbidException();
        }

        _logger.LogInformation("[{_className}][{method}] End", _className, method);

        return _mapper.Map<IEnumerable<ChannelRoleDto>>(
            await _unitOfwork.Channels.GetRoles(channelId)
        );
    }

    public async Task<Guid> RemoveMemberFromChannelAsync(Guid channelId, List<Guid> userIds)
    {
        var method = GetActualAsyncMethodName();
        try
        {
            _logger.LogInformation("[{_className}][{method}] Start", _className, method);
            var channel = await _unitOfwork.Channels
                .Queryable()
                .Include(x => x.ChannelMembers.Where(c => !c.IsDeleted))
                .Where(x => x.Id == channelId)
                .FirstOrDefaultAsync();

            var currentUserId = Guid.Parse(_currentUser.UserId ?? throw new Exception());
            if (channel is null)
            {
                throw new NotFoundException<Channel>(channelId.ToString());
            }

            if (!await _unitOfwork.Channels.CheckIsMemberAsync(channelId, currentUserId))
            {
                throw new ForbidException();
            }
            foreach (var userId in userIds)
            {
                var member = channel.ChannelMembers.FirstOrDefault(x => x.UserId == userId);
                if (member is null)
                {
                    throw new Exception($"User {userId} is not in this channel");
                }

                await _unitOfwork.ChannelMembers.DeleteAsync(member);
            }
            await _unitOfwork.SaveChangeAsync();
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

    public async Task SetRoleToUserAsync(Guid channelId, Guid userId, Guid roleId)
    {
        var method = GetActualAsyncMethodName();
        _logger.LogInformation("[{_className}][{method}] Start", _className, method);
        var isMember = await _unitOfwork.Channels.CheckIsMemberAsync(
            channelId,
            Guid.Parse(_currentUser.UserId ?? throw new UnauthorizedAccessException())
        );
        if (!isMember)
        {
            throw new ForbidException();
        }
        var isExistRole = await _unitOfwork.Channels.CheckIsExistRole(channelId, roleId);
        if (!isExistRole)
        {
            throw new NotFoundException<ChannelRole>(roleId.ToString());
        }
        isMember = await _unitOfwork.Channels.CheckIsMemberAsync(channelId, userId);
        if (!isMember)
        {
            throw new NotAMemberOfWorkspaceException();
        }
        var member = await _unitOfwork.Channels.GetMemberByUserId(channelId, userId);
        member.RoleId = roleId;
        await _unitOfwork.ChannelMembers.UpdateAsync(member);
        await _unitOfwork.SaveChangeAsync();
        _logger.LogInformation("[{_className}][{method}] End", _className, method);
    }

    public async Task<Guid> UpdateAsync(Guid channelId, UpdateChannelDto updateChannelDto)
    {
        var method = GetActualAsyncMethodName();
        try
        {
            _logger.LogInformation("[{_className}][{method}] Start", _className, method);
            var channel = await _unitOfwork.Channels.Queryable()
                .Include(x => x.ChannelMembers.Where(c => !c.IsDeleted))
                .Where(x => x.Id == channelId)
                .FirstOrDefaultAsync();
            var currentUserId = Guid.Parse(_currentUser.UserId ?? throw new Exception());

            if (channel is null)
            {
                throw new NotFoundException<Channel>(channelId.ToString());
            }

            if (!await _unitOfwork.Channels.CheckIsMemberAsync(channelId, currentUserId))
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
            await _unitOfwork.Channels.UpdateAsync(channel);
            await _unitOfwork.SaveChangeAsync();

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
        var isMember = await _unitOfwork.Channels.CheckIsMemberAsync(channelId, userId);
        if (!isMember)
        {
            throw new ForbidException();
        }
        var isExistRole = await _unitOfwork.Channels.CheckIsExistRole(channelId, roleId);
        if (!isExistRole)
        {
            throw new NotFoundException<WorkspaceRole>(roleId.ToString());
        }

        var role = await _unitOfwork.Channels.GetRoleById(channelId, roleId);
        _mapper.Map(input, role);
        await _unitOfwork.ChannelRoles.UpdateAsync(role);
        await _unitOfwork.SaveChangeAsync();

        _logger.LogInformation("[{_className}][{method}] End", _className, method);
    }
}
