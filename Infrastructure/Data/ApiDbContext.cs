using System.ComponentModel;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PBL6.Common;
using PBL6.Common.Enum;
using PBL6.Common.Functions;
using PBL6.Domain.Models.Admins;
using PBL6.Domain.Models.Common;
using PBL6.Domain.Models.Users;

namespace PBL6.Infrastructure.Data
{
    public class ApiDbContext : DbContext
    {
        private IHttpContextAccessor _context;
        // Admins
        public DbSet<Function> Functions { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<AdminAccount> AdminAccounts { get; set; }
        public DbSet<AdminInfo> AdminInfos { get; set; }
        public DbSet<AdminToken> AdminTokens { get; set; }

        // Users
        public DbSet<User> Users { get; set; }
        public DbSet<UserInfo> UserInfos { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<UserNotification> UserNotifications { get; set; }
        public DbSet<UserToken> UserTokens { get; set; }
        public DbSet<Workspace> Workspaces { get; set; }
        public DbSet<Channel> Channels { get; set; }
        public DbSet<ChannelCategory> ChannelCategories { get; set; }
        public DbSet<WorkspaceMember> WorkspaceMembers { get; set; }
        public DbSet<ChannelMember> ChannelMembers { get; set; }
        public DbSet<WorkspaceRole> WorkspaceRoles { get; set; }
        public DbSet<WorkspacePermission> WorkspacePermissions { get; set; }
        public DbSet<PermissionsOfWorkspaceRole> PermissionsOfWorkspaces { get; set; }
        public DbSet<ChannelRole> ChannelRoles { get; set; }
        public DbSet<ChannelPermission> ChannelPermissions { get; set; }
        public DbSet<PermissionsOfChannelRole> PermissionsOfChanelRoles { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<MessageTracking> MessageTrackings { get; set; }
        public DbSet<FileOfMessage> Files { get; set; }
        public DbSet<Meeting> Meetings { get; set; }
        public DbSet<Call> Calls { get; set; }

        public ApiDbContext() { }

        public ApiDbContext(DbContextOptions<ApiDbContext> options)
            : base(options) { }

        protected override void ConfigureConventions(ModelConfigurationBuilder builder)
        {
            builder.Properties<DateOnly>()
                .HaveConversion<DateOnlyConverter>()
                .HaveColumnType("date");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Admin
            modelBuilder.Entity<Function>()
                .HasMany(e => e.Roles)
                .WithMany(e => e.Functions)
                .UsingEntity(
                    "FunctionsOfRoles",
                    l => l.HasOne(typeof(Role))
                        .WithMany()
                        .HasForeignKey("RoleId")
                        .HasPrincipalKey(nameof(Role.Id)),
                    r => r.HasOne(typeof(Function))
                        .WithMany()
                        .HasForeignKey("FunctionId")
                        .HasPrincipalKey(nameof(Function.Id)),
                    j => j.HasKey("RoleId", "FunctionId")
                );

            modelBuilder.Entity<AdminAccount>()
                .HasOne(e => e.Information)
                .WithOne(e => e.AdminAccount)
                .HasForeignKey<AdminAccount>(e => e.InfoId)
                .IsRequired();

            modelBuilder.Entity<AdminAccount>()
                .HasMany(e => e.Roles)
                .WithMany(e => e.AdminAccounts)
                .UsingEntity<RolesOfAdmin>(
                    l => l.HasOne<Role>()
                        .WithMany()
                        .HasForeignKey(e => e.RoleId),
                    r => r.HasOne<AdminAccount>()
                        .WithMany()
                        .HasForeignKey(e => e.AdminId)
                );

            modelBuilder.Entity<AdminAccount>()
                .HasMany(x => x.AdminTokens)
                .WithOne(x => x.AdminAccount)
                .HasForeignKey(x => x.AdminId);

            // User
            modelBuilder.Entity<User>()
               .HasMany(x => x.UserTokens)
               .WithOne(x => x.User)
               .HasForeignKey(x => x.UserId);

            modelBuilder.Entity<User>()
                .HasOne(e => e.Information)
                .WithOne(e => e.User)
                .HasForeignKey<User>(e => e.InfoId)
                .IsRequired();

            modelBuilder.Entity<Notification>()
               .HasMany(x => x.UserNotifications)
               .WithOne(x => x.Notification)
               .HasForeignKey(x => x.NotificationId);

            modelBuilder.Entity<User>()
               .HasMany(x => x.UserNotifications)
               .WithOne(x => x.User)
               .HasForeignKey(x => x.UserId)
               .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Workspace>()
               .HasMany(x => x.Channels)
               .WithOne(x => x.Workspace)
               .HasForeignKey(x => x.WorkspaceId)
               .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Workspace>()
               .HasMany(x => x.Members)
               .WithOne(x => x.Workspace)
               .HasForeignKey(x => x.WorkspaceId)
               .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Workspace>()
               .HasMany(x => x.WorkspaceRoles)
               .WithOne(x => x.Workspace)
               .HasForeignKey(x => x.WorkspaceId)
               .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<WorkspaceMember>()
                .Property(x => x.Status)
                .HasDefaultValue(1);

            modelBuilder.Entity<WorkspaceRole>()
               .HasMany(x => x.Members)
               .WithOne(x => x.WorkspaceRole)
               .HasForeignKey(x => x.RoleId)
               .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<WorkspaceRole>()
               .HasMany(x => x.Permissions)
               .WithOne(x => x.WorkspaceRole)
               .HasForeignKey(x => x.WorkspaceRoleId)
               .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<ChannelMember>()
               .Property(x => x.Status)
                .HasDefaultValue(1);

            modelBuilder.Entity<ChannelRole>()
               .HasMany(x => x.Permissions)
               .WithOne(x => x.ChannelRole)
               .HasForeignKey(x => x.ChannelRoleId)
               .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<ChannelRole>()
                .HasMany(x => x.Members)
                .WithOne(x => x.ChannelRole)
                .HasForeignKey(x => x.RoleId)
                .OnDelete(DeleteBehavior.NoAction);
            
            modelBuilder.Entity<Channel>()
               .HasMany(x => x.ChannelRoles)
               .WithOne(x => x.Channel)
               .HasForeignKey(x => x.ChannelId)
               .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Message>()
                .HasMany(x => x.Children)
                .WithOne(x => x.Parent)
                .HasForeignKey(x => x.ParentId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Message>()
                .HasMany(x => x.MessageTrackings)
                .WithOne(x => x.Message)
                .HasForeignKey(x => x.MessageId)
                .OnDelete(DeleteBehavior.NoAction);
            modelBuilder.Entity<Message>()
                .HasOne(x => x.Sender)
                .WithMany()
                .HasForeignKey(x => x.CreatedBy)
                .OnDelete(DeleteBehavior.NoAction);
            
            modelBuilder.Entity<Message>()
                .HasOne(x => x.Receiver)
                .WithMany()
                .HasForeignKey(x => x.ToUserId)
                .OnDelete(DeleteBehavior.NoAction);
            
            modelBuilder.Entity<Message>()
                .HasMany(x => x.Files)
                .WithOne(x => x.Message)
                .HasForeignKey(x => x.MessageId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Meeting>()
                .HasOne(x => x.Channel)
                .WithMany(c => c.Meetings)
                .HasForeignKey(x => x.ChannelId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Message>()
                .Property(x => x.Type)
                .HasDefaultValue(MESSAGE_TYPE.DEFAULT);
        }

        public override int SaveChanges()
        {
            OnBeforeSaving();
            return base.SaveChanges();
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            OnBeforeSaving();
            return await base.SaveChangesAsync(cancellationToken);
        }

        /// <summary>
        /// Lưu thêm các thông tin cần thiết mặc định khi cập nhật dữ diệu vào Database.
        /// <para>Created at: 10/07/2020</para>
        /// <para>Created by: nhamcotdo</para>
        /// </summary>
        private void OnBeforeSaving()
        {
            // Nếu có sự thay đổi dữ liệu
            if (ChangeTracker.HasChanges())
            {
                // Láy các thông tin cơ bản từ hệ thống
                DateTimeOffset now = DateTimeOffset.UtcNow;
                var accountId = GetAccountId();
                // Duyệt qua hết tất cả dối tượng có thay đổi
                foreach (var entry in ChangeTracker.Entries())
                {
                    try
                    {
                        if (entry.Entity is AuditedEntity root)
                        {
                            switch (entry.State)
                            {
                                case EntityState.Added:
                                    {
                                        root.CreatedAt = now;
                                        root.CreatedBy = accountId;
                                        root.UpdatedAt = null;
                                        root.UpdatedBy = null;
                                        root.IsDeleted = false;
                                        break;
                                    }
                                case EntityState.Modified:
                                    {
                                        if (root.IsDeleted)
                                        {
                                            root.DeletedAt = now;
                                            root.DeletedBy = accountId;
                                        }
                                        else
                                        {
                                            root.UpdatedAt = now;
                                            root.UpdatedBy = accountId;
                                        }
                                        break;
                                    }
                            }
                        }
                    }
                    catch { }
                }
            }
        }

        /// <summary>
        /// Lấy user id đang đăng nhập nếu có
        /// <para>Created at: 15/10/2023</para>
        /// <para>Created by: nhamcotdo</para>
        /// </summary>
        /// <returns>user id của user đang đăng nhập. Trả về 0 nếu không có thông tin user đăng nhập</returns>
        public Guid? GetAccountId()
        {
            try
            {
                Guid? accountId = null;
                ClaimsPrincipal user = null;
                _context ??= StartupState.Instance.Services.GetService<IHttpContextAccessor>();

                if (_context != null && _context.HttpContext != null)
                {
                    user = _context.HttpContext.User;
                }
                if (user != null && user.Identity != null)
                {
                    var identity = user.Identity as ClaimsIdentity;
                    accountId = Guid.Parse(identity.Claims.Where(p => p.Type == CustomClaimTypes.UserId).FirstOrDefault()?.Value);
                }

                return accountId;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Lấy Ip của request hiện tại
        /// <para>Created at: 15/10/2023</para>
        /// <para>Created by: nhamcotdo</para>
        /// </summary>
        /// <returns>Ip của request hiện tại</returns>
        public string GetRequestIp()
        {
            try
            {
                _context ??= StartupState.Instance.Services.GetService<IHttpContextAccessor>();
                string headerIp = _context?.HttpContext == null ? "::1" : _context.HttpContext.Request.Headers["x-request-ip"].ToString();
                string ip = string.IsNullOrEmpty(headerIp) ? _context?.HttpContext.Connection.RemoteIpAddress.ToString() : headerIp;

                return ip;
            }
            catch
            {
                return "::1";
            }
        }
    }
}
