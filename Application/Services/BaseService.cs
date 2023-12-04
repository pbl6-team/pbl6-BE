using AutoMapper;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PBL6.Application.Contract.Common;
using PBL6.Application.Hubs;
using PBL6.Domain.Data;

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
        protected readonly IHubContext<ChatHub> _chatHub;

        public BaseService(
          IServiceProvider serviceProvider
        )
        {
            _mailService = serviceProvider.GetService<IMailService>();
            _config = serviceProvider.GetService<IConfiguration>();
            _logger = serviceProvider.GetService<ILogger<BaseService>>();
            _currentUser = serviceProvider.GetService<ICurrentUserService>();
            _mapper = serviceProvider.GetService<IMapper>();
            _unitOfWork = serviceProvider.GetService<IUnitOfWork>();
            _hostingEnvironment = serviceProvider.GetService<IWebHostEnvironment>();
            _fileService = serviceProvider.GetService<IFileService>();
            _chatHub = serviceProvider.GetService<IHubContext<ChatHub>>();
        }
    }
}