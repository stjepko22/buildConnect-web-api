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
        var normalizedEmail = EmailNormalizer.Normalize(email);
        var user = _dbContext.Users
            .AsNoTracking()
            .Where(user => user.NormalizedEmail == normalizedEmail)
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

    public IReadOnlyCollection<UserProfile> GetContractors(GetContractorsRequest? request = null)
    {
        var usersQuery = _dbContext.Users
            .AsNoTracking()
            .Where(user => user.Role == BuildConnectRoles.Izvodjac);

        var query = request?.Query?.Trim();
        if (!string.IsNullOrWhiteSpace(query))
        {
            usersQuery = usersQuery.Where(user =>
                user.DisplayName.Contains(query) ||
                user.Bio.Contains(query) ||
                user.Location.Contains(query) ||
                (user.ServiceCategoriesJson != null && user.ServiceCategoriesJson.Contains(query)));
        }

        if (!string.IsNullOrWhiteSpace(request?.Location))
        {
            usersQuery = usersQuery.Where(user => user.Location == request.Location);
        }

        if (!string.IsNullOrWhiteSpace(request?.LegalType))
        {
            usersQuery = usersQuery.Where(user => user.LegalType == request.LegalType);
        }

        var contractors = usersQuery
            .OrderBy(user => user.DisplayName)
            .ToArray()
            .Select(MapToModel)
            .ToArray();

        var categories = request?.Categories?
            .Where(category => !string.IsNullOrWhiteSpace(category))
            .Select(category => category.Trim())
            .ToArray();

        if (categories is not { Length: > 0 })
        {
            return contractors;
        }

        return contractors
            .Where(user => (user.ServiceCategories?.Any(serviceCategory => categories.Contains(serviceCategory, StringComparer.Ordinal)) ?? false))
            .ToArray();
    }

    public UserProfile Create(
        UserProfile user,
        string passwordHash,
        string? emailVerificationToken = null,
        DateTimeOffset? emailVerificationTokenExpiresAt = null)
    {
        var entity = new UserEntity
        {
            Id = user.Id,
            Email = user.Email,
            NormalizedEmail = EmailNormalizer.Normalize(user.Email),
            PasswordHash = passwordHash,
            IsEmailVerified = string.IsNullOrWhiteSpace(emailVerificationToken),
            EmailVerificationToken = emailVerificationToken,
            EmailVerificationTokenExpiresAt = emailVerificationTokenExpiresAt,
            DisplayName = user.DisplayName,
            Role = user.Role,
            LegalType = user.LegalType,
            Phone = user.Phone,
            IsPhoneVisible = user.IsPhoneVisible,
            Bio = user.Bio,
            Location = user.Location,
            JoinedAt = user.JoinedAt,
            ServiceCategoriesJson = SerializeServiceCategories(user.ServiceCategories)
        };

        _dbContext.Users.Add(entity);
        _dbContext.SaveChanges();

        return MapToModel(entity);
    }

    public bool VerifyEmail(string email, string token, DateTimeOffset verifiedAt)
    {
        var normalizedEmail = EmailNormalizer.Normalize(email);
        var entity = _dbContext.Users
            .Where(user => user.NormalizedEmail == normalizedEmail)
            .FirstOrDefault();

        if (entity is null)
        {
            return false;
        }

        if (entity.IsEmailVerified)
        {
            return true;
        }

        var isTokenValid =
            !string.IsNullOrWhiteSpace(entity.EmailVerificationToken) &&
            string.Equals(entity.EmailVerificationToken, token, StringComparison.Ordinal) &&
            entity.EmailVerificationTokenExpiresAt >= verifiedAt;

        if (!isTokenValid)
        {
            return false;
        }

        entity.IsEmailVerified = true;
        entity.EmailVerificationToken = null;
        entity.EmailVerificationTokenExpiresAt = null;

        _dbContext.SaveChanges();

        return true;
    }

    public bool UpdateEmailVerificationToken(string email, string token, DateTimeOffset expiresAt)
    {
        var normalizedEmail = EmailNormalizer.Normalize(email);
        var entity = _dbContext.Users
            .Where(user => user.NormalizedEmail == normalizedEmail)
            .FirstOrDefault();

        if (entity is null || entity.IsEmailVerified)
        {
            return false;
        }

        entity.EmailVerificationToken = token;
        entity.EmailVerificationTokenExpiresAt = expiresAt;

        _dbContext.SaveChanges();

        return true;
    }

    public UserProfile UpdateProfile(UserProfile user)
    {
        var entity = _dbContext.Users
            .Where(existingUser => existingUser.Id == user.Id)
            .FirstOrDefault();

        if (entity is null)
        {
            throw new KeyNotFoundException("Korisnik nije pronadjen.");
        }

        entity.DisplayName = user.DisplayName;
        entity.LegalType = user.LegalType;
        entity.Phone = user.Phone;
        entity.IsPhoneVisible = user.IsPhoneVisible;
        entity.Bio = user.Bio;
        entity.Location = user.Location;
        entity.ServiceCategoriesJson = SerializeServiceCategories(user.ServiceCategories);

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
            user.Phone,
            user.IsPhoneVisible);
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
