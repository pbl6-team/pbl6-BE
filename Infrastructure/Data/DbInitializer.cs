using Microsoft.EntityFrameworkCore;
using PBL6.Common.Enum;
using PBL6.Common.Functions;
using PBL6.Domain.Models.Admins;
using PBL6.Domain.Models.Users;

namespace PBL6.Infrastructure.Data
{
    public static class DbInitializer
    {
        public static async Task Initialize(ApiDbContext context)
        {
            context.Database.EnsureCreated();

            if (!context.Users.Any())
            {
                await SeedUsers(context);
            }

            if (!context.WorkspacePermissions.Any())
            {
                await SeedPermissions(context);
            }

            if (!context.AdminAccounts.Any())
            {
                await SeedAdminAccount(context);
            }
        }

        public static async Task SeedUsers(ApiDbContext context)
        {
            await context.Users.AddAsync(new User
                {
                    Email = "User@gmail.com",
                    Username = "User",
                    Password = SecurityFunction.HashPassword("User@123", "123"),
                    PasswordSalt = "123",
                    IsActive = true,
                    Information = new()
                    {
                        FirstName = "User",
                        LastName = "Test",
                        BirthDay = DateTimeOffset.Now,
                        Phone = "",
                        Gender = true,
                        Status = (short)USER.VERIFIED
                    }
                }
            );
            await context.SaveChangesAsync();
        }


        public static async Task SeedPermissions(ApiDbContext context)
        {
            await context.WorkspacePermissions.AddRangeAsync
            (
                new List<WorkspacePermission>
                {
                    new () { Code = "CHANGE_WORKSPACE_NAME", Name = "Change workspace name", Description = "Change workspace name", IsActive = true },
                    new () { Code = "CREATE_UPDATE_CHANNEL", Name = "Create/update channel", Description = "Create/update channel", IsActive = true },
                    new () { Code = "DELETE_CHANNEL", Name = "Delete channel", Description = "Delete channel", IsActive = true },
                    new () { Code = "INVITE_MEMBER", Name = "Invite member", Description = "Invite member", IsActive = true },
                    new () { Code = "DELETE_MEMBER", Name = "Delete member", Description = "Delete member", IsActive = true },
                    new () { Code = "CREATE_UPDATE_ROLE", Name = "Create/update member role", Description = "Create/update member role", IsActive = true }
                }
            );
            await context.SaveChangesAsync();

            await context.ChannelPermissions.AddRangeAsync
            (
                new List<ChannelPermission>
                {
                    new () { Code = "CHANGE_CHANNEL_NAME", Name = "Change channel name", Description = "Change channel name", IsActive = true },
                    new () { Code = "INVITE_MEMBER", Name = "Invite member", Description = "Invite member", IsActive = true },
                    new () { Code = "DELETE_MEMBER", Name = "Delete member", Description = "Delete member", IsActive = true },
                    new () { Code = "CREATE_UPDATE_ROLE", Name = "Create/update member role", Description = "Create/update member role", IsActive = true },
                    new () { Code = "DELETE_OTHER_PEOPLE'S_MESSAGE", Name = "Delete other people's messages", Description = "Delete other people's messages", IsActive = true }
                }
            );
            await context.SaveChangesAsync();
        }

        public static async Task SeedAdminAccount(ApiDbContext context)
        {
            await context.AdminAccounts.AddAsync(new AdminAccount
                {
                    Username = "root",
                    Password = SecurityFunction.HashPassword("Root@123", "123"),
                    PasswordSalt = "123",
                    Email = "Root@fira.com",
                    IsActive = true,
                    Information = new ()
                    {
                        FirstName = "Root",
                        LastName = "Root",
                        BirthDate = DateTimeOffset.Now,
                    }
                }
            );
            await context.SaveChangesAsync();
        }
    }
}