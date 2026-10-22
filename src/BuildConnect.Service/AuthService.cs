using System.Security.Cryptography;
using BuildConnect.Model;
using BuildConnect.Repository.Common;
using BuildConnect.Service.Common;
using Microsoft.AspNetCore.Identity;

namespace BuildConnect.Service;

public sealed class AuthService : IAuthService
{
    private readonly IAuthRepository _authRepository;
    private readonly IEmailVerificationSender _emailVerificationSender;
    private readonly IPasswordHasher<AuthAccount> _passwordHasher;
    private readonly IUserRepository _userRepository;

    public AuthService(
        IAuthRepository authRepository,
        IUserRepository userRepository,
        IEmailVerificationSender emailVerificationSender,
        IPasswordHasher<AuthAccount> passwordHasher)
    {
        _authRepository = authRepository;
        _userRepository = userRepository;
        _emailVerificationSender = emailVerificationSender;
        _passwordHasher = passwordHasher;
    }

    public AuthenticatedUserResponse Login(LoginRequest request)
    {
        ValidateLoginRequest(request);

        var email = request.Email.Trim();
        var normalizedEmail = EmailNormalizer.Normalize(email);
        var account = _authRepository.GetByEmail(normalizedEmail);
        var passwordVerificationResult = account is null
            ? PasswordVerificationResult.Failed
            : _passwordHasher.VerifyHashedPassword(account, account.PasswordHash, request.Password);

        if (account is null || passwordVerificationResult == PasswordVerificationResult.Failed)
        {
            throw new UnauthorizedAccessException("Neispravna email adresa ili lozinka.");
        }

        if (!account.IsEmailVerified)
        {
            throw new UnauthorizedAccessException("Email adresa nije potvrdjena. Provjerite svoj inbox.");
        }

        var user = _userRepository.GetById(account.UserId);
        if (user is null)
        {
            throw new ArgumentException("Korisnicki racun nije pronadjen.");
        }

        return MapToResponse(user);
    }

    public RegistrationResponse Register(RegisterRequest request)
    {
        ValidateRegisterRequest(request);

        var email = request.Email.Trim();
        var normalizedEmail = EmailNormalizer.Normalize(email);
        var existingAccount = _authRepository.GetByEmail(normalizedEmail);
        if (existingAccount is not null)
        {
            throw new ArgumentException("Korisnik s ovom email adresom vec postoji.");
        }

        var role = request.Role.Trim();
        var legalType = ResolveLegalType(role, request.LegalType);
        var displayName = $"{request.FirstName.Trim()} {request.LastName.Trim()}".Trim();
        var userId = Guid.NewGuid().ToString("N");
        var authAccount = new AuthAccount(userId, email, string.Empty, false);
        var passwordHash = _passwordHasher.HashPassword(authAccount, request.Password);
        var emailVerificationToken = CreateEmailVerificationToken();
        var emailVerificationTokenExpiresAt = DateTimeOffset.UtcNow.AddHours(24);

        var user = new UserProfile(
            userId,
            displayName,
            role,
            legalType,
            email,
            string.Empty,
            string.Empty,
            DateTimeOffset.UtcNow,
            role == BuildConnectRoles.Izvodjac ? [] : null,
            NormalizePhone(request.Phone),
            false);

        _userRepository.Create(user, passwordHash, emailVerificationToken, emailVerificationTokenExpiresAt);
        var isVerificationEmailSent = _emailVerificationSender.SendVerificationEmail(email, displayName, emailVerificationToken);

        return new RegistrationResponse(email, isVerificationEmailSent);
    }

    public bool VerifyEmail(VerifyEmailRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Email))
        {
            throw new ArgumentException("Email adresa je obavezna.");
        }

        if (string.IsNullOrWhiteSpace(request.Token))
        {
            throw new ArgumentException("Verifikacijski token je obavezan.");
        }

        return _userRepository.VerifyEmail(request.Email.Trim(), request.Token.Trim(), DateTimeOffset.UtcNow);
    }

    public RegistrationResponse ResendVerificationEmail(ResendVerificationEmailRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Email))
        {
            throw new ArgumentException("Email adresa je obavezna.");
        }

        var email = request.Email.Trim();
        var user = _userRepository.GetByEmail(email);
        var account = _authRepository.GetByEmail(email);

        if (user is null || account is null || account.IsEmailVerified)
        {
            return new RegistrationResponse(email, true);
        }

        var emailVerificationToken = CreateEmailVerificationToken();
        var emailVerificationTokenExpiresAt = DateTimeOffset.UtcNow.AddHours(24);
        var isTokenUpdated = _userRepository.UpdateEmailVerificationToken(email, emailVerificationToken, emailVerificationTokenExpiresAt);
        var isVerificationEmailSent = isTokenUpdated &&
            _emailVerificationSender.SendVerificationEmail(user.Email, user.DisplayName, emailVerificationToken);

        return new RegistrationResponse(user.Email, isVerificationEmailSent);
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

        if (string.IsNullOrWhiteSpace(request.Phone))
        {
            throw new ArgumentException("Kontakt broj je obavezan.");
        }

        if (request.Phone.Trim().Length < 6)
        {
            throw new ArgumentException("Kontakt broj mora imati barem 6 znakova.");
        }

        if (request.Phone.Trim().Length > 32)
        {
            throw new ArgumentException("Kontakt broj moze imati najvise 32 znaka.");
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
            user.LegalType,
            user.Phone,
            user.IsPhoneVisible);
    }

    private static string? NormalizePhone(string? phone)
    {
        var normalizedPhone = phone?.Trim();
        if (string.IsNullOrWhiteSpace(normalizedPhone))
        {
            throw new ArgumentException("Kontakt broj je obavezan.");
        }

        return normalizedPhone;
    }

    private static string CreateEmailVerificationToken()
    {
        return Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
    }
}
