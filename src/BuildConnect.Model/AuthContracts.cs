namespace BuildConnect.Model;

public sealed record AuthAccount(
    string UserId,
    string Email,
    string PasswordHash,
    bool IsEmailVerified);

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
    string LegalType,
    string? Phone = null,
    bool IsPhoneVisible = false);

public sealed record AuthenticatedSessionResponse(
    string AccessToken,
    AuthenticatedUserResponse User);

public sealed record RegistrationResponse(
    string Email,
    bool IsVerificationEmailSent);

public sealed record VerifyEmailRequest(
    string Email,
    string Token);

public sealed record ResendVerificationEmailRequest(
    string Email);

public static class EmailNormalizer
{
    public static string Normalize(string email)
    {
        return email.Trim().ToUpperInvariant();
    }
}
