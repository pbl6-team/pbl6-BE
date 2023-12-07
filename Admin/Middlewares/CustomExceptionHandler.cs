using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using PBL6.Common.Exceptions;

namespace PBL6.Admin.Middlewares
{
    public static class CustomExceptionHandler
    {
        public static void UseCustomExceptionHandler(this IApplicationBuilder app)
        {
            app.UseExceptionHandler(exceptionHandlerApp =>
            {
                exceptionHandlerApp.Run(async context =>
                {
                    var response = context.Response;
                    response.ContentType = "application/json";
                    var exception = context.Features.Get<IExceptionHandlerFeature>()?.Error;
                    var message = exception.Message;

                    response.StatusCode = exception switch
                    {
                        CustomException => (exception as CustomException).StatusCode,
                        _ => (int)HttpStatusCode.InternalServerError,
                    };

                    var isDevelopment = (app as WebApplication).Environment.IsDevelopment();

                    var pd = new ProblemDetails
                    {
                        Title = isDevelopment ? message : "An error occurred on the server.",
                        Status = response.StatusCode,
                        Detail = isDevelopment ? exception?.StackTrace : null,
                        Type = response.StatusCode == ((int)HttpStatusCode.InternalServerError) ? "https://datatracker.ietf.org/doc/html/rfc7231#section-6.6.1" : "https://datatracker.ietf.org/doc/html/rfc7231#section-6.5"
                    };

                    await response.WriteAsync(JsonSerializer.Serialize(pd));
                });
            });
        }

    }
}