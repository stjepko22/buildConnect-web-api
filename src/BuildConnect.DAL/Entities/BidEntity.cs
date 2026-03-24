namespace BuildConnect.DAL.Entities;

public sealed class BidEntity
{
    public string Id { get; set; } = string.Empty;

    public string JobId { get; set; } = string.Empty;

    public string ContractorId { get; set; } = string.Empty;

    public string ContractorName { get; set; } = string.Empty;

    public decimal Amount { get; set; }

    public int DaysToComplete { get; set; }

    public string Message { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public DateTimeOffset CreatedAt { get; set; }

    public JobEntity? Job { get; set; }

    public UserEntity? Contractor { get; set; }
}
