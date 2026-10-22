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
    [ProducesResponseType(typeof(RegistrationResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public ActionResult<RegistrationResponse> Register([FromBody] RegisterRequest request)
    {
        try
        {
            var registration = _authService.Register(request);
            return Created($"/api/auth/verify-email?email={Uri.EscapeDataString(registration.Email)}", registration);
        }
        catch (ArgumentException exception)
        {
            return BadRequest(new { message = exception.Message });
        }
    }

    [HttpPost("verify-email")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult VerifyEmail([FromBody] VerifyEmailRequest request)
    {
        try
        {
            return _authService.VerifyEmail(request)
                ? NoContent()
                : BadRequest(new { message = "Verifikacijski link nije valjan ili je istekao." });
        }
        catch (ArgumentException exception)
        {
            return BadRequest(new { message = exception.Message });
        }
    }

    [HttpPost("resend-verification-email")]
    [ProducesResponseType(typeof(RegistrationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public ActionResult<RegistrationResponse> ResendVerificationEmail([FromBody] ResendVerificationEmailRequest request)
    {
        try
        {
            return Ok(_authService.ResendVerificationEmail(request));
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
