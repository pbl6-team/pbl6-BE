using Application.Services;
using PBL6.API.Services;
using PBL6.Application.Contract.Channels;
using PBL6.Application.Contract.Chats;
using PBL6.Application.Contract.Common;
using PBL6.Application.Contract.Users;
using PBL6.Application.Contract.Workspaces;
using PBL6.Application.Services;
using PBL6.Common;
using PBL6.Domain.Data;
using PBL6.Infrastructure.Data;
using workspace.PBL6.Application.Services;

namespace PBL6.API.Extensions
{
    public static class InfrastructureServices
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton<StartupState>();
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<ICurrentUserService, CurrentUserService>();
            services.AddScoped<IWorkspaceService, WorkspaceService>();
            services.AddScoped<IChannelService, ChannelService>();
            services.AddScoped<IChatService, ChatService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<Application.Contract.Admins.IAuthService, Application.Services.Admins.AuthService>();
            
            services.AddTransient<IMailService, MailService>();
            services.AddTransient<IFileService, FileService>();

            return services;
        }
    }
}