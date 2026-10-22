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
            .Select(user => MapToResponse(user))
            .ToArray();
    }

    public IReadOnlyCollection<UserProfileResponse> GetContractors(GetContractorsRequest? request = null)
    {
        var normalizedRequest = NormalizeContractorRequest(request);

        return _userRepository
            .GetContractors(normalizedRequest)
            .Select(user => MapToResponse(user))
            .ToArray();
    }

    public UserProfileResponse? GetUserById(string id, RequestUserContext? userContext = null)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return null;
        }

        var user = _userRepository.GetById(id.Trim());
        return user is null
            ? null
            : MapToResponse(user, string.Equals(user.Id, userContext?.UserId, StringComparison.Ordinal));
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

        var phone = request.Phone is null ? currentUser.Phone : NormalizePhone(request.Phone);
        var updatedUser = currentUser with
        {
            DisplayName = RequireValue(request.DisplayName, "Ime i prezime"),
            LegalType = RequireLegalType(request.LegalType),
            Phone = phone,
            IsPhoneVisible = !string.IsNullOrWhiteSpace(phone) && request.IsPhoneVisible,
            Bio = NormalizeBio(request.Bio),
            Location = RequireValue(request.Location, "Lokacija"),
            ServiceCategories = NormalizeServiceCategories(request.ServiceCategories, currentUser.Role)
        };

        return MapToResponse(_userRepository.UpdateProfile(updatedUser), includePrivatePhone: true);
    }

    private static UserProfileResponse MapToResponse(UserProfile user, bool includePrivatePhone = false)
    {
        var isPhoneVisible = !string.IsNullOrWhiteSpace(user.Phone) && user.IsPhoneVisible;

        return new UserProfileResponse(
            user.Id,
            user.DisplayName,
            user.Role,
            user.LegalType,
            user.Email,
            user.Bio,
            user.Location,
            user.JoinedAt,
            user.ServiceCategories,
            isPhoneVisible || includePrivatePhone ? user.Phone : null,
            isPhoneVisible);
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

    private static string? NormalizePhone(string? phone)
    {
        var normalizedPhone = phone?.Trim();
        if (string.IsNullOrWhiteSpace(normalizedPhone))
        {
            return null;
        }

        if (normalizedPhone.Length < 6 || normalizedPhone.Length > 32)
        {
            throw new ArgumentException("Kontakt broj mora imati od 6 do 32 znaka.");
        }

        return normalizedPhone;
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

    private static GetContractorsRequest NormalizeContractorRequest(GetContractorsRequest? request)
    {
        var normalizedQuery = string.IsNullOrWhiteSpace(request?.Query)
            ? null
            : request.Query.Trim();

        var normalizedLocation = string.IsNullOrWhiteSpace(request?.Location)
            ? null
            : request.Location.Trim();

        var normalizedLegalType = string.IsNullOrWhiteSpace(request?.LegalType)
            ? null
            : request.LegalType.Trim();

        if (normalizedLegalType is not null && !BuildConnectLegalTypes.IsSupported(normalizedLegalType))
        {
            normalizedLegalType = null;
        }

        var normalizedCategories = request?.Categories?
            .Select(category => category?.Trim())
            .Where(category => !string.IsNullOrWhiteSpace(category))
            .Distinct(StringComparer.Ordinal)
            .Where(category => JobCategories.IsSupported(category!))
            .Cast<string>()
            .ToArray();

        return new GetContractorsRequest(
            normalizedQuery,
            normalizedCategories is { Length: > 0 } ? normalizedCategories : null,
            normalizedLocation,
            normalizedLegalType);
    }
}
