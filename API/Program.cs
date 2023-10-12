
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using PBL6.Domain.Data;
using PBL6.Infrastructure.Data;

namespace API;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.

        builder.Services.AddControllers();
        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(c =>
        {
            c.AddSecurityDefinition("basic", new OpenApiSecurityScheme
            {
                Description = "api key.",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.ApiKey,
                Scheme = "basic"
            });

            c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "basic"
                        },
                        In = ParameterLocation.Header
                    },
                    new List<string>()
                }
            });
        });

        // var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
        var keyVaultEndpoint = new Uri(uriString: builder.Configuration["VaultKey"] ?? "https://pbl6.vault.azure.net/");
        var secretClient = new SecretClient(keyVaultEndpoint, new DefaultAzureCredential());
        var connectionString = secretClient.GetSecret("pbl6connectionstring1").Value.Value;
        // Console.WriteLine(connectionString);

        builder.Services.AddDbContext<ApiDbContext>(options =>
            options.UseSqlServer(connectionString)
                .EnableSensitiveDataLogging()
                .EnableDetailedErrors());

        builder.Services.AddScoped<IUnitOfwork, UnitOfWork>();

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();

        app.UseAuthorization();


        app.MapControllers();

        app.Run();
    }
}