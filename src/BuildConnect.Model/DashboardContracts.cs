namespace BuildConnect.Model;

public sealed record DashboardSummaryResponse(
    int JobsCount,
    int TotalBidsOnMyJobs,
    int ActiveProjectsCount,
    int SentBidsCount,
    int AcceptedBidsCount,
    decimal AverageRating);

public sealed record DashboardJobActivityResponse(
    string JobId,
    string Title,
    decimal? Budget,
    int BidsCount);

public sealed record DashboardBidActivityResponse(
    string BidId,
    string JobId,
    string JobTitle,
    string Status,
    decimal Amount);

public sealed record DashboardRecommendedContractorResponse(
    string ContractorId,
    string DisplayName,
    string Location,
    string LegalType,
    decimal AverageRating,
    int ReviewCount,
    IReadOnlyCollection<string> CategoryMatches,
    bool IsLocationMatch);

public sealed record DashboardResponse(
    DashboardSummaryResponse Summary,
    IReadOnlyCollection<DashboardJobActivityResponse>? JobActivities = null,
    IReadOnlyCollection<DashboardBidActivityResponse>? BidActivities = null,
    IReadOnlyCollection<DashboardRecommendedContractorResponse>? RecommendedContractors = null);
