using AutoMapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PBL6.Application.Contract.Common;

namespace PBL6.Application.Services
{
    public class BaseService
    {
        protected readonly IMailService _mailService;
        protected readonly IConfiguration _config;
        protected readonly ICurrentUserService _currentUser;
        protected readonly IMapper _mapper;

        public BaseService(
          IServiceProvider serviceProvider
        )
        {
            _mailService = serviceProvider.GetService<IMailService>();
            _config = serviceProvider.GetService<IConfiguration>();
            _currentUser = serviceProvider.GetService<ICurrentUserService>();
            _mapper = serviceProvider.GetService<IMapper>();
        }
    }
}