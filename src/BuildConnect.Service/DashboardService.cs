using BuildConnect.Model;
using BuildConnect.Repository.Common;
using BuildConnect.Service.Common;

namespace BuildConnect.Service;

public sealed class DashboardService : IDashboardService
{
    private static readonly string[] DefaultInvestorCategories = ["Fasade"];
    private readonly IBidRepository _bidRepository;
    private readonly IJobRepository _jobRepository;
    private readonly IReviewRepository _reviewRepository;
    private readonly IUserRepository _userRepository;

    public DashboardService(
        IJobRepository jobRepository,
        IBidRepository bidRepository,
        IReviewRepository reviewRepository,
        IUserRepository userRepository)
    {
        _jobRepository = jobRepository;
        _bidRepository = bidRepository;
        _reviewRepository = reviewRepository;
        _userRepository = userRepository;
    }

    public DashboardResponse GetDashboard(RequestUserContext userContext)
    {
        ArgumentNullException.ThrowIfNull(userContext);

        if (string.IsNullOrWhiteSpace(userContext.UserId))
        {
            throw new UnauthorizedAccessException("Korisnicki identitet nije dostupan.");
        }

        return string.Equals(userContext.Role, BuildConnectRoles.Investitor, StringComparison.Ordinal)
            ? BuildInvestorDashboard(userContext.UserId)
            : BuildContractorDashboard(userContext.UserId);
    }

    private DashboardResponse BuildInvestorDashboard(string userId)
    {
        var jobs = _jobRepository.GetAll()
            .Where(job => string.Equals(job.InvestitorId, userId, StringComparison.Ordinal))
            .OrderByDescending(job => job.CreatedAt)
            .ToArray();

        var bids = _bidRepository.GetAll();
        var bidsByJobId = bids
            .GroupBy(bid => bid.JobId, StringComparer.Ordinal)
            .ToDictionary(group => group.Key, group => group.ToArray(), StringComparer.Ordinal);

        var totalBidsOnMyJobs = jobs.Sum(job => bidsByJobId.GetValueOrDefault(job.Id)?.Length ?? 0);
        var activeProjectsCount = jobs.Count(job => string.Equals(job.Status, JobStatuses.InProgress, StringComparison.Ordinal));

        var jobActivities = jobs
            .Select(job => new DashboardJobActivityResponse(
                job.Id,
                job.Title,
                job.Budget,
                bidsByJobId.GetValueOrDefault(job.Id)?.Length ?? 0))
            .ToArray();

        var recommendedContractors = BuildRecommendedContractors(jobs);

        return new DashboardResponse(
            new DashboardSummaryResponse(
                jobs.Length,
                totalBidsOnMyJobs,
                activeProjectsCount,
                0,
                0,
                0m),
            jobActivities,
            null,
            recommendedContractors);
    }

    private DashboardResponse BuildContractorDashboard(string userId)
    {
        var bids = _bidRepository.GetByContractorId(userId)
            .OrderByDescending(bid => bid.CreatedAt)
            .ToArray();
        var jobsById = _jobRepository.GetAll()
            .ToDictionary(job => job.Id, StringComparer.Ordinal);
        var reviews = _reviewRepository.GetByRevieweeId(userId);
        var acceptedBidsCount = bids.Count(bid => string.Equals(bid.Status, BidStatuses.Accepted, StringComparison.Ordinal));
        var averageRating = reviews.Count > 0
            ? Math.Round((decimal)reviews.Average(review => review.Rating), 1, MidpointRounding.AwayFromZero)
            : 0m;

        var bidActivities = bids
            .Select(bid => new DashboardBidActivityResponse(
                bid.Id,
                bid.JobId,
                jobsById.GetValueOrDefault(bid.JobId)?.Title ?? "Nepoznat posao",
                bid.Status,
                bid.Amount))
            .ToArray();

        return new DashboardResponse(
            new DashboardSummaryResponse(
                0,
                0,
                0,
                bids.Length,
                acceptedBidsCount,
                averageRating),
            null,
            bidActivities,
            null);
    }

    private IReadOnlyCollection<DashboardRecommendedContractorResponse> BuildRecommendedContractors(IReadOnlyCollection<Job> jobs)
    {
        var preferredCategories = jobs.Count > 0
            ? jobs.Select(job => job.Category).Distinct(StringComparer.Ordinal).ToArray()
            : DefaultInvestorCategories;
        var preferredLocations = jobs
            .Select(job => job.Location)
            .Distinct(StringComparer.Ordinal)
            .ToArray();

        var reviewsByContractor = _reviewRepository.GetAll()
            .GroupBy(review => review.RevieweeId, StringComparer.Ordinal)
            .ToDictionary(group => group.Key, group => group.ToArray(), StringComparer.Ordinal);

        return _userRepository.GetContractors()
            .Select(contractor =>
            {
                var serviceCategories = contractor.ServiceCategories ?? [];
                var categoryMatches = serviceCategories
                    .Where(category => preferredCategories.Contains(category, StringComparer.Ordinal))
                    .Distinct(StringComparer.Ordinal)
                    .ToArray();
                var isLocationMatch = preferredLocations.Contains(contractor.Location, StringComparer.Ordinal);
                var reviews = reviewsByContractor.GetValueOrDefault(contractor.Id) ?? [];
                var reviewCount = reviews.Length;
                var averageRating = reviewCount > 0
                    ? Math.Round((decimal)reviews.Average(review => review.Rating), 1, MidpointRounding.AwayFromZero)
                    : 0m;
                var matchScore = categoryMatches.Length * 3m + (isLocationMatch ? 2m : 0m) + (averageRating * 1.5m) + Math.Min(reviewCount * 0.2m, 1m);

                return new
                {
                    Contractor = contractor,
                    CategoryMatches = categoryMatches,
                    IsLocationMatch = isLocationMatch,
                    AverageRating = averageRating,
                    ReviewCount = reviewCount,
                    MatchScore = matchScore
                };
            })
            .Where(item => item.CategoryMatches.Length > 0 || item.IsLocationMatch)
            .OrderByDescending(item => item.MatchScore)
            .ThenByDescending(item => item.AverageRating)
            .Take(3)
            .Select(item => new DashboardRecommendedContractorResponse(
                item.Contractor.Id,
                item.Contractor.DisplayName,
                item.Contractor.Location,
                item.Contractor.LegalType,
                item.AverageRating,
                item.ReviewCount,
                item.CategoryMatches,
                item.IsLocationMatch))
            .ToArray();
    }
}
