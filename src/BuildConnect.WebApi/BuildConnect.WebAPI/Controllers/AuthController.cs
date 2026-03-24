using BuildConnect.Model;
using BuildConnect.Service.Common;
using BuildConnect.WebAPI.Authentication;
using Microsoft.AspNetCore.Mvc;

namespace BuildConnect.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly IJwtTokenService _jwtTokenService;

    public AuthController(IAuthService authService, IJwtTokenService jwtTokenService)
    {
        _authService = authService;
        _jwtTokenService = jwtTokenService;
    }

    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthenticatedSessionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public ActionResult<AuthenticatedSessionResponse> Login([FromBody] LoginRequest request)
    {
        try
        {
            var user = _authService.Login(request);
            return Ok(BuildSessionResponse(user));
        }
        catch (ArgumentException exception)
        {
            return BadRequest(new { message = exception.Message });
        }
        catch (UnauthorizedAccessException exception)
        {
            return Unauthorized(new { message = exception.Message });
        }
    }

    [HttpPost("register")]
    [ProducesResponseType(typeof(AuthenticatedSessionResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public ActionResult<AuthenticatedSessionResponse> Register([FromBody] RegisterRequest request)
    {
        try
        {
            var user = _authService.Register(request);
            return Created($"/api/users/{user.Id}", BuildSessionResponse(user));
        }
        catch (ArgumentException exception)
        {
            return BadRequest(new { message = exception.Message });
        }
    }

    private AuthenticatedSessionResponse BuildSessionResponse(AuthenticatedUserResponse user)
    {
        return new AuthenticatedSessionResponse(_jwtTokenService.CreateToken(user), user);
    }
}
