using PBL6.API.Middlewares;
using PBL6.API.Extensions;
using PBL6.Common;
using PBL6.Application.SignalR.ChatHub;

namespace PBL6.API
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services
                .AddApiServices(builder.Configuration)
                .AddInfrastructureServices(builder.Configuration)
                .AddDataContextServices(builder.Configuration);

            var app = builder.Build();
            StartupState.Instance.Services = app.Services;
            StartupState.Instance.Configuration = builder.Configuration;
            
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger(options =>
                {
                    options.SerializeAsV2 = true;
                });
                app.UseSwaggerUI();
                await app.UseItToSeedSqlServerAsync();
            }
            else
            {
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseCustomExceptionHandler();
            app.UseStaticFiles();
            app.UseHttpsRedirection();
            app.UseAuthorization();
            app.Map("/", () => Results.Redirect("/swagger"));
            app.MapControllers();
            app.UseCors("CorsPolicy");
            app.MapHub<ChatHub>("/chatHub");

            app.Run();
        }
    }
}