using BuildConnect.Model;
using BuildConnect.Repository.Common;
using BuildConnect.Service.Common;

namespace BuildConnect.Service;

public sealed class JobService : IJobService
{
    private readonly IJobRepository _jobRepository;

    public JobService(IJobRepository jobRepository)
    {
        _jobRepository = jobRepository;
    }

    public IReadOnlyCollection<JobResponse> GetJobs()
    {
        return _jobRepository
            .GetAll()
            .Select(MapToResponse)
            .ToArray();
    }

    public JobResponse? GetJobById(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return null;
        }

        var job = _jobRepository.GetById(id.Trim());
        return job is null ? null : MapToResponse(job);
    }

    public JobResponse CreateJob(CreateJobRequest request, RequestUserContext userContext)
    {
        ValidateUserContext(userContext);
        ValidateRequest(
            request.Title,
            request.Description,
            request.Category,
            request.Location,
            request.Budget,
            request.Deadline);

        var job = new Job(
            Guid.NewGuid().ToString("N"),
            request.Title.Trim(),
            request.Description.Trim(),
            request.Category.Trim(),
            request.Location.Trim(),
            request.Budget,
            request.Deadline.Trim(),
            userContext.UserId,
            DateTimeOffset.UtcNow);

        var createdJob = _jobRepository.Create(job);
        return MapToResponse(createdJob);
    }

    public JobResponse? UpdateJob(string id, UpdateJobRequest request, RequestUserContext userContext)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return null;
        }

        ValidateUserContext(userContext);
        ValidateRequest(
            request.Title,
            request.Description,
            request.Category,
            request.Location,
            request.Budget,
            request.Deadline);

        var existingJob = _jobRepository.GetById(id.Trim());
        if (existingJob is null)
        {
            return null;
        }

        if (!string.Equals(existingJob.InvestitorId, userContext.UserId, StringComparison.Ordinal))
        {
            throw new UnauthorizedAccessException("Mozete uredjivati samo vlastite oglase.");
        }

        var updatedJob = new Job(
            existingJob.Id,
            request.Title.Trim(),
            request.Description.Trim(),
            request.Category.Trim(),
            request.Location.Trim(),
            request.Budget,
            request.Deadline.Trim(),
            existingJob.InvestitorId,
            existingJob.CreatedAt);

        var savedJob = _jobRepository.Update(updatedJob);
        return savedJob is null ? null : MapToResponse(savedJob);
    }

    private static void ValidateUserContext(RequestUserContext userContext)
    {
        if (string.IsNullOrWhiteSpace(userContext.UserId))
        {
            throw new UnauthorizedAccessException("Korisnicki identitet nije dostupan.");
        }

        if (!string.Equals(userContext.Role, BuildConnectRoles.Investitor, StringComparison.Ordinal))
        {
            throw new UnauthorizedAccessException("Samo investitori mogu kreirati oglas.");
        }
    }

    private static void ValidateRequest(
        string title,
        string description,
        string category,
        string location,
        decimal? budget,
        string deadline)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            throw new ArgumentException("Naslov oglasa je obavezan.");
        }

        if (string.IsNullOrWhiteSpace(description))
        {
            throw new ArgumentException("Opis oglasa je obavezan.");
        }

        if (string.IsNullOrWhiteSpace(location))
        {
            throw new ArgumentException("Lokacija je obavezna.");
        }

        if (string.IsNullOrWhiteSpace(deadline))
        {
            throw new ArgumentException("Rok zavrsetka je obavezan.");
        }

        if (string.IsNullOrWhiteSpace(category) || !JobCategories.IsSupported(category.Trim()))
        {
            throw new ArgumentException("Odabrana kategorija nije podrzana.");
        }

        if (budget is <= 0)
        {
            throw new ArgumentException("Budzet mora biti veci od 0 ako je definiran.");
        }
    }

    private static JobResponse MapToResponse(Job job)
    {
        return new JobResponse(
            job.Id,
            job.Title,
            job.Description,
            job.Category,
            job.Location,
            job.Budget,
            job.Deadline,
            job.InvestitorId,
            job.CreatedAt);
    }
}
