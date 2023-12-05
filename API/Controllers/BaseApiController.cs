using Microsoft.AspNetCore.Mvc;
using PBL6.Application.Filters;

namespace PBL6.API.Controllers
{
    [Route("api/[controller]")]
    [Produces("application/json")]
    [TypeFilter(typeof(ApiKeyFilterAttribute))]
    [ApiController]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public class BaseApiController : ControllerBase { }
}
