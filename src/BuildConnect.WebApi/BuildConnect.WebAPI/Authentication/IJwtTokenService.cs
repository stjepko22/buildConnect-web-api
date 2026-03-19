using BuildConnect.Model;

namespace BuildConnect.WebAPI.Authentication;

public interface IJwtTokenService
{
    string CreateToken(AuthenticatedUserResponse user);
}
