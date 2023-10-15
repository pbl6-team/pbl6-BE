using System.ComponentModel;
using Microsoft.EntityFrameworkCore;
using PBL6.Domain.Models;
using PBL6.Domain.Models.Admins;
using PBL6.Domain.Models.Users;

namespace PBL6.Infrastructure.Data
{
    public class ApiDbContext : DbContext
    {
        // Admins
        public DbSet<Example> Examples { get; set; }
        public DbSet<Function> Functions { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<AdminAccount> AdminAccounts { get; set; }
        public DbSet<AdminInfo> AdminInfos { get; set; }

        // Users
        public DbSet<Plan> Plans { get; set; }
        public DbSet<PlanDetail> PlanDetails { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<UserNotification> UserNotifications { get; set; }
        public DbSet<UserToken> UserTokens { get; set; }
        public DbSet<Workspace> Workspaces { get; set; }
        public DbSet<Chanel> Chanels { get; set; }
        public DbSet<WorkspaceMember> WorkspaceMembers { get; set; }
        public DbSet<ChanelMember> ChanelMembers { get; set; }
        public DbSet<WorkspaceRole> WorkspaceRoles { get; set; }
        public DbSet<WorkspacePermission> WorkspacePermissions { get; set; }
        public DbSet<PermissionsOfWorkspaceRole> PermissionsOfWorkspaces { get; set; }
        public DbSet<ChanelRole> ChanelRoles { get; set; }
        public DbSet<ChanelPermission> ChanelPermissions { get; set; }
        public DbSet<PermissionsOfChanelRole> PermissionsOfChanelRoles { get; set; }

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
               .HasMany(x => x.Chanels)
               .WithOne(x => x.Workspace)
               .HasForeignKey(x => x.WorkspaceId)
               .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Workspace>()
               .HasMany(x => x.Members)
               .WithOne(x => x.Workspace)
               .HasForeignKey(x => x.WorkspaceId)
               .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<WorkspaceRole>()
               .HasMany(x => x.Members)
               .WithOne(x => x.WorkspaceRole)
               .HasForeignKey(x => x.WorkspaceId)
               .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<WorkspaceRole>()
                .HasMany(e => e.Permissions)
                .WithMany(e => e.WorkspaceRoles)
                .UsingEntity<PermissionsOfWorkspaceRole>(
                    l => l.HasOne<WorkspacePermission>()
                        .WithMany()
                        .HasForeignKey(e => e.PermissionId),
                    r => r.HasOne<WorkspaceRole>()
                        .WithMany()
                        .HasForeignKey(e => e.WorkspaceRoleId)
                );

            modelBuilder.Entity<ChanelRole>()
                .HasMany(e => e.Permissions)
                .WithMany(e => e.ChanelRoles)
                .UsingEntity<PermissionsOfChanelRole>(
                    l => l.HasOne<ChanelPermission>()
                        .WithMany()
                        .HasForeignKey(e => e.PermissionId),
                    r => r.HasOne<ChanelRole>()
                        .WithMany()
                        .HasForeignKey(e => e.ChanelRoleId)
                );
        }
    }
}
