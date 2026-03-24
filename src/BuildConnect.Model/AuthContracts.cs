namespace BuildConnect.Model;

public sealed record AuthAccount(
    string UserId,
    string Email,
    string PasswordHash);

public sealed record LoginRequest(
    string Email,
    string Password);

public sealed record RegisterRequest(
    string FirstName,
    string LastName,
    string Email,
    string Password,
    string? Phone,
    string Role,
    string? LegalType);

public sealed record AuthenticatedUserResponse(
    string Id,
    string Email,
    string DisplayName,
    string Role,
    string LegalType);

public sealed record AuthenticatedSessionResponse(
    string AccessToken,
    AuthenticatedUserResponse User);
