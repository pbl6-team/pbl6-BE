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

        public async Task<WorkspaceDto> DeleteAsync(Guid id)
        {
            var method = GetActualAsyncMethodName();
            try
            {
                _logger.LogInformation("[{_className}][{method}] Start", _className, method);
                var workspace = await _unitOfwork.Workspaces.FindAsync(id);

                if (workspace is null) throw new NotFoundException<Workspace>(id.ToString());

                await _unitOfwork.Workspaces.DeleteAsync(workspace);
                await _unitOfwork.SaveChangeAsync();
                _logger.LogInformation("[{_className}][{method}] End", _className, method);

                return _mapper.Map<WorkspaceDto>(workspace);
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
                var workspaces = await _unitOfwork.Workspaces.Queryable().ToListAsync();

                _logger.LogInformation("[{_className}][{method}] End", _className, method);
                return _mapper.Map<IEnumerable<WorkspaceDto>>(workspaces);
            }
            catch (Exception e)
            {
                _logger.LogInformation("[{_className}][{method}] Error: {message}", _className, method, e.Message);

                throw;
            }
        }

        public async Task<WorkspaceDto> GetByIdAsync(Guid id)
        {
            var method = GetActualAsyncMethodName();
            try
            {
                _logger.LogInformation("[{_className}][{method}] Start", _className, method);
                var workspace = await _unitOfwork.Workspaces.Queryable().FirstOrDefaultAsync(x => x.Id == id);

                _logger.LogInformation("[{_className}][{method}] End", _className, method);
                return _mapper.Map<WorkspaceDto>(workspace);
            }
            catch (Exception e)
            {
                _logger.LogInformation("[{_className}][{method}] Error: {message}", _className, method, e.Message);

                throw;
            }
        }

        public async Task<IEnumerable<WorkspaceDto>> GetByNameAsync(string name)
        {
            var method = GetActualAsyncMethodName();
            try
            {
                _logger.LogInformation("[{_className}][{method}] Start", _className, method);
                var workspaces = await _unitOfwork.Workspaces.Queryable().Where(x => x.Name.Contains(name)).ToListAsync();

                _logger.LogInformation("[{_className}][{method}] End", _className, method);
                return _mapper.Map<IEnumerable<WorkspaceDto>>(workspaces);
            }
            catch (Exception e)
            {
                _logger.LogInformation("[{_className}][{method}] Error: {message}", _className, method, e.Message);

                throw;
            }
        }

        public async Task<WorkspaceDto> UpdateAsync(Guid id, UpdateWorkspaceDto updateWorkspaceDto)
        {
            var method = GetActualAsyncMethodName();
            try
            {
                _logger.LogInformation("[{_className}][{method}] Start", _className, method);
                var userId = Guid.Parse(_currentUser.UserId ?? throw new Exception());
                var workspace = await _unitOfwork.Workspaces.FindAsync(id);

                if (workspace is null) throw new NotFoundException<Workspace>(id.ToString());

                if (updateWorkspaceDto.Description is null)
                {
                    updateWorkspaceDto.Description = string.Empty;
                }

                _mapper.Map(updateWorkspaceDto, workspace);
                await _unitOfwork.Workspaces.UpdateAsync(workspace);
                await _unitOfwork.SaveChangeAsync();

                _logger.LogInformation("[{_className}][{method}] End", _className, method);
                return _mapper.Map<WorkspaceDto>(workspace);
            }
            catch (Exception e)
            {
                _logger.LogInformation("[{_className}][{method}] Error: {message}", _className, method, e.Message);

                throw;
            }
        }

        public async Task<WorkspaceDto> UpdateAvatarAsync(Guid id, UpdateAvatarWorkspaceDto updateAvatarWorkspaceDto)
        {
            var method = GetActualAsyncMethodName();
            try
            {
                _logger.LogInformation("[{_className}][{method}] Start", _className, method);
                var userId = Guid.Parse(_currentUser.UserId ?? throw new Exception());
                var workspace = await _unitOfwork.Workspaces.FindAsync(id);

                if (workspace is null) throw new NotFoundException<Workspace>(id.ToString());
                
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

                return _mapper.Map<WorkspaceDto>(workspace);
            }
            catch (Exception e)
            {
                _logger.LogInformation("[{_className}][{method}] Error: {message}", _className, method, e.Message);

                throw;
            }
        }
    }
}