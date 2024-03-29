using AutoMapper;
using Hangfire;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PBL6.Application.Contract.Common;
using PBL6.Application.ExternalServices;
using PBL6.Domain.Data;
using IExternalNotificationService = PBL6.Application.Contract.ExternalServices.Notifications.INotificationService; 

namespace PBL6.Application.Services
{
    public class BaseService
    {
        protected readonly IMailService _mailService;
        protected readonly IConfiguration _config;
        protected readonly ICurrentUserService _currentUser;
        protected readonly IMapper _mapper;
        protected readonly ILogger<BaseService> _logger;
        protected readonly IUnitOfWork _unitOfWork;
        protected readonly IWebHostEnvironment _hostingEnvironment;
        protected readonly IFileService _fileService;
        protected readonly IBackgroundJobClient _backgroundJobClient;
        protected readonly IExternalNotificationService _notificationService;
        protected readonly IHubService _hubService;

        public BaseService(IServiceProvider serviceProvider)
        {
            _mailService = serviceProvider.GetService<IMailService>();
            _config = serviceProvider.GetService<IConfiguration>();
            _logger = serviceProvider.GetService<ILogger<BaseService>>();
            _currentUser = serviceProvider.GetService<ICurrentUserService>();
            _mapper = serviceProvider.GetService<IMapper>();
            _unitOfWork = serviceProvider.GetService<IUnitOfWork>();
            _hostingEnvironment = serviceProvider.GetService<IWebHostEnvironment>();
            _fileService = serviceProvider.GetService<IFileService>();
            _backgroundJobClient = serviceProvider.GetService<IBackgroundJobClient>();
            _notificationService = serviceProvider.GetService<IExternalNotificationService>();
            _hubService = serviceProvider.GetService<IHubService>();
        }
    }
}
