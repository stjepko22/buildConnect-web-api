using BuildConnect.Model;
using BuildConnect.Repository.Common;
using BuildConnect.Service.Common;

namespace BuildConnect.Service;

public sealed class UserService : IUserService
{
    private const int MaxServiceCategoryCount = 10;
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

    public UserProfileResponse UpdateCurrentUserProfile(UpdateUserProfileRequest request, RequestUserContext userContext)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(userContext);

        var currentUser = _userRepository.GetById(userContext.UserId);
        if (currentUser is null)
        {
            throw new KeyNotFoundException("Korisnik nije pronadjen.");
        }

        var updatedUser = currentUser with
        {
            DisplayName = RequireValue(request.DisplayName, "Ime i prezime"),
            LegalType = RequireLegalType(request.LegalType),
            Bio = NormalizeBio(request.Bio),
            Location = RequireValue(request.Location, "Lokacija"),
            ServiceCategories = NormalizeServiceCategories(request.ServiceCategories, currentUser.Role)
        };

        return MapToResponse(_userRepository.UpdateProfile(updatedUser));
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

    private static string RequireValue(string? value, string fieldName)
    {
        var normalizedValue = value?.Trim();
        if (string.IsNullOrWhiteSpace(normalizedValue))
        {
            throw new ArgumentException($"{fieldName} je obavezno.");
        }

        return normalizedValue;
    }

    private static string RequireLegalType(string? legalType)
    {
        var normalizedLegalType = legalType?.Trim();
        if (string.IsNullOrWhiteSpace(normalizedLegalType) || !BuildConnectLegalTypes.IsSupported(normalizedLegalType))
        {
            throw new ArgumentException("Odabrani tip korisnika nije podrzan.");
        }

        return normalizedLegalType;
    }

    private static string NormalizeBio(string? bio)
    {
        return bio?.Trim() ?? string.Empty;
    }

    private static IReadOnlyCollection<string>? NormalizeServiceCategories(IReadOnlyCollection<string>? serviceCategories, string role)
    {
        if (!string.Equals(role, BuildConnectRoles.Izvodjac, StringComparison.Ordinal))
        {
            return null;
        }

        if (serviceCategories is null || serviceCategories.Count == 0)
        {
            return null;
        }

        var normalizedCategories = serviceCategories
            .Select(category => category?.Trim())
            .Where(category => !string.IsNullOrWhiteSpace(category))
            .Distinct(StringComparer.Ordinal)
            .Take(MaxServiceCategoryCount)
            .Cast<string>()
            .ToArray();

        return normalizedCategories.Length == 0 ? null : normalizedCategories;
    }
}
