using BuildConnect.Model;

namespace BuildConnect.Repository.Common;

public interface IReviewRepository
{
    IReadOnlyCollection<Review> GetAll();

    IReadOnlyCollection<Review> GetByRevieweeId(string revieweeId);

    Review? GetByJobId(string jobId);

    Review Create(Review review);
}
