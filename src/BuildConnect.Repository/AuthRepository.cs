using BuildConnect.DAL;
using BuildConnect.Model;
using BuildConnect.Repository.Common;
using Microsoft.EntityFrameworkCore;

namespace BuildConnect.Repository;

public sealed class AuthRepository : IAuthRepository
{
    private readonly BuildConnectDbContext _dbContext;

    public AuthRepository(BuildConnectDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public AuthAccount? GetByEmail(string email)
    {
        return _dbContext.Users
            .AsNoTracking()
            .Where(user => user.Email == email)
            .Select(user => new AuthAccount(
                user.Id,
                user.Email,
                user.PasswordHash))
            .FirstOrDefault();
    }
}
