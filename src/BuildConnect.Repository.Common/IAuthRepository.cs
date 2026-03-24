using BuildConnect.Model;

namespace BuildConnect.Repository.Common;

public interface IAuthRepository
{
    AuthAccount? GetByEmail(string email);
}
