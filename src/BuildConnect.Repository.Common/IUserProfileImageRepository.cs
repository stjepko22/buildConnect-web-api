using BuildConnect.Model;

namespace BuildConnect.Repository.Common;

public interface IUserProfileImageRepository
{
    Task<UserProfileImage?> GetAsync(string userId, string kind, CancellationToken cancellationToken = default);

    Task SaveAsync(string userId, string kind, UserProfileImage image, CancellationToken cancellationToken = default);

    Task DeleteAsync(string userId, string kind, CancellationToken cancellationToken = default);
}
