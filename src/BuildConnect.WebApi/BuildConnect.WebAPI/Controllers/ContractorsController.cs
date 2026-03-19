using BuildConnect.Model;
using BuildConnect.Service.Common;
using Microsoft.AspNetCore.Mvc;

namespace BuildConnect.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class ContractorsController : ControllerBase
{
    private readonly IUserService _userService;

    public ContractorsController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyCollection<UserProfileResponse>), StatusCodes.Status200OK)]
    public ActionResult<IReadOnlyCollection<UserProfileResponse>> Get()
    {
        return Ok(_userService.GetContractors());
    }
}
