using BuildConnect.Model;
using BuildConnect.Repository.Common;
using BuildConnect.Service.Common;

namespace BuildConnect.Service;

public sealed class JobService : IJobService
{
    private readonly IJobRepository _jobRepository;
    private readonly IBidRepository _bidRepository;

    public JobService(IJobRepository jobRepository, IBidRepository bidRepository)
    {
        _jobRepository = jobRepository;
        _bidRepository = bidRepository;
    }

    public IReadOnlyCollection<JobResponse> GetJobs(GetJobsRequest? request = null)
    {
        var normalizedRequest = NormalizeRequest(request);

        return _jobRepository
            .GetAll(normalizedRequest)
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
            DateTimeOffset.UtcNow,
            JobStatuses.Open);

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

        if (!CanEditJob(existingJob.Status))
        {
            throw new ArgumentException("Oglas nije moguce uredjivati u trenutnom statusu.");
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
            existingJob.CreatedAt,
            existingJob.Status);

        var savedJob = _jobRepository.Update(updatedJob);
        return savedJob is null ? null : MapToResponse(savedJob);
    }

    public JobResponse? CloseJob(string id, RequestUserContext userContext)
    {
        var existingJob = GetOwnedJob(id, userContext, "zatvoriti");
        if (existingJob is null)
        {
            return null;
        }

        if (!string.Equals(existingJob.Status, JobStatuses.Open, StringComparison.Ordinal))
        {
            throw new ArgumentException("Zatvoriti mozete samo otvoreni oglas.");
        }

        EnsureJobDoesNotHaveAcceptedBid(existingJob.Id, "Oglas s prihvacenom ponudom nije moguce zatvoriti.");

        var closedJob = _jobRepository.UpdateStatus(existingJob.Id, JobStatuses.Closed);
        return closedJob is null ? null : MapToResponse(closedJob);
    }

    public JobResponse? CompleteJob(string id, RequestUserContext userContext)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return null;
        }

        ValidateContractorContext(userContext);

        var existingJob = _jobRepository.GetById(id.Trim());
        if (existingJob is null)
        {
            return null;
        }

        if (!string.Equals(existingJob.Status, JobStatuses.InProgress, StringComparison.Ordinal))
        {
            throw new ArgumentException("Posao mozete zavrsiti tek kada je u tijeku.");
        }

        var acceptedBid = _bidRepository
            .GetByJobId(existingJob.Id)
            .FirstOrDefault(bid => string.Equals(bid.Status, BidStatuses.Accepted, StringComparison.Ordinal));

        if (acceptedBid is null)
        {
            throw new ArgumentException("Za ovaj posao nije pronadjena prihvacena ponuda.");
        }

        if (!string.Equals(acceptedBid.ContractorId, userContext.UserId, StringComparison.Ordinal))
        {
            throw new UnauthorizedAccessException("Samo odabrani izvodjac moze oznaciti posao zavrsenim.");
        }

        var completedJob = _jobRepository.UpdateStatus(existingJob.Id, JobStatuses.Completed);
        return completedJob is null ? null : MapToResponse(completedJob);
    }

    public bool DeleteJob(string id, RequestUserContext userContext)
    {
        var existingJob = GetOwnedJob(id, userContext, "obrisati");
        if (existingJob is null)
        {
            return false;
        }

        if (!CanDeleteJob(existingJob.Status))
        {
            throw new ArgumentException("Oglas nije moguce obrisati nakon prihvacene ponude ili zavrsenog projekta.");
        }

        EnsureJobDoesNotHaveAcceptedBid(existingJob.Id, "Oglas s prihvacenom ponudom nije moguce obrisati.");
        return _jobRepository.Delete(existingJob.Id);
    }

    private static void ValidateUserContext(RequestUserContext userContext)
    {
        if (string.IsNullOrWhiteSpace(userContext.UserId))
        {
            throw new UnauthorizedAccessException("Korisnicki identitet nije dostupan.");
        }

        if (!string.Equals(userContext.Role, BuildConnectRoles.Investitor, StringComparison.Ordinal))
        {
            throw new UnauthorizedAccessException("Samo investitori mogu upravljati oglasima.");
        }
    }

    private static void ValidateContractorContext(RequestUserContext userContext)
    {
        if (string.IsNullOrWhiteSpace(userContext.UserId))
        {
            throw new UnauthorizedAccessException("Korisnicki identitet nije dostupan.");
        }

        if (!string.Equals(userContext.Role, BuildConnectRoles.Izvodjac, StringComparison.Ordinal))
        {
            throw new UnauthorizedAccessException("Samo izvodjaci mogu oznaciti posao zavrsenim.");
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

    private static GetJobsRequest NormalizeRequest(GetJobsRequest? request)
    {
        var normalizedQuery = string.IsNullOrWhiteSpace(request?.Query)
            ? null
            : request.Query.Trim();

        var normalizedCategories = request?.Categories?
            .Select(category => category?.Trim())
            .Where(category => !string.IsNullOrWhiteSpace(category))
            .Distinct(StringComparer.Ordinal)
            .Where(category => JobCategories.IsSupported(category!))
            .Cast<string>()
            .ToArray();

        return new GetJobsRequest(
            normalizedQuery,
            normalizedCategories is { Length: > 0 } ? normalizedCategories : null);
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
            job.CreatedAt,
            job.Status);
    }

    private Job? GetOwnedJob(string id, RequestUserContext userContext, string actionName)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return null;
        }

        ValidateUserContext(userContext);

        var existingJob = _jobRepository.GetById(id.Trim());
        if (existingJob is null)
        {
            return null;
        }

        if (!string.Equals(existingJob.InvestitorId, userContext.UserId, StringComparison.Ordinal))
        {
            throw new UnauthorizedAccessException($"Mozete {actionName} samo vlastite oglase.");
        }

        return existingJob;
    }

    private void EnsureJobDoesNotHaveAcceptedBid(string jobId, string message)
    {
        var hasAcceptedBid = _bidRepository
            .GetByJobId(jobId)
            .Any(bid => string.Equals(bid.Status, BidStatuses.Accepted, StringComparison.Ordinal));

        if (hasAcceptedBid)
        {
            throw new ArgumentException(message);
        }
    }

    private static bool CanEditJob(string status)
    {
        return string.Equals(status, JobStatuses.Open, StringComparison.Ordinal) ||
               string.Equals(status, JobStatuses.Closed, StringComparison.Ordinal);
    }

    private static bool CanDeleteJob(string status)
    {
        return string.Equals(status, JobStatuses.Open, StringComparison.Ordinal) ||
               string.Equals(status, JobStatuses.Closed, StringComparison.Ordinal);
    }
}
