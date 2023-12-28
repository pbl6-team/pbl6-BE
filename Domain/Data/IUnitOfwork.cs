using Microsoft.EntityFrameworkCore.Storage;
using PBL6.Domain.Data.Admins;
using PBL6.Domain.Data.Users;

namespace PBL6.Domain.Data
{
    public interface IUnitOfWork
    {
        IUserRepository Users { get; }
        IUserTokenRepository UserTokens { get; }
        IWorkspaceRepository Workspaces { get; }
        IChannelRepository Channels { get; }
        IWorkspaceMemberRepository WorkspaceMembers { get; }
        IChannelMemberRepository ChannelMembers { get; }
        IWorkspaceRoleRepository WorkspaceRoles { get; }
        IWorkspacePermissionRepository WorkspacePermissions { get; }
        IChannelPermissionRepository ChannelPermissions { get; }
        IChannelRoleRepository ChannelRoles { get; }
        IMessageRepository Messages { get; }
        IMessageTrackingRepository MessageTrackings { get; }
        IAdminRepository Admins { get; }
        IAdminTokenRepository AdminTokens { get; }
        IPermissionsOfWorkspaceRoleRepository PermissionsOfWorkspaceRoles { get; }
        IPermissionsOfChannelRoleRepository PermissionsOfChannelRoles { get; }
        INotificationRepository Notifications { get; }

        IRepository<T> Repository<T>() where T : class;

        Task SaveChangeAsync();
        Task<IDbContextTransaction> BeginTransactionAsync();
        Task CommitAsync(IDbContextTransaction transaction);
        Task RollbackAsync(IDbContextTransaction transaction);
        void Dispose();
    }
}