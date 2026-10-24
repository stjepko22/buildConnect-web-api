using System.Buffers;
using BuildConnect.Model;
using BuildConnect.Repository.Common;
using BuildConnect.Service.Common;

namespace BuildConnect.Service;

public sealed class UserProfileImageService : IUserProfileImageService
{
    private readonly IUserProfileImageRepository _imageRepository;
    private readonly IUserService _userService;

    public UserProfileImageService(IUserProfileImageRepository imageRepository, IUserService userService)
    {
        _imageRepository = imageRepository;
        _userService = userService;
    }

    public Task<UserProfileImage?> GetAsync(string userId, string kind, CancellationToken cancellationToken = default)
    {
        RequireKind(kind);
        return _imageRepository.GetAsync(userId, kind, cancellationToken);
    }

    public async Task<UserProfileResponse> UploadCurrentUserImageAsync(
        string kind,
        Stream content,
        string? contentType,
        RequestUserContext userContext,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(content);
        ArgumentNullException.ThrowIfNull(userContext);
        RequireKind(kind);
        GetOwner(userContext);

        var normalizedContentType = contentType?.Trim().ToLowerInvariant();
        if (normalizedContentType is not ("image/jpeg" or "image/png" or "image/webp"))
        {
            throw new ArgumentException("Odaberite JPG, PNG ili WebP fotografiju.");
        }

        var bytes = await ReadContentAsync(content, cancellationToken);
        if (!MatchesSignature(bytes, normalizedContentType))
        {
            throw new ArgumentException("Sadrzaj datoteke ne odgovara odabranom formatu fotografije.");
        }

        var image = new UserProfileImage(bytes, normalizedContentType, Guid.NewGuid().ToString("N"));
        await _imageRepository.SaveAsync(userContext.UserId, kind, image, cancellationToken);
        return GetOwner(userContext);
    }

    public async Task<UserProfileResponse> DeleteCurrentUserImageAsync(
        string kind,
        RequestUserContext userContext,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(userContext);
        RequireKind(kind);
        GetOwner(userContext);
        await _imageRepository.DeleteAsync(userContext.UserId, kind, cancellationToken);
        return GetOwner(userContext);
    }

    private UserProfileResponse GetOwner(RequestUserContext userContext)
    {
        return _userService.GetUserById(userContext.UserId, userContext)
            ?? throw new KeyNotFoundException("Korisnik nije pronadjen.");
    }

    private static void RequireKind(string kind)
    {
        if (!UserProfileImageKinds.IsSupported(kind))
        {
            throw new ArgumentException("Odaberite profilnu ili naslovnu fotografiju.");
        }
    }

    private static async Task<byte[]> ReadContentAsync(Stream content, CancellationToken cancellationToken)
    {
        using var output = new MemoryStream();
        var buffer = ArrayPool<byte>.Shared.Rent(81920);
        try
        {
            while (true)
            {
                // Read at most one byte beyond the limit, even when stream Length is unavailable or incorrect.
                var remaining = UserProfileImageLimits.MaxFileSize - (int)output.Length;
                var count = await content.ReadAsync(buffer.AsMemory(0, Math.Min(buffer.Length, remaining + 1)), cancellationToken);
                if (count == 0)
                {
                    break;
                }

                if (count > remaining)
                {
                    throw new ArgumentException("Fotografija moze imati najvise 5 MB.");
                }

                await output.WriteAsync(buffer.AsMemory(0, count), cancellationToken);
            }

            if (output.Length == 0)
            {
                throw new ArgumentException("Odaberite fotografiju koja nije prazna.");
            }

            return output.ToArray();
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buffer);
        }
    }

    private static bool MatchesSignature(ReadOnlySpan<byte> content, string contentType)
    {
        return contentType switch
        {
            "image/jpeg" => content.StartsWith(new byte[] { 0xff, 0xd8, 0xff }),
            "image/png" => content.StartsWith(new byte[] { 0x89, 0x50, 0x4e, 0x47, 0x0d, 0x0a, 0x1a, 0x0a }),
            "image/webp" => content.Length >= 12 && content[..4].SequenceEqual("RIFF"u8) && content.Slice(8, 4).SequenceEqual("WEBP"u8),
            _ => false
        };
    }
}
