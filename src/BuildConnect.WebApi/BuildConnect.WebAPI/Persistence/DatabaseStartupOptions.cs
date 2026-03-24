namespace BuildConnect.WebAPI.Persistence;

public sealed class DatabaseStartupOptions
{
    public const string SectionName = "DatabaseStartup";

    public bool ApplyMigrationsOnStartup { get; init; }

    public bool SeedOnStartup { get; init; }
}
