using BuildConnect.Model;

namespace BuildConnect.Service.Common;

public interface IUserService
{
    IReadOnlyCollection<UserProfileResponse> GetUsers(string? role = null);

    IReadOnlyCollection<UserProfileResponse> GetContractors(GetContractorsRequest? request = null);

    UserProfileResponse? GetUserById(string id, RequestUserContext? userContext = null);

    UserProfileResponse UpdateCurrentUserProfile(UpdateUserProfileRequest request, RequestUserContext userContext);
}
