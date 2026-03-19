using BuildConnect.DAL;
using BuildConnect.Model;
using BuildConnect.Repository.Common;

namespace BuildConnect.Repository;

public sealed class UserRepository : IUserRepository
{
    private readonly InMemoryUserDataStore _userDataStore;

    public UserRepository(InMemoryUserDataStore userDataStore)
    {
        _userDataStore = userDataStore;
    }

    public IReadOnlyCollection<UserProfile> GetAll()
    {
        return _userDataStore.GetAll();
    }

    public UserProfile? GetById(string id)
    {
        return _userDataStore.GetById(id);
    }

    public UserProfile? GetByEmail(string email)
    {
        return _userDataStore.GetByEmail(email);
    }

    public IReadOnlyCollection<UserProfile> GetByRole(string role)
    {
        return _userDataStore.GetByRole(role);
    }

    public UserProfile Create(UserProfile user)
    {
        return _userDataStore.Add(user);
    }
}
