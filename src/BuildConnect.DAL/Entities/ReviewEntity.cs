namespace BuildConnect.DAL.Entities;

public sealed class ReviewEntity
{
    public string Id { get; set; } = string.Empty;

    public string JobId { get; set; } = string.Empty;

    public string ReviewerId { get; set; } = string.Empty;

    public string RevieweeId { get; set; } = string.Empty;

    public int Rating { get; set; }

    public string Comment { get; set; } = string.Empty;

    public DateTimeOffset CreatedAt { get; set; }

    public JobEntity? Job { get; set; }

    public UserEntity? Reviewer { get; set; }

    public UserEntity? Reviewee { get; set; }
}
