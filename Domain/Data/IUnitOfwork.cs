using Microsoft.EntityFrameworkCore.Storage;
using PBL6.Domain.Data.Users;

namespace PBL6.Domain.Data
{
    public interface IUnitOfWork
    {
        IExampleRepository Examples { get; }
        IUserRepository Users { get; }
        IUserTokenRepository UserTokens { get; }
        IWorkspaceRepository Workspaces { get; }
        IChannelRepository Channels { get; }
        IWorkspaceMemberRepository WorkspaceMembers { get; }
        IChannelMemberRepository ChannelMembers { get; }
        Task SaveChangeAsync();
        Task<IDbContextTransaction> BeginTransactionAsync();
        Task CommitAsync(IDbContextTransaction transaction);
        Task RollbackAsync(IDbContextTransaction transaction);
        void Dispose();
    }
}