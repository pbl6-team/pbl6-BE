using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.Components;
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

        public ChannelService(
            IServiceProvider serviceProvider
            ) : base(serviceProvider)
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

                var workspace = await _unitOfwork.Workspaces.FindAsync(channel.WorkspaceId);
                if (workspace is null)
                {
                    throw new NotFoundException<Workspace>(channel.WorkspaceId.ToString());
                }
                
                if (channel.Description is null)
                {
                    channel.Description = string.Empty;
                }

                channel.OwnerId = currentUserId;

                channel.ChannelMembers = new List<ChannelMember>
                {
                    new(){UserId = currentUserId, AddBy = currentUserId}
                };
                channel.CategoryId = Guid.Empty;
                channel = await _unitOfwork.Channels.AddAsync(channel);
                await _unitOfwork.SaveChangeAsync();
                _logger.LogInformation("[{_className}][{method}] End", _className, method);

                return channel.Id;
            }
            catch (Exception e)
            {
                _logger.LogInformation("[{_className}][{method}] Error: {message}", _className, method, e.Message);

                throw;
            }
        
    }

    public async Task<Guid> AddMemberToChannelAsync(Guid channelId, Guid userId)
    {
        var method = GetActualAsyncMethodName();
        try
        {
            _logger.LogInformation("[{_className}][{method}] Start", _className, method);
            var channel = await _unitOfwork.Channels.Queryable().Include(x => x.ChannelMembers.Where(c => !c.IsDeleted)).Where(x => x.Id == channelId).FirstOrDefaultAsync();
            var currentUserId = Guid.Parse(_currentUser.UserId ?? throw new Exception());
            if (channel is null)
            {
                throw new NotFoundException<Channel>(channelId.ToString());
            }

            var workspace = await _unitOfwork.Workspaces.Queryable().Include(x => x.Members.Where(m => !m.IsDeleted)).Where(w => w.Id == channel.WorkspaceId).FirstOrDefaultAsync();
            if (!workspace.Members.Any(x => x.UserId == userId))
            {
                throw new Exception("User is not in the workspace of this channel");
            }

            var user = await _unitOfwork.Users.FindAsync(userId);
            if (user is null)
            {
                throw new NotFoundException<User>(userId.ToString());
            }

            var member = channel.ChannelMembers.FirstOrDefault(x => x.UserId == userId);
            if (member is not null)
            {
                throw new Exception("User is already in this channel");
            }

            var channelMember = new ChannelMember
            {
                UserId = userId,
                AddBy = currentUserId
            };

            channel.ChannelMembers.Add(channelMember);
            await _unitOfwork.SaveChangeAsync();
            _logger.LogInformation("[{_className}][{method}] End", _className, method);

            return channelId;
        }
        catch (Exception e)
        {
            _logger.LogInformation("[{_className}][{method}] Error: {message}", _className, method, e.Message);

            throw;
        }
    }

    public async Task<Guid> DeleteAsync(Guid channelId)
    {
        var method = GetActualAsyncMethodName();
        try
        {
            _logger.LogInformation("[{_className}][{method}] Start", _className, method);
            var channel = await _unitOfwork.Channels.FindAsync(channelId);

            if (channel is null) throw new NotFoundException<Channel>(channelId.ToString());

            await _unitOfwork.Channels.DeleteAsync(channel);
            await _unitOfwork.SaveChangeAsync();
            _logger.LogInformation("[{_className}][{method}] End", _className, method);

            return channel.Id;
        }
        catch (Exception e)
        {
            _logger.LogInformation("[{_className}][{method}] Error: {message}", _className, method, e.Message);

            throw;
        }
    }

    public async Task<IEnumerable<ChannelDto>> GetAllChannelsOfAWorkspaceAsync(Guid workspaceId)
    {
        var method = GetActualAsyncMethodName();
        try
        {
            _logger.LogInformation("[{_className}][{method}] Start", _className, method);
            var channels = await _unitOfwork.Channels.Queryable().Include(x => x.ChannelMembers.Where(c => !c.IsDeleted)).Where(x => x.WorkspaceId == workspaceId).ToListAsync();
            _logger.LogInformation("[{_className}][{method}] End", _className, method);
            return _mapper.Map<IEnumerable<ChannelDto>>(channels);
        }
        catch (Exception e)
        {
            _logger.LogInformation("[{_className}][{method}] Error: {message}", _className, method, e.Message);

            throw;
        }
    }

    public async Task<ChannelDto> GetByIdAsync(Guid channelId)
    {
        var method = GetActualAsyncMethodName();
        try
        {
            _logger.LogInformation("[{_className}][{method}] Start", _className, method);
            var channel = await _unitOfwork.Channels.Queryable().Include(x => x.ChannelMembers.Where(c => !c.IsDeleted)).FirstOrDefaultAsync(x => x.Id == channelId);

            if (channel is null) throw new NotFoundException<Channel>(channelId.ToString());

            _logger.LogInformation("[{_className}][{method}] End", _className, method);
            return _mapper.Map<ChannelDto>(channel);
        }
        catch (Exception e)
        {
            _logger.LogInformation("[{_className}][{method}] Error: {message}", _className, method, e.Message);

            throw;
        }
    }

    public async Task<IEnumerable<ChannelDto>> GetByNameAsync(string channelName)
    {
        var method = GetActualAsyncMethodName();
        try
        {
            _logger.LogInformation("[{_className}][{method}] Start", _className, method);
            var channels = await _unitOfwork.Channels.Queryable().Include(x => x.ChannelMembers.Where(c => !c.IsDeleted)).Where(x => x.Name.Contains(channelName)).ToListAsync();

            _logger.LogInformation("[{_className}][{method}] End", _className, method);
            return _mapper.Map<IEnumerable<ChannelDto>>(channels);
        }
        catch (Exception e)
        {
            _logger.LogInformation("[{_className}][{method}] Error: {message}", _className, method, e.Message);

            throw;
        }
    }

    public async Task<Guid> RemoveMemberFromChannelAsync(Guid channelId, Guid userId)
    {
        var method = GetActualAsyncMethodName();
        try
        {
            _logger.LogInformation("[{_className}][{method}] Start", _className, method);
            var channel = await _unitOfwork.Channels.Queryable().Include(x => x.ChannelMembers.Where(c => !c.IsDeleted)).Where(x => x.Id == channelId).FirstOrDefaultAsync();

            var currentUserId = Guid.Parse(_currentUser.UserId ?? throw new Exception());
            if (channel is null)
            {
                throw new NotFoundException<Channel>(channelId.ToString());
            }

            var member = channel.ChannelMembers.FirstOrDefault(x => x.UserId == userId);
            if (member is null)
            {
                throw new Exception("User is not in this channel");
            }

            await _unitOfwork.ChannelMembers.DeleteAsync(member);
            await _unitOfwork.SaveChangeAsync();
            _logger.LogInformation("[{_className}][{method}] End", _className, method);

            return channel.Id;
        }
        catch (Exception e)
        {
            _logger.LogInformation("[{_className}][{method}] Error: {message}", _className, method, e.Message);

            throw;
        }
    }

    public async Task<Guid> UpdateAsync(Guid channelId, UpdateChannelDto updateChannelDto)
    {
        var method = GetActualAsyncMethodName();
        try
        {
            _logger.LogInformation("[{_className}][{method}] Start", _className, method);
            var channel = await _unitOfwork.Channels.FindAsync(channelId);
            if (channel is null)
            {
                throw new NotFoundException<Channel>(channelId.ToString());
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
            _logger.LogInformation("[{_className}][{method}] Error: {message}", _className, method, e.Message);

            throw;
        }
    }
}