using BuildConnect.Model;
using BuildConnect.Service.Common;
using Microsoft.AspNetCore.Mvc;

namespace BuildConnect.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class JobsController : ControllerBase
{
    private const string UserIdHeaderName = "X-User-Id";
    private const string UserRoleHeaderName = "X-User-Role";

    private readonly IJobService _jobService;

    public JobsController(IJobService jobService)
    {
        _jobService = jobService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyCollection<JobResponse>), StatusCodes.Status200OK)]
    public ActionResult<IReadOnlyCollection<JobResponse>> Get()
    {
        return Ok(_jobService.GetJobs());
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(JobResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult<JobResponse> GetById(string id)
    {
        var job = _jobService.GetJobById(id);
        if (job is null)
        {
            return NotFound(new { message = "Posao nije pronadjen." });
        }

        return Ok(job);
    }

    [HttpPost]
    [ProducesResponseType(typeof(JobResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public ActionResult<JobResponse> Post([FromBody] CreateJobRequest request)
    {
        if (!TryBuildRequestUserContext(out var userContext, out var errorResult))
        {
            return errorResult!;
        }

        try
        {
            var createdJob = _jobService.CreateJob(request, userContext);
            return CreatedAtAction(nameof(GetById), new { id = createdJob.Id }, createdJob);
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

    private bool TryBuildRequestUserContext(out RequestUserContext userContext, out ActionResult<JobResponse>? errorResult)
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
