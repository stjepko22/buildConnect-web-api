namespace BuildConnect.DAL.Entities;

public sealed class JobEntity
{
    public string Id { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string Category { get; set; } = string.Empty;

    public string Location { get; set; } = string.Empty;

    public decimal? Budget { get; set; }

    public string Deadline { get; set; } = string.Empty;

    public string InvestitorId { get; set; } = string.Empty;

    public DateTimeOffset CreatedAt { get; set; }

    public UserEntity? Investitor { get; set; }

    public ICollection<BidEntity> Bids { get; set; } = [];

    public ICollection<ReviewEntity> Reviews { get; set; } = [];
}
