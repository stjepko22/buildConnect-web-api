using BuildConnect.Model;
using BuildConnect.Service.Common;
using Microsoft.AspNetCore.Mvc;

namespace BuildConnect.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class ReviewsController : ControllerBase
{
    private readonly IReviewService _reviewService;

    public ReviewsController(IReviewService reviewService)
    {
        _reviewService = reviewService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyCollection<ReviewResponse>), StatusCodes.Status200OK)]
    public ActionResult<IReadOnlyCollection<ReviewResponse>> Get([FromQuery] string? jobId, [FromQuery] string? revieweeId)
    {
        return Ok(_reviewService.GetReviews(jobId, revieweeId));
    }
}
