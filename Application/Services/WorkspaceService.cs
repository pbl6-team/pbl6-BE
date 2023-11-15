using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PBL6.Application.Contract.Workspaces;
using PBL6.Application.Contract.Workspaces.Dtos;
using PBL6.Common.Consts;
using PBL6.Common.Exceptions;
using PBL6.Common.Functions;
using PBL6.Domain.Models.Users;

namespace PBL6.Application.Services
{
    public class WorkspaceService : BaseService, IWorkspaceService
    {
        private readonly string _className;

        public WorkspaceService(
            IServiceProvider serviceProvider
            ) : base(serviceProvider)
        {
            _className = typeof(WorkspaceService).Name;
        }

        static string GetActualAsyncMethodName([CallerMemberName] string name = null) => name;

        public async Task<Guid> AddAsync(CreateWorkspaceDto workspaceDto)
        {
            var method = GetActualAsyncMethodName();
            try
            {
                _logger.LogInformation("[{_className}][{method}] Start", _className, method);
                var workspace = _mapper.Map<Workspace>(workspaceDto);
                var userId = Guid.Parse(_currentUser.UserId ?? throw new Exception());

                if (workspaceDto.Avatar is not null)
                {
                    var imageUrl = await _fileService.UploadImageToImgbb(workspaceDto.Avatar, workspace.Id);
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
                    new () {
                        OwnerId = userId,
                        Name = CommonConsts.DEFAULT_CHANNEL,
                        Description = string.Empty,
                        ChannelMembers = new List<ChannelMember>
                        {
                            // TODO: Change roleId, status
                            new(){UserId = userId, AddBy = userId}
                        }
                    }
                };
                workspace.Members = new List<WorkspaceMember>
                {
                    // TODO: Change roleId, status
                    new(){UserId = userId, AddBy = userId}
                };

                workspace = await _unitOfwork.Workspaces.AddAsync(workspace);
                await _unitOfwork.SaveChangeAsync();
                _logger.LogInformation("[{_className}][{method}] End", _className, method);

                return workspace.Id;
            }
            catch (Exception e)
            {
                _logger.LogInformation("[{_className}][{method}] Error: {message}", _className, method, e.Message);

                throw;
            }
        }

        public async Task<Guid> DeleteAsync(Guid workspaceId)
        {
            var method = GetActualAsyncMethodName();
            try
            {
                _logger.LogInformation("[{_className}][{method}] Start", _className, method);
                var workspace = await _unitOfwork.Workspaces.FindAsync(workspaceId);

                if (workspace is null) throw new NotFoundException<Workspace>(workspaceId.ToString());

                await _unitOfwork.Workspaces.DeleteAsync(workspace);
                await _unitOfwork.SaveChangeAsync();
                _logger.LogInformation("[{_className}][{method}] End", _className, method);

                return workspace.Id;
            }
            catch (Exception e)
            {
                _logger.LogInformation("[{_className}][{method}] Error: {message}", _className, method, e.Message);

                throw;
            }
        }

        public async Task<IEnumerable<WorkspaceDto>> GetAllAsync()
        {
            var method = GetActualAsyncMethodName();
            try
            {
                _logger.LogInformation("[{_className}][{method}] Start", _className, method);
                var workspaces = await _unitOfwork.Workspaces.Queryable().Include(x => x.Channels.Where(c => !c.IsDeleted)).Include(x => x.Members.Where(m => !m.IsDeleted)).ToListAsync();

                _logger.LogInformation("[{_className}][{method}] End", _className, method);
                return _mapper.Map<IEnumerable<WorkspaceDto>>(workspaces);
            }
            catch (Exception e)
            {
                _logger.LogInformation("[{_className}][{method}] Error: {message}", _className, method, e.Message);

                throw;
            }
        }

        public async Task<WorkspaceDto> GetByIdAsync(Guid workspaceId)
        {
            var method = GetActualAsyncMethodName();
            try
            {
                _logger.LogInformation("[{_className}][{method}] Start", _className, method);
                var workspace = await _unitOfwork.Workspaces.Queryable().Include(x => x.Channels.Where(c => !c.IsDeleted)).Include(x => x.Members.Where(m => !m.IsDeleted)).FirstOrDefaultAsync(x => x.Id == workspaceId);
                if (workspace is null) throw new NotFoundException<Workspace>(workspaceId.ToString());
                _logger.LogInformation("[{_className}][{method}] End", _className, method);
                return _mapper.Map<WorkspaceDto>(workspace);
            }
            catch (Exception e)
            {
                _logger.LogInformation("[{_className}][{method}] Error: {message}", _className, method, e.Message);

                throw;
            }
        }

        public async Task<IEnumerable<WorkspaceDto>> GetByNameAsync(string workspaceName)
        {
            var method = GetActualAsyncMethodName();
            try
            {
                _logger.LogInformation("[{_className}][{method}] Start", _className, method);
                var workspaces = await _unitOfwork.Workspaces.Queryable().Include(x => x.Channels.Where(c => !c.IsDeleted)).Include(x => x.Members.Where(m => !m.IsDeleted)).Where(x => x.Name.Contains(workspaceName)).ToListAsync();

                _logger.LogInformation("[{_className}][{method}] End", _className, method);
                return _mapper.Map<IEnumerable<WorkspaceDto>>(workspaces);
            }
            catch (Exception e)
            {
                _logger.LogInformation("[{_className}][{method}] Error: {message}", _className, method, e.Message);

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
                var workspace = await _unitOfwork.Workspaces.FindAsync(workspaceId);

                if (workspace is null) throw new NotFoundException<Workspace>(workspaceId.ToString());

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
                _logger.LogInformation("[{_className}][{method}] Error: {message}", _className, method, e.Message);

                throw;
            }
        }

        public async Task<Guid> UpdateAvatarAsync(Guid workspaceId, UpdateAvatarWorkspaceDto updateAvatarWorkspaceDto)
        {
            var method = GetActualAsyncMethodName();
            try
            {
                _logger.LogInformation("[{_className}][{method}] Start", _className, method);
                var userId = Guid.Parse(_currentUser.UserId ?? throw new Exception());
                var workspace = await _unitOfwork.Workspaces.FindAsync(workspaceId);

                if (workspace is null) throw new NotFoundException<Workspace>(workspaceId.ToString());
                
                if (updateAvatarWorkspaceDto.Avatar is not null)
                {
                    workspace.AvatarUrl = await _fileService.UploadImageToImgbb(updateAvatarWorkspaceDto.Avatar, workspace.Id);
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
                _logger.LogInformation("[{_className}][{method}] Error: {message}", _className, method, e.Message);

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
                var workspace = await _unitOfwork.Workspaces.Queryable().Include(x => x.Members.Where(m => !m.IsDeleted)).FirstOrDefaultAsync(x => x.Id == workspaceId);

                if (workspace is null)
                {
                    throw new NotFoundException<Workspace>(workspaceId.ToString());
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

                workspace.Members.Add(new WorkspaceMember
                {
                    UserId = userId,
                    AddBy = currentUserId
                });

                await _unitOfwork.Workspaces.UpdateAsync(workspace);
                await _unitOfwork.SaveChangeAsync();

                _logger.LogInformation("[{_className}][{method}] End", _className, method);

                return workspace.Id;
            }
            catch (Exception e)
            {
                _logger.LogInformation("[{_className}][{method}] Error: {message}", _className, method, e.Message);
                throw;
            }
        }

        public async Task<Guid> RemoveMemberFromWorkspaceAsync(Guid workspaceId, Guid userId)
        {
            var method = GetActualAsyncMethodName();
            try
            {
                _logger.LogInformation("[{_className}][{method}] Start", _className, method);

                var workspace = await _unitOfwork.Workspaces.Queryable().Include(x => x.Members.Where(m => !m.IsDeleted)).FirstOrDefaultAsync(x => x.Id == workspaceId);
                if (workspace is null)
                {
                    throw new NotFoundException<Workspace>(workspaceId.ToString());
                }

                var member = workspace.Members.FirstOrDefault(x => x.UserId == userId);
                if (member is null)
                {
                    throw new Exception("User is not a member of this workspace");
                }

                await _unitOfwork.WorkspaceMembers.DeleteAsync(member);
                
                var channels = await _unitOfwork.Channels.Queryable().Include(c => c.ChannelMembers.Where(m => !m.IsDeleted)).Where(x => x.WorkspaceId == workspaceId).ToListAsync();
                foreach (var channel in channels)
                {
                    var channelMember = channel.ChannelMembers.FirstOrDefault(x => x.UserId == userId);
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
                _logger.LogInformation("[{_className}][{method}] Error: {message}", _className, method, e.Message);
                throw;
            }
        }
    }
}