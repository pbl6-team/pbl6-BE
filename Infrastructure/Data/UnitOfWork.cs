using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using PBL6.Domain.Data;
using PBL6.Domain.Data.Admins;
using PBL6.Domain.Data.Users;
using PBL6.Domain.Models.Users;
using PBL6.Infrastructure.Repositories;

namespace PBL6.Infrastructure.Data
{
    public class UnitOfWork : IUnitOfWork, IDisposable
    {
        private readonly ApiDbContext _apiDbContext;

        public IUserRepository Users { get; }
        public IUserTokenRepository UserTokens { get; }
        public IWorkspaceRepository Workspaces { get; }
        public IChannelRepository Channels { get; }
        public IWorkspaceMemberRepository WorkspaceMembers { get; }
        public IChannelMemberRepository ChannelMembers { get; }
        public IWorkspaceRoleRepository WorkspaceRoles { get; }
        public IWorkspacePermissionRepository WorkspacePermissions { get; }
        public IChannelPermissionRepository ChannelPermissions { get; }
        public IChannelRoleRepository ChannelRoles { get; }
        public IMessageRepository Messages { get; }
        public IAdminRepository Admins { get; }
        public IAdminTokenRepository AdminTokens { get; }
        public IPermissionsOfWorkspaceRoleRepository PermissionsOfWorkspaceRoles { get; }
        public IPermissionsOfChannelRoleRepository PermissionsOfChannelRoles { get; }
        public INotificationRepository Notifications { get; }

        public UnitOfWork(ApiDbContext apiDbContext, ILoggerFactory loggerFactory)
        {
            _apiDbContext = apiDbContext;
            var logger = loggerFactory.CreateLogger("logs");

            Users = new UserRepository(apiDbContext, logger);
            UserTokens = new UserTokenRepository(apiDbContext, logger);
            Workspaces = new WorkspaceRepository(apiDbContext, logger);
            Channels = new ChannelRepository(apiDbContext, logger);
            WorkspaceMembers = new WorkspaceMemberRepository(apiDbContext, logger);
            ChannelMembers = new ChannelMemberRepository(apiDbContext, logger);
            WorkspaceRoles = new WorkspaceRoleRepository(apiDbContext, logger);
            WorkspacePermissions = new WorkspacePermissionRepository(apiDbContext, logger);
            ChannelPermissions = new ChannelPermissionRepository(apiDbContext, logger);
            ChannelRoles = new ChannelRoleRepository(apiDbContext, logger);
            Messages = new MessageRepository(apiDbContext, logger);
            Admins = new AdminRepository(apiDbContext, logger);
            AdminTokens = new AdminTokenRepository(apiDbContext, logger);
            PermissionsOfWorkspaceRoles = new PermissionsOfWorkspaceRoleRepository(apiDbContext, logger);
            PermissionsOfChannelRoles = new PermissionsOfChannelRoleRepository(apiDbContext, logger);
            Notifications = new NotificationRepository(apiDbContext, logger);
        }

        public async Task SaveChangeAsync()
        {
            await _apiDbContext.SaveChangesAsync();
        }

        public void Dispose()
        {
            _apiDbContext.Dispose();
        }

        public async Task<IDbContextTransaction> BeginTransactionAsync()
        {
            return await _apiDbContext.Database.BeginTransactionAsync();
        }

        public async Task CommitAsync(IDbContextTransaction transaction)
        {
            if (transaction is not null)
                await transaction.CommitAsync();
        }

        public async Task RollbackAsync(IDbContextTransaction transaction)
        {
            if (transaction is not null)
                await transaction.RollbackAsync();
        }

        public IRepository<T> Repository<T>() where T : class
        {
            return new Repository<T>(_apiDbContext, null);
        }
    }
}
