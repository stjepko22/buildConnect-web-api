using BuildConnect.Model;
using BuildConnect.Service.Common;
using Microsoft.AspNetCore.Mvc;

namespace BuildConnect.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyCollection<UserProfileResponse>), StatusCodes.Status200OK)]
    public ActionResult<IReadOnlyCollection<UserProfileResponse>> Get([FromQuery] string? role)
    {
        return Ok(_userService.GetUsers(role));
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(UserProfileResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult<UserProfileResponse> GetById(string id)
    {
        var user = _userService.GetUserById(id);
        if (user is null)
        {
            return NotFound(new { message = "Korisnik nije pronadjen." });
        }

        return Ok(user);
    }
}
