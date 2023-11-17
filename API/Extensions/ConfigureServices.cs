using System.Reflection;
using System.Text;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.NewtonsoftJson;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using PBL6.Application;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace PBL6.API.Extensions
{
    public static class ConfigureServices
    {
        public static IServiceCollection AddApiServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddControllers()
                            .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
            services.AddMvcCore()
                .AddRazorViewEngine();
            services.Configure<MvcOptions>(options =>
            {
                options.ModelMetadataDetailsProviders.Add(
                    new NewtonsoftJsonValidationMetadataProvider());
            });

            services.AddCors(options =>
            {
                var allowHostsConfig = configuration["AllowedHosts"] ?? "";
                options.AddPolicy("CorsPolicy", 
                builder =>
                {
                    if (allowHostsConfig.Equals("*"))
                    {
                        builder.SetIsOriginAllowed((origin) => true)
                            .AllowAnyMethod()
                            .AllowAnyHeader()
                            .AllowCredentials();
                    }
                    else
                    {
                        var allowHosts = allowHostsConfig.Split(";");
                        builder.WithOrigins(allowHosts)
                            .AllowAnyOrigin()
                            .AllowAnyMethod()
                            .AllowAnyHeader();
                    }
                });
            });

            services.AddEndpointsApiExplorer();
            services.AddAutoMapper(typeof(AutoMapperProfile));
            services.AddHttpContextAccessor();

            // Add Swagger
            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo
                {
                    Version = "v1",
                    Title = "Fira API",
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
                        Array.Empty<string>()
                    }
                });
                var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
                options.CustomSchemaIds(x => x.FullName);
                options.OperationFilter<AddRequiredHeaderParameter>();
            });

            return services;
        }

    }

    public class AddRequiredHeaderParameter : IOperationFilter
    {
        void IOperationFilter.Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            operation.Parameters ??= new List<OpenApiParameter>();
            operation.Parameters.Add(new OpenApiParameter
            {
                Name = "x-apikey",
                In = ParameterLocation.Header,
                Description = "API key để xác thực nguồn gọi API",
                Schema = new OpenApiSchema() { Type = "string", Default = new OpenApiString("5J0jCR1dAkvDt3YVoahpux0eawahkQB9") },
                Required = true
            });
        }
    }
}