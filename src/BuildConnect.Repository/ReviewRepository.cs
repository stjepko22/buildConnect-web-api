using BuildConnect.DAL;
using BuildConnect.Model;
using BuildConnect.Repository.Common;

namespace BuildConnect.Repository;

public sealed class ReviewRepository : IReviewRepository
{
    private readonly InMemoryReviewDataStore _reviewDataStore;

    public ReviewRepository(InMemoryReviewDataStore reviewDataStore)
    {
        _reviewDataStore = reviewDataStore;
    }

    public IReadOnlyCollection<Review> GetAll()
    {
        return _reviewDataStore.GetAll();
    }

    public IReadOnlyCollection<Review> GetByRevieweeId(string revieweeId)
    {
        return _reviewDataStore.GetByRevieweeId(revieweeId);
    }

    public Review? GetByJobId(string jobId)
    {
        return _reviewDataStore.GetByJobId(jobId);
    }

    public Review Create(Review review)
    {
        return _reviewDataStore.Add(review);
    }
}
