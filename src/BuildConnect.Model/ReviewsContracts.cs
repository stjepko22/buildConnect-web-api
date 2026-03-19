namespace BuildConnect.Model;

public sealed record Review(
    string Id,
    string JobId,
    string ReviewerId,
    string RevieweeId,
    int Rating,
    string Comment,
    DateTimeOffset CreatedAt);

public sealed record ReviewResponse(
    string Id,
    string JobId,
    string ReviewerId,
    string RevieweeId,
    int Rating,
    string Comment,
    DateTimeOffset CreatedAt);

public sealed record CreateReviewRequest(
    int Rating,
    string Comment);
