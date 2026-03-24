namespace BuildConnect.DAL.Entities;

public sealed class UserEntity
{
    public string Id { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string PasswordHash { get; set; } = string.Empty;

    public string DisplayName { get; set; } = string.Empty;

    public string Role { get; set; } = string.Empty;

    public string LegalType { get; set; } = string.Empty;

    public string Bio { get; set; } = string.Empty;

    public string Location { get; set; } = string.Empty;

    public DateTimeOffset JoinedAt { get; set; }

    public string? ServiceCategoriesJson { get; set; }

    public ICollection<JobEntity> Jobs { get; set; } = [];

    public ICollection<BidEntity> SubmittedBids { get; set; } = [];

    public ICollection<ReviewEntity> WrittenReviews { get; set; } = [];

    public ICollection<ReviewEntity> ReceivedReviews { get; set; } = [];
}
