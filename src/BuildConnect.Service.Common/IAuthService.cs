using BuildConnect.Model;

namespace BuildConnect.Service.Common;

public interface IAuthService
{
    AuthenticatedUserResponse Login(LoginRequest request);

    RegistrationResponse Register(RegisterRequest request);

    bool VerifyEmail(VerifyEmailRequest request);

    RegistrationResponse ResendVerificationEmail(ResendVerificationEmailRequest request);
}
