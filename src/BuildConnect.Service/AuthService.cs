using BuildConnect.Model;
using BuildConnect.Repository.Common;
using BuildConnect.Service.Common;

namespace BuildConnect.Service;

public sealed class AuthService : IAuthService
{
    private readonly IAuthRepository _authRepository;
    private readonly IUserRepository _userRepository;

    public AuthService(IAuthRepository authRepository, IUserRepository userRepository)
    {
        _authRepository = authRepository;
        _userRepository = userRepository;
    }

    public AuthenticatedUserResponse Login(LoginRequest request)
    {
        ValidateLoginRequest(request);

        var normalizedEmail = request.Email.Trim();
        var account = _authRepository.GetByEmail(normalizedEmail);
        if (account is null || !string.Equals(account.Password, request.Password, StringComparison.Ordinal))
        {
            throw new UnauthorizedAccessException("Neispravna email adresa ili lozinka.");
        }

        var user = _userRepository.GetById(account.UserId);
        if (user is null)
        {
            throw new ArgumentException("Korisnicki racun nije pronadjen.");
        }

        return MapToResponse(user);
    }

    public AuthenticatedUserResponse Register(RegisterRequest request)
    {
        ValidateRegisterRequest(request);

        var normalizedEmail = request.Email.Trim();
        var existingAccount = _authRepository.GetByEmail(normalizedEmail);
        if (existingAccount is not null)
        {
            throw new ArgumentException("Korisnik s ovom email adresom vec postoji.");
        }

        var role = request.Role.Trim();
        var legalType = ResolveLegalType(role, request.LegalType);
        var displayName = $"{request.FirstName.Trim()} {request.LastName.Trim()}".Trim();
        var userId = Guid.NewGuid().ToString("N");

        var user = new UserProfile(
            userId,
            displayName,
            role,
            legalType,
            normalizedEmail,
            string.Empty,
            string.Empty,
            DateTimeOffset.UtcNow,
            role == BuildConnectRoles.Izvodjac ? [] : null);

        var createdUser = _userRepository.Create(user);
        _authRepository.Create(new AuthAccount(createdUser.Id, normalizedEmail, request.Password));

        return MapToResponse(createdUser);
    }

    private static void ValidateLoginRequest(LoginRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Email))
        {
            throw new ArgumentException("Email adresa je obavezna.");
        }

        if (string.IsNullOrWhiteSpace(request.Password))
        {
            throw new ArgumentException("Lozinka je obavezna.");
        }
    }

    private static void ValidateRegisterRequest(RegisterRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.FirstName))
        {
            throw new ArgumentException("Ime je obavezno.");
        }

        if (string.IsNullOrWhiteSpace(request.LastName))
        {
            throw new ArgumentException("Prezime je obavezno.");
        }

        if (string.IsNullOrWhiteSpace(request.Email))
        {
            throw new ArgumentException("Email adresa je obavezna.");
        }

        if (string.IsNullOrWhiteSpace(request.Password) || request.Password.Length < 6)
        {
            throw new ArgumentException("Lozinka mora imati barem 6 znakova.");
        }

        if (!string.Equals(request.Role, BuildConnectRoles.Investitor, StringComparison.Ordinal)
            && !string.Equals(request.Role, BuildConnectRoles.Izvodjac, StringComparison.Ordinal))
        {
            throw new ArgumentException("Odabrana korisnicka uloga nije podrzana.");
        }
    }

    private static string ResolveLegalType(string role, string? legalType)
    {
        if (string.Equals(role, BuildConnectRoles.Investitor, StringComparison.Ordinal))
        {
            return BuildConnectLegalTypes.Firma;
        }

        if (string.IsNullOrWhiteSpace(legalType) || !BuildConnectLegalTypes.IsSupported(legalType.Trim()))
        {
            throw new ArgumentException("Odabrani pravni oblik nije podrzan.");
        }

        return legalType.Trim();
    }

    private static AuthenticatedUserResponse MapToResponse(UserProfile user)
    {
        return new AuthenticatedUserResponse(
            user.Id,
            user.Email,
            user.DisplayName,
            user.Role,
            user.LegalType);
    }
}
