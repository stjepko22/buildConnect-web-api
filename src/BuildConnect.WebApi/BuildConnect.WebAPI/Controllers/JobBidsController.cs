using BuildConnect.Model;
using BuildConnect.Service.Common;
using Microsoft.AspNetCore.Mvc;

namespace BuildConnect.WebAPI.Controllers;

[ApiController]
[Route("api/jobs/{jobId}/bids")]
public sealed class JobBidsController : ControllerBase
{
    private const string UserIdHeaderName = "X-User-Id";
    private const string UserRoleHeaderName = "X-User-Role";
    private const string UserDisplayNameHeaderName = "X-User-Display-Name";

    private readonly IBidService _bidService;

    public JobBidsController(IBidService bidService)
    {
        _bidService = bidService;
    }

    [HttpPost]
    [ProducesResponseType(typeof(BidResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public ActionResult<BidResponse> Post(string jobId, [FromBody] CreateBidRequest request)
    {
        if (!TryBuildRequestUserContext(out var userContext, out var errorResult))
        {
            return errorResult!;
        }

        try
        {
            var createdBid = _bidService.CreateBid(jobId, request, userContext);
            return Created($"/api/bids/{createdBid.Id}", createdBid);
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

    private bool TryBuildRequestUserContext(out RequestUserContext userContext, out ActionResult<BidResponse>? errorResult)
    {
        var userId = Request.Headers[UserIdHeaderName].ToString();
        var userRole = Request.Headers[UserRoleHeaderName].ToString();
        var userDisplayName = Request.Headers[UserDisplayNameHeaderName].ToString();

        if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(userRole))
        {
            userContext = default!;
            errorResult = Unauthorized(new { message = "Nedostaje korisnicki kontekst u zaglavljima zahtjeva." });
            return false;
        }

        userContext = new RequestUserContext(
            userId.Trim(),
            userRole.Trim(),
            string.IsNullOrWhiteSpace(userDisplayName) ? null : userDisplayName.Trim());
        errorResult = null;
        return true;
    }
}
