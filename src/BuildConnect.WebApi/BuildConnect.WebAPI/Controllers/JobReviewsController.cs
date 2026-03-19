using BuildConnect.Model;
using BuildConnect.Service.Common;
using Microsoft.AspNetCore.Mvc;

namespace BuildConnect.WebAPI.Controllers;

[ApiController]
[Route("api/jobs/{jobId}/reviews")]
public sealed class JobReviewsController : ControllerBase
{
    private const string UserIdHeaderName = "X-User-Id";
    private const string UserRoleHeaderName = "X-User-Role";

    private readonly IReviewService _reviewService;

    public JobReviewsController(IReviewService reviewService)
    {
        _reviewService = reviewService;
    }

    [HttpPost]
    [ProducesResponseType(typeof(ReviewResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public ActionResult<ReviewResponse> Post(string jobId, [FromBody] CreateReviewRequest request)
    {
        if (!TryBuildRequestUserContext(out var userContext, out var errorResult))
        {
            return errorResult!;
        }

        try
        {
            var createdReview = _reviewService.CreateReview(jobId, request, userContext);
            return Created($"/api/reviews/{createdReview.Id}", createdReview);
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

    private bool TryBuildRequestUserContext(out RequestUserContext userContext, out ActionResult<ReviewResponse>? errorResult)
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
