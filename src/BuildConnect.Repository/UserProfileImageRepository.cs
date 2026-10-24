using BuildConnect.DAL;
using BuildConnect.DAL.Entities;
using BuildConnect.Model;
using BuildConnect.Repository.Common;
using Microsoft.EntityFrameworkCore;

namespace BuildConnect.Repository;

public sealed class UserProfileImageRepository : IUserProfileImageRepository
{
    private readonly BuildConnectDbContext _dbContext;

    public UserProfileImageRepository(BuildConnectDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<UserProfileImage?> GetAsync(string userId, string kind, CancellationToken cancellationToken = default)
    {
        return _dbContext.UserProfileImages
            .AsNoTracking()
            .Where(image => image.UserId == userId && image.Kind == kind)
            .Select(image => new UserProfileImage(image.Content, image.ContentType, image.Version))
            .SingleOrDefaultAsync(cancellationToken);
    }

    public async Task SaveAsync(string userId, string kind, UserProfileImage image, CancellationToken cancellationToken = default)
    {
        var user = await GetUserAsync(userId, cancellationToken);
        var entity = await _dbContext.UserProfileImages
            .SingleOrDefaultAsync(existingImage => existingImage.UserId == userId && existingImage.Kind == kind, cancellationToken);

        if (entity is null)
        {
            entity = new UserProfileImageEntity { UserId = userId, Kind = kind };
            _dbContext.UserProfileImages.Add(entity);
        }

        entity.Content = image.Content;
        entity.ContentType = image.ContentType;
        entity.Version = image.Version;
        entity.UpdatedAt = DateTimeOffset.UtcNow;
        SetVersion(user, kind, image.Version);

        // SaveChanges wraps the image and its public URL version in one transaction.
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(string userId, string kind, CancellationToken cancellationToken = default)
    {
        var user = await GetUserAsync(userId, cancellationToken);
        var entity = await _dbContext.UserProfileImages
            .SingleOrDefaultAsync(image => image.UserId == userId && image.Kind == kind, cancellationToken);

        if (entity is not null)
        {
            _dbContext.UserProfileImages.Remove(entity);
        }

        SetVersion(user, kind, null);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task<UserEntity> GetUserAsync(string userId, CancellationToken cancellationToken)
    {
        return await _dbContext.Users.SingleOrDefaultAsync(user => user.Id == userId, cancellationToken)
            ?? throw new KeyNotFoundException("Korisnik nije pronadjen.");
    }

    private static void SetVersion(UserEntity user, string kind, string? version)
    {
        if (kind == UserProfileImageKinds.Profile)
        {
            user.ProfileImageVersion = version;
        }
        else
        {
            user.CoverImageVersion = version;
        }
    }
}
