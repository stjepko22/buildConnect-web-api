using BuildConnect.Model;

namespace BuildConnect.Service.Common;

public interface IUserProfileImageService
{
    Task<UserProfileImage?> GetAsync(string userId, string kind, CancellationToken cancellationToken = default);

    Task<UserProfileResponse> UploadCurrentUserImageAsync(
        string kind,
        Stream content,
        string? contentType,
        RequestUserContext userContext,
        CancellationToken cancellationToken = default);

    Task<UserProfileResponse> DeleteCurrentUserImageAsync(
        string kind,
        RequestUserContext userContext,
        CancellationToken cancellationToken = default);
}
