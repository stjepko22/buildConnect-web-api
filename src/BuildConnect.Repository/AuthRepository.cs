using BuildConnect.DAL;
using BuildConnect.Model;
using BuildConnect.Repository.Common;

namespace BuildConnect.Repository;

public sealed class AuthRepository : IAuthRepository
{
    private readonly InMemoryAuthDataStore _authDataStore;

    public AuthRepository(InMemoryAuthDataStore authDataStore)
    {
        _authDataStore = authDataStore;
    }

    public AuthAccount? GetByEmail(string email)
    {
        return _authDataStore.GetByEmail(email);
    }

    public AuthAccount Create(AuthAccount account)
    {
        return _authDataStore.Add(account);
    }
}
