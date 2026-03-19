using BuildConnect.Model;
using BuildConnect.Service.Common;
using Microsoft.AspNetCore.Mvc;

namespace BuildConnect.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class BidsController : ControllerBase
{
    private const string UserIdHeaderName = "X-User-Id";
    private const string UserRoleHeaderName = "X-User-Role";
    private readonly IBidService _bidService;

    public BidsController(IBidService bidService)
    {
        _bidService = bidService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyCollection<BidResponse>), StatusCodes.Status200OK)]
    public ActionResult<IReadOnlyCollection<BidResponse>> Get([FromQuery] string? jobId, [FromQuery] string? contractorId)
    {
        return Ok(_bidService.GetBids(jobId, contractorId));
    }

    [HttpPost("{bidId}/accept")]
    [ProducesResponseType(typeof(IReadOnlyCollection<BidResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public ActionResult<IReadOnlyCollection<BidResponse>> Accept(string bidId)
    {
        if (!TryBuildRequestUserContext(out var userContext, out var errorResult))
        {
            return errorResult!;
        }

        try
        {
            return Ok(_bidService.AcceptBid(bidId, userContext));
        }
        catch (ArgumentException exception)
        {
            return BadRequest(new { message = exception.Message });
        }
        catch (UnauthorizedAccessException exception)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { message = exception.Message });
        }
    }

    private bool TryBuildRequestUserContext(out RequestUserContext userContext, out ActionResult<IReadOnlyCollection<BidResponse>>? errorResult)
    {
        var userId = Request.Headers[UserIdHeaderName].ToString();
        var userRole = Request.Headers[UserRoleHeaderName].ToString();

        if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(userRole))
        {
            userContext = default!;
            errorResult = Unauthorized(new { message = "Nedostaje korisnicki kontekst u zaglavljima zahtjeva." });
            return false;
        }

        userContext = new RequestUserContext(userId.Trim(), userRole.Trim());
        errorResult = null;
        return true;
    }
}
