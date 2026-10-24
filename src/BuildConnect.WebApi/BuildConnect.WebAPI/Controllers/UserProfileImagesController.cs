using BuildConnect.Model;
using BuildConnect.Service.Common;
using BuildConnect.WebAPI.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BuildConnect.WebAPI.Controllers;

[ApiController]
[Route("api/users")]
public sealed class UserProfileImagesController : ControllerBase
{
    private readonly IUserProfileImageService _imageService;

    public UserProfileImagesController(IUserProfileImageService imageService)
    {
        _imageService = imageService;
    }

    [HttpGet("{id}/images/{kind}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetAsync(string id, string kind, [FromQuery] string? v, CancellationToken cancellationToken)
    {
        try
        {
            var image = await _imageService.GetAsync(id, kind, cancellationToken);
            if (image is null)
            {
                return NotFound(new { message = "Fotografija nije pronadjena." });
            }

            Response.Headers.XContentTypeOptions = "nosniff";
            // Do not cache a versionless URL or an outdated URL after replacement.
            Response.Headers.CacheControl = string.Equals(v, image.Version, StringComparison.Ordinal)
                ? "public, max-age=3600"
                : "no-cache";
            return File(image.Content, image.ContentType);
        }
        catch (ArgumentException exception)
        {
            return BadRequest(new { message = exception.Message });
        }
    }

    [HttpPost("me/images/{kind}")]
    [Authorize]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(UserProfileImageLimits.MaxFileSize + 64 * 1024)]
    [RequestFormLimits(MultipartBodyLengthLimit = UserProfileImageLimits.MaxFileSize + 64 * 1024)]
    [ProducesResponseType(typeof(UserProfileResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status413PayloadTooLarge)]
    public async Task<ActionResult<UserProfileResponse>> UploadAsync(string kind, [FromForm] IFormFile? file, CancellationToken cancellationToken)
    {
        var userContext = User.ToRequestUserContext();
        if (userContext is null)
        {
            return Unauthorized(new { message = "Korisnicki identitet nije dostupan u tokenu." });
        }

        if (file is null)
        {
            return BadRequest(new { message = "Odaberite fotografiju za prijenos." });
        }

        try
        {
            await using var content = file.OpenReadStream();
            return Ok(await _imageService.UploadCurrentUserImageAsync(kind, content, file.ContentType, userContext, cancellationToken));
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

    [HttpDelete("me/images/{kind}")]
    [Authorize]
    [ProducesResponseType(typeof(UserProfileResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserProfileResponse>> DeleteAsync(string kind, CancellationToken cancellationToken)
    {
        var userContext = User.ToRequestUserContext();
        if (userContext is null)
        {
            return Unauthorized(new { message = "Korisnicki identitet nije dostupan u tokenu." });
        }

        try
        {
            return Ok(await _imageService.DeleteCurrentUserImageAsync(kind, userContext, cancellationToken));
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
