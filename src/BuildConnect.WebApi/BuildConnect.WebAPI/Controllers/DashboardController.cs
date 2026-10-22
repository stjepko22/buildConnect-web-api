using BuildConnect.Model;
using BuildConnect.Service.Common;
using BuildConnect.WebAPI.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BuildConnect.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public sealed class DashboardController : ControllerBase
{
    private readonly IDashboardService _dashboardService;

    public DashboardController(IDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(DashboardResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public ActionResult<DashboardResponse> Get()
    {
        var userContext = User.ToRequestUserContext();
        if (userContext is null)
        {
            return Unauthorized(new { message = "Korisnicki identitet nije dostupan u tokenu." });
        }

        return Ok(_dashboardService.GetDashboard(userContext));
    }
}
