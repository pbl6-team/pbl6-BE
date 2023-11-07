using Microsoft.EntityFrameworkCore;
using PBL6.Infrastructure.Data;

namespace PBL6.API.Extensions
{
    public static class DataContextServices
    {
        public static IServiceCollection AddDataContextServices(this IServiceCollection services, IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection");
            services.AddDbContext<ApiDbContext>(options =>
                options.UseSqlServer(connectionString)
                    .EnableSensitiveDataLogging()
                    .EnableDetailedErrors());

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