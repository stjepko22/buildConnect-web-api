using BuildConnect.Model;

namespace BuildConnect.Service.Common;

public interface IUserService
{
    IReadOnlyCollection<UserProfileResponse> GetUsers(string? role = null);

    IReadOnlyCollection<UserProfileResponse> GetContractors();

    UserProfileResponse? GetUserById(string id);

    UserProfileResponse UpdateCurrentUserProfile(UpdateUserProfileRequest request, RequestUserContext userContext);
}
