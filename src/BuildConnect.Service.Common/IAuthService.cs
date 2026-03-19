using BuildConnect.Model;

namespace BuildConnect.Service.Common;

public interface IAuthService
{
    AuthenticatedUserResponse Login(LoginRequest request);

    AuthenticatedUserResponse Register(RegisterRequest request);
}
