using API.Helpers;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ServiceFilter<LogUserActivity>]
[ApiController]
[Route("api/[controller]")]
public abstract class BaseApiController : ControllerBase
{
}
