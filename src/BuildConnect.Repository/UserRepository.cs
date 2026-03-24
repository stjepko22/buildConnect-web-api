using System.Text.Json;
using BuildConnect.DAL;
using BuildConnect.DAL.Entities;
using BuildConnect.Model;
using BuildConnect.Repository.Common;
using Microsoft.EntityFrameworkCore;

namespace BuildConnect.Repository;

public sealed class UserRepository : IUserRepository
{
    private readonly BuildConnectDbContext _dbContext;

    public UserRepository(BuildConnectDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public IReadOnlyCollection<UserProfile> GetAll()
    {
        return _dbContext.Users
            .AsNoTracking()
            .OrderBy(user => user.DisplayName)
            .ToArray()
            .Select(MapToModel)
            .ToArray();
    }

    public UserProfile? GetById(string id)
    {
        var user = _dbContext.Users
            .AsNoTracking()
            .Where(user => user.Id == id)
            .FirstOrDefault();

        return user is null ? null : MapToModel(user);
    }

    public UserProfile? GetByEmail(string email)
    {
        var user = _dbContext.Users
            .AsNoTracking()
            .Where(user => user.Email == email)
            .FirstOrDefault();

        return user is null ? null : MapToModel(user);
    }

    public IReadOnlyCollection<UserProfile> GetByRole(string role)
    {
        return _dbContext.Users
            .AsNoTracking()
            .Where(user => user.Role == role)
            .OrderBy(user => user.DisplayName)
            .ToArray()
            .Select(MapToModel)
            .ToArray();
    }

    public UserProfile Create(UserProfile user)
    {
        var entity = new UserEntity
        {
            Id = user.Id,
            Email = user.Email,
            PasswordHash = user.PasswordHash,
            DisplayName = user.DisplayName,
            Role = user.Role,
            LegalType = user.LegalType,
            Bio = user.Bio,
            Location = user.Location,
            JoinedAt = user.JoinedAt,
            ServiceCategoriesJson = SerializeServiceCategories(user.ServiceCategories)
        };

        _dbContext.Users.Add(entity);
        _dbContext.SaveChanges();

        return MapToModel(entity);
    }

    private static UserProfile MapToModel(UserEntity user)
    {
        return new UserProfile(
            user.Id,
            user.DisplayName,
            user.Role,
            user.LegalType,
            user.Email,
            user.Bio,
            user.Location,
            user.JoinedAt,
            DeserializeServiceCategories(user.ServiceCategoriesJson),
            user.PasswordHash);
    }

    private static IReadOnlyCollection<string>? DeserializeServiceCategories(string? serviceCategoriesJson)
    {
        if (string.IsNullOrWhiteSpace(serviceCategoriesJson))
        {
            return null;
        }

        return JsonSerializer.Deserialize<string[]>(serviceCategoriesJson);
    }

    private static string? SerializeServiceCategories(IReadOnlyCollection<string>? serviceCategories)
    {
        return serviceCategories is null ? null : JsonSerializer.Serialize(serviceCategories);
    }
}
