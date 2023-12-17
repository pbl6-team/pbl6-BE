using System.Net;
using System.Net.Mail;
using System.Text;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using PBL6.Application.Contract.ExternalServices.Mails;

namespace PBL6.Application.ExternalServices
{
    public interface IMailService
    {
        Task<bool> Send(string toEmail, string subject, string template, string jsonData);
        Task<string> RenderPartialToStringAsync<TModel>(string partialName, TModel model);
    }

    public class MailService : IMailService
    {
        private readonly ILogger<MailService> _logger;
        private readonly IConfiguration _configuration;
        private readonly IRazorViewEngine _viewEngine;
        private readonly ITempDataProvider _tempDataProvider;
        private readonly IServiceProvider _serviceProvider;
        SmtpClient smtpClient = null;

        public MailService(
            ILogger<MailService> logger,
            IConfiguration configuration,
            IRazorViewEngine viewEngine,
            ITempDataProvider tempDataProvider,
            IServiceProvider serviceProvider
        )
        {
            _logger = logger;
            _configuration =
                configuration ?? throw new ArgumentNullException(nameof(configuration));
            _viewEngine = viewEngine ?? throw new ArgumentNullException(nameof(viewEngine));
            _tempDataProvider =
                tempDataProvider ?? throw new ArgumentNullException(nameof(tempDataProvider));
            _serviceProvider =
                serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        }

        public async Task<bool> Send(
            string toEmail,
            string subject,
            string template,
            string jsonData
        )
        {
            try
            {
                if (string.IsNullOrEmpty(toEmail))
                {
                    return false;
                }
                string body = await RenderPartialToStringAsync(
                    "Views/Templates/" + template,
                    new MailData() { JsonData = jsonData }
                );
                var mailConfig = _configuration.GetSection("Mail");
                smtpClient ??= new SmtpClient
                {
                    EnableSsl = bool.Parse(mailConfig["Ssl"]),
                    Host = mailConfig["Host"],
                    Port = int.Parse(mailConfig["Port"]),
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential(
                        mailConfig["Email"],
                        mailConfig["Password"]
                    ),
                    DeliveryMethod = SmtpDeliveryMethod.Network
                };
                var msg = new MailMessage
                {
                    IsBodyHtml = true,
                    BodyEncoding = Encoding.UTF8,
                    From = string.IsNullOrEmpty(mailConfig["Name"])
                        ? new MailAddress(mailConfig["Email"])
                        : new MailAddress(mailConfig["Email"], mailConfig["Name"]),
                    Subject = subject,
                    Body = body,
                    Priority = MailPriority.Normal,
                };
                msg.To.Add(toEmail);
                await smtpClient.SendMailAsync(msg);

                return true;
            }
            catch (Exception e)
            {
                _logger.LogError("[SendEmail] Exception {message}", e.Message);
                return false;
            }
        }

        public async Task<string> RenderPartialToStringAsync<TModel>(
            string partialName,
            TModel model
        )
        {
            var actionContext = GetActionContext();
            var partial = FindView(actionContext, partialName);
            using var output = new StringWriter();
            var viewContext = new ViewContext(
                actionContext,
                partial,
                new ViewDataDictionary<TModel>(
                    metadataProvider: new EmptyModelMetadataProvider(),
                    modelState: new ModelStateDictionary()
                )
                {
                    Model = model
                },
                new TempDataDictionary(actionContext.HttpContext, _tempDataProvider),
                output,
                new HtmlHelperOptions()
            );
            await partial.RenderAsync(viewContext);

            return output.ToString();
        }

        private IView FindView(ActionContext actionContext, string partialName)
        {
            var getPartialResult = _viewEngine.GetView(null, partialName, false);
            if (getPartialResult.Success)
            {
                return getPartialResult.View;
            }
            var findPartialResult = _viewEngine.FindView(actionContext, partialName, false);
            if (findPartialResult.Success)
            {
                return findPartialResult.View;
            }
            var searchedLocations = getPartialResult.SearchedLocations.Concat(
                findPartialResult.SearchedLocations
            );
            var errorMessage = string.Join(
                Environment.NewLine,
                new[]
                {
                    $"Unable to find partial '{partialName}'. The following locations were searched:"
                }.Concat(searchedLocations)
            );
            ;
            throw new InvalidOperationException(errorMessage);
        }

        private ActionContext GetActionContext()
        {
            var httpContext = new DefaultHttpContext { RequestServices = _serviceProvider };

            return new ActionContext(httpContext, new RouteData(), new ActionDescriptor());
        }
    }
}
