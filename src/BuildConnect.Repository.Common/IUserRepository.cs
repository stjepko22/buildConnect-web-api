using BuildConnect.Model;

namespace BuildConnect.Repository.Common;

public interface IUserRepository
{
    IReadOnlyCollection<UserProfile> GetAll();

    UserProfile? GetById(string id);

    UserProfile? GetByEmail(string email);

    IReadOnlyCollection<UserProfile> GetByRole(string role);

    IReadOnlyCollection<UserProfile> GetContractors(GetContractorsRequest? request = null);

    UserProfile Create(
        UserProfile user,
        string passwordHash,
        string? emailVerificationToken = null,
        DateTimeOffset? emailVerificationTokenExpiresAt = null);

    UserProfile UpdateProfile(UserProfile user);

    bool VerifyEmail(string email, string token, DateTimeOffset verifiedAt);

    bool UpdateEmailVerificationToken(string email, string token, DateTimeOffset expiresAt);
}
