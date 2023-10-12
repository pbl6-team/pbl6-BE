using Microsoft.AspNetCore.Mvc;
using PBL6.API.Filters;

namespace PBL6.API.Controllers
{
    [Route("api/[controller]")]
    // [TypeFilter(typeof(AuthorizationFilterAttribute))]
    [ApiController]
    public class BaseApiController : ControllerBase
    {
    }
}