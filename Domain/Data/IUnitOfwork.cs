using Microsoft.EntityFrameworkCore.Storage;
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

        Task SaveChangeAsync();
        Task<IDbContextTransaction> BeginTransactionAsync();
        Task CommitAsync(IDbContextTransaction transaction);
        Task RollbackAsync(IDbContextTransaction transaction);
        void Dispose();
    }
}