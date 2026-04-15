using BuildConnect.Model;
using BuildConnect.Service.Common;
using BuildConnect.WebAPI.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BuildConnect.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class JobsController : ControllerBase
{
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
    [Authorize]
    [ProducesResponseType(typeof(JobResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public ActionResult<JobResponse> Post([FromBody] CreateJobRequest request)
    {
        var userContext = User.ToRequestUserContext();
        if (userContext is null)
        {
            return Unauthorized(new { message = "Korisnicki identitet nije dostupan u tokenu." });
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

    [HttpPut("{id}")]
    [Authorize]
    [ProducesResponseType(typeof(JobResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult<JobResponse> Put(string id, [FromBody] UpdateJobRequest request)
    {
        var userContext = User.ToRequestUserContext();
        if (userContext is null)
        {
            return Unauthorized(new { message = "Korisnicki identitet nije dostupan u tokenu." });
        }

        try
        {
            var updatedJob = _jobService.UpdateJob(id, request, userContext);
            if (updatedJob is null)
            {
                return NotFound(new { message = "Posao nije pronadjen." });
            }

            return Ok(updatedJob);
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
