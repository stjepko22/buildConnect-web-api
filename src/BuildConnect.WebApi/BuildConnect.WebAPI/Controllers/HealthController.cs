using BuildConnect.Model;
using BuildConnect.Service.Common;
using Microsoft.AspNetCore.Mvc;

namespace BuildConnect.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class HealthController : ControllerBase
{
    private readonly IApiStatusService _apiStatusService;

    public HealthController(IApiStatusService apiStatusService)
    {
        _apiStatusService = apiStatusService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiStatus), StatusCodes.Status200OK)]
    public ActionResult<ApiStatus> Get()
    {
        return Ok(_apiStatusService.GetStatus());
    }
}
