using Application.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PBL6.Application.Contract.Channels;
using PBL6.Application.Contract.Chats;
using PBL6.Application.Contract.Common;
using PBL6.Application.Contract.Users;
using PBL6.Application.Contract.Workspaces;
using PBL6.Application.Services;
using PBL6.Common;
using INotificationService = PBL6.Application.Services.INotificationService;
using NotificationService = PBL6.Application.Services.NotificationService;
using ExternalNotificationService = PBL6.Application.ExternalServices.NotificationService;
using IExternalNotificationService = PBL6.Application.Contract.ExternalServices.Notifications.INotificationService;
using PBL6.Application.ExternalServices;
using Application.Contract.Admins;
using Application.Services.Admins;
using Application.Contract.Dashboard;

namespace PBL6.Application
{
    public static class InfrastructureServices
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton<StartupState>();
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<Contract.Admins.IAuthService, Services.Admins.AuthService>();
            services.AddScoped<ICurrentUserService, CurrentUserService>();
            services.AddScoped<IWorkspaceService, WorkspaceService>();
            services.AddScoped<IChannelService, ChannelService>();
            services.AddScoped<IChatService, ChatService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<INotificationService, NotificationService>();
            services.AddScoped<IAdminService, AdminService>();
            services.AddScoped<IDashboardService, DashboardService>();

            services.AddScoped<IExternalNotificationService, ExternalNotificationService>();
            services.AddTransient<IMailService, MailService>();
            services.AddTransient<IFileService, FileService>();
            services.AddTransient<IMeetingService, MeetingService>();

            return services;
        }
    }
}