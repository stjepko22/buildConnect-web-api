namespace BuildConnect.Model;

public static class BidStatuses
{
    public const string Pending = "PENDING";
    public const string Accepted = "ACCEPTED";
    public const string Rejected = "REJECTED";
}

public sealed record Bid(
    string Id,
    string JobId,
    string ContractorId,
    string ContractorName,
    decimal Amount,
    int DaysToComplete,
    string Message,
    string Status,
    DateTimeOffset CreatedAt);

public sealed record BidResponse(
    string Id,
    string JobId,
    string ContractorId,
    string ContractorName,
    decimal Amount,
    int DaysToComplete,
    string Message,
    string Status,
    DateTimeOffset CreatedAt);

public sealed record CreateBidRequest(
    decimal Amount,
    int DaysToComplete,
    string Message);
