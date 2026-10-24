using BuildConnect.Model;
using BuildConnect.Repository.Common;
using BuildConnect.Service;
using BuildConnect.Tests.Support;

namespace BuildConnect.Tests;

public sealed class UserProfileImageServiceTests
{
    [Theory]
    [InlineData("image/png")]
    [InlineData("image/jpeg")]
    [InlineData("image/webp")]
    public async Task Upload_UsesAuthenticatedOwner_AndPreservesPrivateProfileFields(string contentType)
    {
        var owner = TestData.CreateIzvodjac() with { Phone = "0919876543", IsPhoneVisible = false };
        var repository = new FakeImageRepository();
        var service = new UserProfileImageService(repository, new UserService(new FakeUserRepository([owner])));
        var bytes = contentType switch
        {
            "image/png" => UserProfileImageTestData.Png,
            "image/jpeg" => UserProfileImageTestData.Jpeg,
            _ => UserProfileImageTestData.Webp
        };
        using var stream = new MemoryStream(bytes);

        var result = await service.UploadCurrentUserImageAsync("profile", stream, contentType, new(owner.Id, owner.Role));

        var saved = Assert.Single(repository.Images);
        Assert.Equal((owner.Id, "profile"), saved.Key);
        Assert.Equal(bytes, saved.Value.Content);
        Assert.Equal(contentType, saved.Value.ContentType);
        Assert.Equal(32, saved.Value.Version.Length);
        Assert.Equal(owner.Phone, result.Phone);
        Assert.False(result.IsPhoneVisible);
        Assert.Equal(owner.DisplayName, result.DisplayName);
        Assert.Equal(owner.ServiceCategories, result.ServiceCategories);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("image/svg+xml")]
    [InlineData("application/octet-stream")]
    [InlineData("text/html")]
    public async Task Upload_RejectsUnsupportedDeclaredContentType(string? contentType)
    {
        var (service, repository, owner) = CreateService();
        using var stream = new MemoryStream(UserProfileImageTestData.Png);

        await Assert.ThrowsAsync<ArgumentException>(() => service.UploadCurrentUserImageAsync("profile", stream, contentType, owner));

        Assert.Empty(repository.Images);
    }

    [Theory]
    [InlineData("image/jpeg")]
    [InlineData("image/webp")]
    public async Task Upload_RejectsMismatchedMagicBytes(string contentType)
    {
        var (service, repository, owner) = CreateService();
        using var stream = new MemoryStream(UserProfileImageTestData.Png);

        await Assert.ThrowsAsync<ArgumentException>(() => service.UploadCurrentUserImageAsync("profile", stream, contentType, owner));

        Assert.Empty(repository.Images);
    }

    [Fact]
    public async Task Upload_RejectsEmptyFile()
    {
        var (service, repository, owner) = CreateService();
        using var stream = new MemoryStream();

        await Assert.ThrowsAsync<ArgumentException>(() => service.UploadCurrentUserImageAsync("profile", stream, "image/png", owner));

        Assert.Empty(repository.Images);
    }

    [Fact]
    public async Task Upload_StopsAfterLimitPlusOneByte_WithoutTrustingStreamLength()
    {
        var (service, repository, owner) = CreateService();
        using var stream = new EndlessStream();

        await Assert.ThrowsAsync<ArgumentException>(() => service.UploadCurrentUserImageAsync("cover", stream, "image/jpeg", owner));

        Assert.Equal(UserProfileImageLimits.MaxFileSize + 1, stream.BytesRead);
        Assert.Empty(repository.Images);
    }

    [Fact]
    public async Task Upload_RespectsCancellation()
    {
        var (service, repository, owner) = CreateService();
        using var stream = new MemoryStream(UserProfileImageTestData.Png);
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => service.UploadCurrentUserImageAsync("profile", stream, "image/png", owner, cancellation.Token));

        Assert.Empty(repository.Images);
    }

    [Fact]
    public async Task InvalidKind_AndMissingOwner_CannotPersistImages()
    {
        var (service, repository, owner) = CreateService();
        using var stream = new MemoryStream(UserProfileImageTestData.Png);

        await Assert.ThrowsAsync<ArgumentException>(() => service.UploadCurrentUserImageAsync("gallery", stream, "image/png", owner));
        await Assert.ThrowsAsync<ArgumentException>(() => service.GetAsync(owner.UserId, "gallery"));
        await Assert.ThrowsAsync<ArgumentException>(() => service.DeleteCurrentUserImageAsync("gallery", owner));
        await Assert.ThrowsAsync<KeyNotFoundException>(() => service.UploadCurrentUserImageAsync("profile", stream, "image/png", new("missing-user", owner.Role)));
        await Assert.ThrowsAsync<KeyNotFoundException>(() => service.DeleteCurrentUserImageAsync("profile", new("missing-user", owner.Role)));

        Assert.Empty(repository.Images);
    }

    private static (UserProfileImageService Service, FakeImageRepository Repository, RequestUserContext Owner) CreateService()
    {
        var owner = TestData.CreateIzvodjac();
        var repository = new FakeImageRepository();
        return (new UserProfileImageService(repository, new UserService(new FakeUserRepository([owner]))), repository, new(owner.Id, owner.Role));
    }

    private sealed class FakeImageRepository : IUserProfileImageRepository
    {
        public Dictionary<(string UserId, string Kind), UserProfileImage> Images { get; } = [];

        public Task<UserProfileImage?> GetAsync(string userId, string kind, CancellationToken cancellationToken = default)
            => Task.FromResult(Images.GetValueOrDefault((userId, kind)));

        public Task SaveAsync(string userId, string kind, UserProfileImage image, CancellationToken cancellationToken = default)
        {
            Images[(userId, kind)] = image;
            return Task.CompletedTask;
        }

        public Task DeleteAsync(string userId, string kind, CancellationToken cancellationToken = default)
        {
            Images.Remove((userId, kind));
            return Task.CompletedTask;
        }
    }

    private sealed class EndlessStream : Stream
    {
        public int BytesRead { get; private set; }
        public override bool CanRead => true;
        public override bool CanSeek => false;
        public override bool CanWrite => false;
        public override long Length => throw new NotSupportedException();
        public override long Position { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }
        public override int Read(byte[] buffer, int offset, int count)
        {
            Array.Fill(buffer, (byte)0xff, offset, count);
            BytesRead += count;
            return count;
        }
        public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            buffer.Span.Fill(0xff);
            BytesRead += buffer.Length;
            return ValueTask.FromResult(buffer.Length);
        }
        public override void Flush() => throw new NotSupportedException();
        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
        public override void SetLength(long value) => throw new NotSupportedException();
        public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
    }
}
