namespace BuildConnect.Model;

public static class BuildConnectLegalTypes
{
    public const string FizickaOsoba = "FIZICKA_OSOBA";
    public const string Firma = "FIRMA";

    public static bool IsSupported(string legalType)
    {
        return string.Equals(legalType, FizickaOsoba, StringComparison.Ordinal)
            || string.Equals(legalType, Firma, StringComparison.Ordinal);
    }
}

public sealed record UserProfile(
    string Id,
    string DisplayName,
    string Role,
    string LegalType,
    string Email,
    string Bio,
    string Location,
    DateTimeOffset JoinedAt,
    IReadOnlyCollection<string>? ServiceCategories = null);

public sealed record UserProfileResponse(
    string Id,
    string DisplayName,
    string Role,
    string LegalType,
    string Email,
    string Bio,
    string Location,
    DateTimeOffset JoinedAt,
    IReadOnlyCollection<string>? ServiceCategories = null);

public sealed record UpdateUserProfileRequest(
    string DisplayName,
    string LegalType,
    string Bio,
    string Location,
    IReadOnlyCollection<string>? ServiceCategories = null);
