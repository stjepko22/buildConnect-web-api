using BuildConnect.Model;
using BuildConnect.Repository.Common;

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

    public UserProfile Create(UserProfile user)
    {
        _users.Add(user);
        return user;
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

    public IReadOnlyCollection<Job> GetAll()
    {
        return _jobs.OrderByDescending(job => job.CreatedAt).ToArray();
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
