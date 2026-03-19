using BuildConnect.Model;
using BuildConnect.Repository.Common;
using BuildConnect.Service.Common;

namespace BuildConnect.Service;

public sealed class UserService : IUserService
{
    private readonly IUserRepository _userRepository;

    public UserService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public IReadOnlyCollection<UserProfileResponse> GetUsers(string? role = null)
    {
        var users = string.IsNullOrWhiteSpace(role)
            ? _userRepository.GetAll()
            : _userRepository.GetByRole(role.Trim());

        return users
            .Select(MapToResponse)
            .ToArray();
    }

    public IReadOnlyCollection<UserProfileResponse> GetContractors()
    {
        return _userRepository
            .GetByRole(BuildConnectRoles.Izvodjac)
            .Select(MapToResponse)
            .ToArray();
    }

    public UserProfileResponse? GetUserById(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return null;
        }

        var user = _userRepository.GetById(id.Trim());
        return user is null ? null : MapToResponse(user);
    }

    private static UserProfileResponse MapToResponse(UserProfile user)
    {
        return new UserProfileResponse(
            user.Id,
            user.DisplayName,
            user.Role,
            user.LegalType,
            user.Email,
            user.Bio,
            user.Location,
            user.JoinedAt,
            user.ServiceCategories);
    }
}
