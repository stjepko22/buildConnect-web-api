using BuildConnect.Model;
using BuildConnect.Service.Common;
using BuildConnect.WebAPI.Authentication;
using Microsoft.AspNetCore.Authorization;
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

    [HttpPut("me")]
    [Authorize]
    [ProducesResponseType(typeof(UserProfileResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult<UserProfileResponse> UpdateCurrentUser([FromBody] UpdateUserProfileRequest request)
    {
        var userContext = User.ToRequestUserContext();
        if (userContext is null)
        {
            return Unauthorized(new { message = "Korisnicki identitet nije dostupan u tokenu." });
        }

        try
        {
            return Ok(_userService.UpdateCurrentUserProfile(request, userContext));
        }
        catch (ArgumentException exception)
        {
            return BadRequest(new { message = exception.Message });
        }
        catch (KeyNotFoundException exception)
        {
            return NotFound(new { message = exception.Message });
        }
    }
}
