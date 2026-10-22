namespace BuildConnect.Model;

public static class BuildConnectRoles
{
    public const string Investitor = "INVESTITOR";
    public const string Izvodjac = "IZVODJAC";
}

public static class JobCategories
{
    public static readonly string[] All =
    [
        "Gradnja",
        "Renovacija",
        "Fasade",
        "Krovovi",
        "Keramika",
        "Elektro",
        "Vodoinstalacije",
        "Grijanje",
        "Stolarija",
        "Ostalo"
    ];

    public static bool IsSupported(string category)
    {
        return All.Contains(category, StringComparer.Ordinal);
    }
}

public static class JobStatuses
{
    public const string Open = "OPEN";
    public const string Closed = "CLOSED";
    public const string InProgress = "IN_PROGRESS";
    public const string Completed = "COMPLETED";

    public static readonly string[] All =
    [
        Open,
        Closed,
        InProgress,
        Completed
    ];

    public static bool IsSupported(string status)
    {
        return All.Contains(status, StringComparer.Ordinal);
    }
}

public sealed record RequestUserContext(string UserId, string Role, string? DisplayName = null);

public sealed record Job(
    string Id,
    string Title,
    string Description,
    string Category,
    string Location,
    decimal? Budget,
    string Deadline,
    string InvestitorId,
    DateTimeOffset CreatedAt,
    string Status);

public sealed record JobResponse(
    string Id,
    string Title,
    string Description,
    string Category,
    string Location,
    decimal? Budget,
    string Deadline,
    string InvestitorId,
    DateTimeOffset CreatedAt,
    string Status);

public sealed record GetJobsRequest(
    string? Query = null,
    IReadOnlyCollection<string>? Categories = null);

public sealed record CreateJobRequest(
    string Title,
    string Description,
    string Category,
    string Location,
    decimal? Budget,
    string Deadline);

public sealed record UpdateJobRequest(
    string Title,
    string Description,
    string Category,
    string Location,
    decimal? Budget,
    string Deadline);
