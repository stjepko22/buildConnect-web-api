using BuildConnect.Model;
using BuildConnect.Repository.Common;
using BuildConnect.Service.Common;

namespace BuildConnect.Service;

public sealed class ReviewService : IReviewService
{
    private readonly IReviewRepository _reviewRepository;
    private readonly IJobRepository _jobRepository;
    private readonly IBidRepository _bidRepository;

    public ReviewService(IReviewRepository reviewRepository, IJobRepository jobRepository, IBidRepository bidRepository)
    {
        _reviewRepository = reviewRepository;
        _jobRepository = jobRepository;
        _bidRepository = bidRepository;
    }

    public IReadOnlyCollection<ReviewResponse> GetReviews(string? jobId = null, string? revieweeId = null)
    {
        if (!string.IsNullOrWhiteSpace(jobId))
        {
            var review = _reviewRepository.GetByJobId(jobId.Trim());
            return review is null ? [] : [MapToResponse(review)];
        }

        var reviews = !string.IsNullOrWhiteSpace(revieweeId)
            ? _reviewRepository.GetByRevieweeId(revieweeId.Trim())
            : _reviewRepository.GetAll();

        return reviews
            .Select(MapToResponse)
            .ToArray();
    }

    public ReviewResponse CreateReview(string jobId, CreateReviewRequest request, RequestUserContext userContext)
    {
        ValidateCreateContext(userContext);
        ValidateCreateRequest(request);

        var normalizedJobId = jobId.Trim();
        var job = _jobRepository.GetById(normalizedJobId);
        if (job is null)
        {
            throw new ArgumentException("Posao za recenziju nije pronadjen.");
        }

        if (!string.Equals(job.InvestitorId, userContext.UserId, StringComparison.Ordinal))
        {
            throw new UnauthorizedAccessException("Ne mozete recenzirati posao koji nije vas.");
        }

        var existingReview = _reviewRepository.GetByJobId(normalizedJobId);
        if (existingReview is not null)
        {
            throw new ArgumentException("Za ovaj posao je vec ostavljena recenzija.");
        }

        var acceptedBid = _bidRepository
            .GetByJobId(normalizedJobId)
            .FirstOrDefault(bid => string.Equals(bid.Status, BidStatuses.Accepted, StringComparison.Ordinal));

        if (acceptedBid is null)
        {
          throw new ArgumentException("Recenziju mozete ostaviti tek nakon prihvacene ponude.");
        }

        var review = new Review(
            Guid.NewGuid().ToString("N"),
            normalizedJobId,
            userContext.UserId,
            acceptedBid.ContractorId,
            request.Rating,
            request.Comment.Trim(),
            DateTimeOffset.UtcNow);

        var createdReview = _reviewRepository.Create(review);
        return MapToResponse(createdReview);
    }

    private static void ValidateCreateContext(RequestUserContext userContext)
    {
        if (string.IsNullOrWhiteSpace(userContext.UserId))
        {
            throw new UnauthorizedAccessException("Korisnicki identitet nije dostupan.");
        }

        if (!string.Equals(userContext.Role, BuildConnectRoles.Investitor, StringComparison.Ordinal))
        {
            throw new UnauthorizedAccessException("Samo investitor moze ostaviti recenziju.");
        }
    }

    private static void ValidateCreateRequest(CreateReviewRequest request)
    {
        if (request.Rating < 1 || request.Rating > 5)
        {
            throw new ArgumentException("Ocjena mora biti izmedju 1 i 5.");
        }

        if (string.IsNullOrWhiteSpace(request.Comment) || request.Comment.Trim().Length < 10)
        {
            throw new ArgumentException("Komentar mora imati barem 10 znakova.");
        }
    }

    private static ReviewResponse MapToResponse(Review review)
    {
        return new ReviewResponse(
            review.Id,
            review.JobId,
            review.ReviewerId,
            review.RevieweeId,
            review.Rating,
            review.Comment,
            review.CreatedAt);
    }
}
