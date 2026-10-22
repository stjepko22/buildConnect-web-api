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
    public ActionResult<IReadOnlyCollection<JobResponse>> Get(
        [FromQuery(Name = "q")] string? query,
        [FromQuery(Name = "category")] string[]? categories)
    {
        return Ok(_jobService.GetJobs(new GetJobsRequest(query, categories)));
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

    [HttpPost("{id}/close")]
    [Authorize]
    [ProducesResponseType(typeof(JobResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult<JobResponse> Close(string id)
    {
        var userContext = User.ToRequestUserContext();
        if (userContext is null)
        {
            return Unauthorized(new { message = "Korisnicki identitet nije dostupan u tokenu." });
        }

        try
        {
            var closedJob = _jobService.CloseJob(id, userContext);
            if (closedJob is null)
            {
                return NotFound(new { message = "Posao nije pronadjen." });
            }

            return Ok(closedJob);
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

    [HttpPost("{id}/complete")]
    [Authorize]
    [ProducesResponseType(typeof(JobResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult<JobResponse> Complete(string id)
    {
        var userContext = User.ToRequestUserContext();
        if (userContext is null)
        {
            return Unauthorized(new { message = "Korisnicki identitet nije dostupan u tokenu." });
        }

        try
        {
            var completedJob = _jobService.CompleteJob(id, userContext);
            if (completedJob is null)
            {
                return NotFound(new { message = "Posao nije pronadjen." });
            }

            return Ok(completedJob);
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

    [HttpDelete("{id}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult Delete(string id)
    {
        var userContext = User.ToRequestUserContext();
        if (userContext is null)
        {
            return Unauthorized(new { message = "Korisnicki identitet nije dostupan u tokenu." });
        }

        try
        {
            var isDeleted = _jobService.DeleteJob(id, userContext);
            if (!isDeleted)
            {
                return NotFound(new { message = "Posao nije pronadjen." });
            }

            return NoContent();
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
