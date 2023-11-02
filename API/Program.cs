using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using PBL6.Application;
using PBL6.Application.Contract.Examples;
using PBL6.Application.Services;
using PBL6.Domain.Data;
using PBL6.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.NewtonsoftJson;
using System.Text.Json.Serialization;
using PBL6.Application.Contract.Users;
using PBL6.API.Middlewares;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Reflection;
using PBL6.Application.Contract.Common;
using PBL6.API.Services;

namespace PBL6.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            var serviceName = typeof(Program).Namespace;
            // Add services to the container.

            builder.Services.AddControllers()
                .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
            builder.Services.AddMvcCore()
                .AddRazorViewEngine();

            builder.Services.Configure<MvcOptions>(options =>
            {
                options.ModelMetadataDetailsProviders.Add(
                    new NewtonsoftJsonValidationMetadataProvider());
            });
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();


            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = false;
                options.SaveToken = true;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(builder.Configuration["Jwt:SecretKey"])),
                    ValidateIssuer = false,
                    ValidateAudience = false
                };
            });

            builder.Services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo
                {
                    Version = "V3",
                    Title = "ToDo API",
                    Description = "An ASP.NET Core Web API for managing ToDo items",
                    TermsOfService = new Uri("http://localhost:5000/#"),
                    Contact = new OpenApiContact
                    {
                        Name = "Contact",
                        Url = new Uri("http://localhost:5000/#")
                    },
                    License = new OpenApiLicense
                    {
                        Name = "License",
                        Url = new Uri("http://localhost:5000/#")
                    }
                });

                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    In = ParameterLocation.Header,
                    Description = "Please enter a valid token",
                    Name = "Authorization",
                    Type = SecuritySchemeType.ApiKey,
                    BearerFormat = "JWT",
                    Scheme = "Bearer"
                });

                options.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type=ReferenceType.SecurityScheme,
                            Id="Bearer"
                        }
                    },
                    new string[]{}
                }
                });
                var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
                options.CustomSchemaIds(x => x.FullName);
            });

            builder.Services.AddAutoMapper(typeof(AutoMapperProfile));
            builder.Services.AddHttpContextAccessor();

            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
            // var keyVaultEndpoint = new Uri(uriString: builder.Configuration["VaultKey"] ?? "https://pbl6.vault.azure.net/");
            // var secretClient = new SecretClient(keyVaultEndpoint, new DefaultAzureCredential());
            // var connectionString = secretClient.GetSecret("pbl6connectionstring1").Value.Value;
            // Console.WriteLine(connectionString);

            builder.Services.AddDbContext<ApiDbContext>(options =>
                options.UseSqlServer(connectionString)
                    .EnableSensitiveDataLogging()
                    .EnableDetailedErrors());

            builder.Services.AddScoped<IUnitOfwork, UnitOfWork>();
            builder.Services.AddScoped<IExampleService, ExampleService>();
            builder.Services.AddScoped<IAuthService, AuthService>();
            builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

            builder.Services.AddTransient<IMailService, MailService>();


            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger(options =>
                {
                    options.SerializeAsV2 = true;
                });
                app.UseSwaggerUI();
            }

            app.UseCustomExceptionHandler();

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}