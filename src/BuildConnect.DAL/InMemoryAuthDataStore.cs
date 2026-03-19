using BuildConnect.Model;

namespace BuildConnect.DAL;

public sealed class InMemoryAuthDataStore
{
    private readonly object _syncRoot = new();
    private readonly List<AuthAccount> _accounts =
    [
        new("investitor-1", "investitor@buildconnect.hr", "invest123"),
        new("izvodjac-1", "izvodjac@buildconnect.hr", "izvodjac123")
    ];

    public AuthAccount? GetByEmail(string email)
    {
        lock (_syncRoot)
        {
            return _accounts.FirstOrDefault(account => string.Equals(account.Email, email, StringComparison.OrdinalIgnoreCase));
        }
    }

    public AuthAccount Add(AuthAccount account)
    {
        lock (_syncRoot)
        {
            _accounts.Add(account);
            return account;
        }
    }
}
