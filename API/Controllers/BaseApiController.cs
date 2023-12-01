using Microsoft.AspNetCore.Mvc;
using PBL6.Application.Filters;

namespace PBL6.API.Controllers
{
    [Route("api/[controller]")]
    [Produces("application/json")]
    [TypeFilter(typeof(ApiKeyFilterAttribute))]
    [ApiController]
    public class BaseApiController : ControllerBase
    {
    }
}