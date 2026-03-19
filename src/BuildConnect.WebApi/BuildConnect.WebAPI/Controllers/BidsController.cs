using BuildConnect.Model;
using BuildConnect.Service.Common;
using BuildConnect.WebAPI.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BuildConnect.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class BidsController : ControllerBase
{
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
    [Authorize]
    [ProducesResponseType(typeof(IReadOnlyCollection<BidResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public ActionResult<IReadOnlyCollection<BidResponse>> Accept(string bidId)
    {
        var userContext = User.ToRequestUserContext();
        if (userContext is null)
        {
            return Unauthorized(new { message = "Korisnicki identitet nije dostupan u tokenu." });
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
}
