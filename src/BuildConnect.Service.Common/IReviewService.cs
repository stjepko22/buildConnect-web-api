using BuildConnect.Model;

namespace BuildConnect.Service.Common;

public interface IReviewService
{
    IReadOnlyCollection<ReviewResponse> GetReviews(string? jobId = null, string? revieweeId = null);

    ReviewResponse CreateReview(string jobId, CreateReviewRequest request, RequestUserContext userContext);
}
