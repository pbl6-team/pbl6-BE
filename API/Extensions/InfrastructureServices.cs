using PBL6.API.Services;
using PBL6.Application.Contract.Common;
using PBL6.Application.Contract.Examples;
using PBL6.Application.Contract.Users;
using PBL6.Application.Contract.Workspaces;
using PBL6.Application.Services;
using PBL6.Common;
using PBL6.Domain.Data;
using PBL6.Infrastructure.Data;

namespace PBL6.API.Extensions
{
    public static class InfrastructureServices
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton<StartupState>();
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<IExampleService, ExampleService>();
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<ICurrentUserService, CurrentUserService>();
            services.AddScoped<IWorkspaceService, WorkspaceService>();

            services.AddTransient<IMailService, MailService>();
            services.AddTransient<IFileService, FileService>();

            return services;
        }
    }
}