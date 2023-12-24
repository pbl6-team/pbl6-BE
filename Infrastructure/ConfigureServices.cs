using System.Net.Http.Headers;
using System.Text;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PBL6.Domain.Data;
using PBL6.Infrastructure.Data;

namespace PBL6.Infrastructure
{
    public static class DataContextServices
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection");
            services.AddDbContext<ApiDbContext>(options =>
                options.UseSqlServer(connectionString)
                    .EnableSensitiveDataLogging()
                    .EnableDetailedErrors());
            services.AddScoped<IUnitOfWork, UnitOfWork>();

             services.AddHttpClient("Meeting", client =>
            {
                client.BaseAddress = new Uri(configuration.GetValue<string>("Meeting:ServerUrl"));
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.ASCII.GetBytes($"OPENVIDUAPP:{configuration.GetValue<string>("Meeting:SecretKey")}")));
            });

            return services;
        }

        public static async Task<IApplicationBuilder> UseItToSeedSqlServerAsync(this IApplicationBuilder app)
        {
            ArgumentNullException.ThrowIfNull(app, nameof(app));

            using var scope = app.ApplicationServices.CreateScope();
            var services = scope.ServiceProvider;
            try
            {
                var context = services.GetRequiredService<ApiDbContext>();
                context.Database.Migrate();
                await DbInitializer.Initialize(context);
            }
            catch (Exception)
            {

            }

            return app;
        }

    }
}