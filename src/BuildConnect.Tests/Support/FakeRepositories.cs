using BuildConnect.Model;
using BuildConnect.Repository.Common;
using BuildConnect.Service.Common;

namespace BuildConnect.Tests.Support;

internal sealed class FakeAuthRepository : IAuthRepository
{
    private readonly List<AuthAccount> _accounts = [];

    public FakeAuthRepository(params IEnumerable<AuthAccount>[] accountCollections)
    {
        foreach (var accountCollection in accountCollections)
        {
            _accounts.AddRange(accountCollection);
        }
    }

    public AuthAccount? GetByEmail(string email)
    {
        return _accounts.FirstOrDefault(account => string.Equals(account.Email, email, StringComparison.OrdinalIgnoreCase));
    }
}

internal sealed class FakeUserRepository : IUserRepository
{
    private readonly List<UserProfile> _users = [];
    private readonly Dictionary<string, string> _passwordHashes = new(StringComparer.Ordinal);

    public FakeUserRepository(params IEnumerable<UserProfile>[] userCollections)
    {
        foreach (var userCollection in userCollections)
        {
            _users.AddRange(userCollection);
        }
    }

    public IReadOnlyCollection<UserProfile> GetAll()
    {
        return _users.OrderBy(user => user.DisplayName, StringComparer.Ordinal).ToArray();
    }

    public UserProfile? GetById(string id)
    {
        return _users.FirstOrDefault(user => string.Equals(user.Id, id, StringComparison.Ordinal));
    }

    public UserProfile? GetByEmail(string email)
    {
        return _users.FirstOrDefault(user => string.Equals(user.Email, email, StringComparison.OrdinalIgnoreCase));
    }

    public IReadOnlyCollection<UserProfile> GetByRole(string role)
    {
        return _users
            .Where(user => string.Equals(user.Role, role, StringComparison.Ordinal))
            .OrderBy(user => user.DisplayName, StringComparer.Ordinal)
            .ToArray();
    }

    public IReadOnlyCollection<UserProfile> GetContractors(GetContractorsRequest? request = null)
    {
        IEnumerable<UserProfile> users = _users.Where(user => string.Equals(user.Role, BuildConnectRoles.Izvodjac, StringComparison.Ordinal));
        var query = request?.Query?.Trim();

        if (!string.IsNullOrWhiteSpace(query))
        {
            users = users.Where(user =>
                user.DisplayName.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                user.Bio.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                user.Location.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                (user.ServiceCategories?.Any(category => category.Contains(query, StringComparison.OrdinalIgnoreCase)) ?? false));
        }

        if (!string.IsNullOrWhiteSpace(request?.Location))
        {
            users = users.Where(user => string.Equals(user.Location, request.Location, StringComparison.Ordinal));
        }

        if (!string.IsNullOrWhiteSpace(request?.LegalType))
        {
            users = users.Where(user => string.Equals(user.LegalType, request.LegalType, StringComparison.Ordinal));
        }

        var categories = request?.Categories?
            .Where(category => !string.IsNullOrWhiteSpace(category))
            .Select(category => category.Trim())
            .ToArray();

        if (categories is { Length: > 0 })
        {
            users = users.Where(user =>
                (user.ServiceCategories?.Any(serviceCategory => categories.Contains(serviceCategory, StringComparer.Ordinal)) ?? false));
        }

        return users.OrderBy(user => user.DisplayName, StringComparer.Ordinal).ToArray();
    }

    public UserProfile Create(
        UserProfile user,
        string passwordHash,
        string? emailVerificationToken = null,
        DateTimeOffset? emailVerificationTokenExpiresAt = null)
    {
        _users.Add(user);
        _passwordHashes[user.Id] = passwordHash;
        return user;
    }

    public UserProfile UpdateProfile(UserProfile user)
    {
        var existingUserIndex = _users.FindIndex(existingUser => string.Equals(existingUser.Id, user.Id, StringComparison.Ordinal));
        if (existingUserIndex < 0)
        {
            throw new KeyNotFoundException("Korisnik nije pronadjen.");
        }

        _users[existingUserIndex] = user;
        return user;
    }

    public bool VerifyEmail(string email, string token, DateTimeOffset verifiedAt)
    {
        return _users.Any(user => string.Equals(user.Email, email, StringComparison.OrdinalIgnoreCase));
    }

    public bool UpdateEmailVerificationToken(string email, string token, DateTimeOffset expiresAt)
    {
        return _users.Any(user => string.Equals(user.Email, email, StringComparison.OrdinalIgnoreCase));
    }

    public string? GetStoredPasswordHash(string userId)
    {
        return _passwordHashes.GetValueOrDefault(userId);
    }
}

internal sealed class FakeEmailVerificationSender : IEmailVerificationSender
{
    public List<(string Email, string DisplayName, string Token)> SentEmails { get; } = [];

    public bool IsSuccessful { get; set; } = true;

    public bool SendVerificationEmail(string email, string displayName, string token)
    {
        SentEmails.Add((email, displayName, token));
        return IsSuccessful;
    }
}

internal sealed class FakeJobRepository : IJobRepository
{
    private readonly List<Job> _jobs = [];

    public FakeJobRepository(params IEnumerable<Job>[] jobCollections)
    {
        foreach (var jobCollection in jobCollections)
        {
            _jobs.AddRange(jobCollection);
        }
    }

    public IReadOnlyCollection<Job> GetAll(GetJobsRequest? request = null)
    {
        IEnumerable<Job> jobs = _jobs;
        var query = request?.Query?.Trim();

        if (!string.IsNullOrWhiteSpace(query))
        {
            jobs = jobs.Where(job =>
                job.Title.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                job.Description.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                job.Location.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                job.Category.Contains(query, StringComparison.OrdinalIgnoreCase));
        }

        var categories = request?.Categories?
            .Where(category => !string.IsNullOrWhiteSpace(category))
            .Select(category => category.Trim())
            .ToArray();

        if (categories is { Length: > 0 })
        {
            jobs = jobs.Where(job => categories.Contains(job.Category, StringComparer.Ordinal));
        }

        return jobs.OrderByDescending(job => job.CreatedAt).ToArray();
    }

    public Job? GetById(string id)
    {
        return _jobs.FirstOrDefault(job => string.Equals(job.Id, id, StringComparison.Ordinal));
    }

    public Job Create(Job job)
    {
        _jobs.Add(job);
        return job;
    }

    public Job? Update(Job job)
    {
        var existingJobIndex = _jobs.FindIndex(existingJob => string.Equals(existingJob.Id, job.Id, StringComparison.Ordinal));
        if (existingJobIndex < 0)
        {
            return null;
        }

        _jobs[existingJobIndex] = job;
        return job;
    }

    public Job? UpdateStatus(string id, string status)
    {
        var existingJobIndex = _jobs.FindIndex(existingJob => string.Equals(existingJob.Id, id, StringComparison.Ordinal));
        if (existingJobIndex < 0)
        {
            return null;
        }

        var updatedJob = _jobs[existingJobIndex] with { Status = status };
        _jobs[existingJobIndex] = updatedJob;
        return updatedJob;
    }

    public bool Delete(string id)
    {
        return _jobs.RemoveAll(existingJob => string.Equals(existingJob.Id, id, StringComparison.Ordinal)) > 0;
    }
}

internal sealed class FakeBidRepository : IBidRepository
{
    private readonly List<Bid> _bids = [];

    public FakeBidRepository(params IEnumerable<Bid>[] bidCollections)
    {
        foreach (var bidCollection in bidCollections)
        {
            _bids.AddRange(bidCollection);
        }
    }

    public IReadOnlyCollection<Bid> GetAll()
    {
        return _bids.OrderByDescending(bid => bid.CreatedAt).ToArray();
    }

    public IReadOnlyCollection<Bid> GetByJobId(string jobId)
    {
        return _bids
            .Where(bid => string.Equals(bid.JobId, jobId, StringComparison.Ordinal))
            .OrderByDescending(bid => bid.CreatedAt)
            .ToArray();
    }

    public IReadOnlyCollection<Bid> GetByContractorId(string contractorId)
    {
        return _bids
            .Where(bid => string.Equals(bid.ContractorId, contractorId, StringComparison.Ordinal))
            .OrderByDescending(bid => bid.CreatedAt)
            .ToArray();
    }

    public Bid? GetById(string id)
    {
        return _bids.FirstOrDefault(bid => string.Equals(bid.Id, id, StringComparison.Ordinal));
    }

    public Bid Create(Bid bid)
    {
        _bids.Add(bid);
        return bid;
    }

    public void ReplaceForJob(string jobId, IReadOnlyCollection<Bid> bids)
    {
        _bids.RemoveAll(existingBid => string.Equals(existingBid.JobId, jobId, StringComparison.Ordinal));
        _bids.AddRange(bids);
    }
}

internal sealed class FakeReviewRepository : IReviewRepository
{
    private readonly List<Review> _reviews = [];

    public FakeReviewRepository(params IEnumerable<Review>[] reviewCollections)
    {
        foreach (var reviewCollection in reviewCollections)
        {
            _reviews.AddRange(reviewCollection);
        }
    }

    public IReadOnlyCollection<Review> GetAll()
    {
        return _reviews.OrderByDescending(review => review.CreatedAt).ToArray();
    }

    public IReadOnlyCollection<Review> GetByRevieweeId(string revieweeId)
    {
        return _reviews
            .Where(review => string.Equals(review.RevieweeId, revieweeId, StringComparison.Ordinal))
            .OrderByDescending(review => review.CreatedAt)
            .ToArray();
    }

    public Review? GetByJobId(string jobId)
    {
        return _reviews.FirstOrDefault(review => string.Equals(review.JobId, jobId, StringComparison.Ordinal));
    }

    public Review Create(Review review)
    {
        _reviews.Add(review);
        return review;
    }
}
