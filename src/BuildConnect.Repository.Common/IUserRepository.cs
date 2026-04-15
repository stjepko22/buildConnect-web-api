using BuildConnect.Model;

namespace BuildConnect.Repository.Common;

public interface IUserRepository
{
    IReadOnlyCollection<UserProfile> GetAll();

    UserProfile? GetById(string id);

    UserProfile? GetByEmail(string email);

    IReadOnlyCollection<UserProfile> GetByRole(string role);

    UserProfile Create(UserProfile user, string passwordHash);

    UserProfile UpdateProfile(UserProfile user);
}
