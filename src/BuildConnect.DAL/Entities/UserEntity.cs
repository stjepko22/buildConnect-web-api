namespace BuildConnect.DAL.Entities;

public sealed class UserEntity
{
    public string Id { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string NormalizedEmail { get; set; } = string.Empty;

    public string PasswordHash { get; set; } = string.Empty;

    public bool IsEmailVerified { get; set; }

    public string? EmailVerificationToken { get; set; }

    public DateTimeOffset? EmailVerificationTokenExpiresAt { get; set; }

    public string DisplayName { get; set; } = string.Empty;

    public string Role { get; set; } = string.Empty;

    public string LegalType { get; set; } = string.Empty;

    public string? Phone { get; set; }

    public bool IsPhoneVisible { get; set; }

    public string Bio { get; set; } = string.Empty;

    public string Location { get; set; } = string.Empty;

    public DateTimeOffset JoinedAt { get; set; }

    public string? ServiceCategoriesJson { get; set; }

    public string? ProfileImageVersion { get; set; }

    public string? CoverImageVersion { get; set; }

    public ICollection<JobEntity> Jobs { get; set; } = [];

    public ICollection<BidEntity> SubmittedBids { get; set; } = [];

    public ICollection<ReviewEntity> WrittenReviews { get; set; } = [];

    public ICollection<ReviewEntity> ReceivedReviews { get; set; } = [];
}
