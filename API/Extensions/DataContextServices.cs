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

    }
}