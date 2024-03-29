using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using PBL6.Application.Contract.Common;
using PBL6.Common.Functions;

namespace PBL6.Application.Services
{
    public class CurrentUserService : ICurrentUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CurrentUserService(IHttpContextAccessor httpContextAccessor)
        {   
            _httpContextAccessor = httpContextAccessor;
        }

        public string UserId => _httpContextAccessor.HttpContext?.User?.FindFirstValue(CustomClaimTypes.UserId);

        public string Email => _httpContextAccessor.HttpContext?.User?.FindFirstValue(CustomClaimTypes.Email);

        public bool IsAdmin => _httpContextAccessor.HttpContext?.User?.FindFirstValue(CustomClaimTypes.IsAdmin) == "true";
    }
}