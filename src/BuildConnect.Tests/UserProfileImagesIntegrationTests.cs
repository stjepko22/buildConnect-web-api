using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using BuildConnect.DAL;
using BuildConnect.Model;
using BuildConnect.Tests.Support;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace BuildConnect.Tests;

public sealed class UserProfileImagesIntegrationTests : IClassFixture<IntegrationTestWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly IntegrationTestWebApplicationFactory _factory;

    public UserProfileImagesIntegrationTests(IntegrationTestWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public void MigrationSnapshot_MatchesCurrentModel()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<BuildConnectDbContext>();
        Assert.False(context.Database.HasPendingModelChanges());
    }

    [Fact]
    public async Task Images_CanBeUploadedReadReplacedAndDeleted_WithoutChangingOtherProfileFields()
    {
        var session = await LoginAsync("izvodjac@buildconnect.hr", "izvodjac123");
        var updatedProfileRequest = new UpdateUserProfileRequest(
            session.User.DisplayName, session.User.LegalType, "Profil s fotografijama.", "Zagreb", ["Fasade"], "0919876543", false);
        using var updateRequest = new HttpRequestMessage(HttpMethod.Put, "/api/users/me")
        {
            Content = JsonContent.Create(updatedProfileRequest)
        };
        updateRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", session.AccessToken);
        using var updateResponse = await _client.SendAsync(updateRequest);
        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);
        var beforeUpload = await updateResponse.ReadRequiredJsonAsync<UserProfileResponse>();

        using var profileUpload = await UploadAsync(session, "profile", UserProfileImageTestData.Png, "image/png");
        Assert.Equal(HttpStatusCode.OK, profileUpload.StatusCode);
        var withProfile = await profileUpload.ReadRequiredJsonAsync<UserProfileResponse>();
        AssertProfileUnchanged(beforeUpload, withProfile);
        Assert.NotNull(withProfile.ProfileImageUrl);
        Assert.StartsWith($"/api/users/{session.User.Id}/images/profile?v=", withProfile.ProfileImageUrl);

        using var coverUpload = await UploadAsync(session, "cover", UserProfileImageTestData.Jpeg, "image/jpeg");
        Assert.Equal(HttpStatusCode.OK, coverUpload.StatusCode);
        var withBoth = await coverUpload.ReadRequiredJsonAsync<UserProfileResponse>();
        AssertProfileUnchanged(beforeUpload, withBoth);
        Assert.Equal(withProfile.ProfileImageUrl, withBoth.ProfileImageUrl);
        Assert.NotNull(withBoth.CoverImageUrl);
        await AssertImageAsync(withBoth.ProfileImageUrl!, UserProfileImageTestData.Png, "image/png");
        await AssertImageAsync(withBoth.CoverImageUrl!, UserProfileImageTestData.Jpeg, "image/jpeg");

        using var publicResponse = await _client.GetAsync($"/api/users/{session.User.Id}");
        var publicProfile = await publicResponse.ReadRequiredJsonAsync<UserProfileResponse>();
        Assert.Null(publicProfile.Phone);
        Assert.False(publicProfile.IsPhoneVisible);
        Assert.Equal(withBoth.ProfileImageUrl, publicProfile.ProfileImageUrl);
        Assert.Equal(withBoth.CoverImageUrl, publicProfile.CoverImageUrl);

        using var contractorsResponse = await _client.GetAsync("/api/contractors");
        var contractors = await contractorsResponse.ReadRequiredJsonAsync<UserProfileResponse[]>();
        var listed = Assert.Single(contractors, user => user.Id == session.User.Id);
        Assert.Equal(withBoth.ProfileImageUrl, listed.ProfileImageUrl);
        Assert.Equal(withBoth.CoverImageUrl, listed.CoverImageUrl);
        Assert.Null(listed.Phone);

        // An ordinary profile edit must not overwrite the image versions.
        using var editAgain = new HttpRequestMessage(HttpMethod.Put, "/api/users/me")
        {
            Content = JsonContent.Create(updatedProfileRequest with { Bio = "Novi opis bez promjene fotografija." })
        };
        editAgain.Headers.Authorization = new AuthenticationHeaderValue("Bearer", session.AccessToken);
        using var editResponse = await _client.SendAsync(editAgain);
        Assert.Equal(HttpStatusCode.OK, editResponse.StatusCode);
        var afterEdit = await editResponse.ReadRequiredJsonAsync<UserProfileResponse>();
        Assert.Equal(withBoth.ProfileImageUrl, afterEdit.ProfileImageUrl);
        Assert.Equal(withBoth.CoverImageUrl, afterEdit.CoverImageUrl);

        using var replacement = await UploadAsync(session, "profile", UserProfileImageTestData.Webp, "image/webp");
        Assert.Equal(HttpStatusCode.OK, replacement.StatusCode);
        var replaced = await replacement.ReadRequiredJsonAsync<UserProfileResponse>();
        Assert.NotEqual(withBoth.ProfileImageUrl, replaced.ProfileImageUrl);
        Assert.Equal(withBoth.CoverImageUrl, replaced.CoverImageUrl);
        AssertProfileUnchanged(afterEdit, replaced);
        await AssertImageAsync(replaced.ProfileImageUrl!, UserProfileImageTestData.Webp, "image/webp");

        using var oldVersion = await _client.GetAsync(withProfile.ProfileImageUrl);
        Assert.Equal(HttpStatusCode.OK, oldVersion.StatusCode);
        Assert.True(oldVersion.Headers.CacheControl?.NoCache);

        using var profileDelete = await DeleteAsync(session, "profile");
        Assert.Equal(HttpStatusCode.OK, profileDelete.StatusCode);
        var afterDelete = await profileDelete.ReadRequiredJsonAsync<UserProfileResponse>();
        Assert.Null(afterDelete.ProfileImageUrl);
        Assert.Equal(replaced.CoverImageUrl, afterDelete.CoverImageUrl);
        AssertProfileUnchanged(afterEdit, afterDelete);
        using var deletedImage = await _client.GetAsync(replaced.ProfileImageUrl);
        Assert.Equal(HttpStatusCode.NotFound, deletedImage.StatusCode);
        await AssertImageAsync(afterDelete.CoverImageUrl!, UserProfileImageTestData.Jpeg, "image/jpeg");

        using var repeatDelete = await DeleteAsync(session, "profile");
        Assert.Equal(HttpStatusCode.OK, repeatDelete.StatusCode);
        using var coverDelete = await DeleteAsync(session, "cover");
        Assert.Equal(HttpStatusCode.OK, coverDelete.StatusCode);
        var withoutImages = await coverDelete.ReadRequiredJsonAsync<UserProfileResponse>();
        Assert.Null(withoutImages.CoverImageUrl);
        Assert.Null(withoutImages.ProfileImageUrl);
    }

    [Fact]
    public async Task AnonymousUsers_CannotUploadOrDelete_AndCannotChooseAnotherOwner()
    {
        using var upload = await UploadAsync(null, "profile", UserProfileImageTestData.Png, "image/png");
        Assert.Equal(HttpStatusCode.Unauthorized, upload.StatusCode);
        using var deletion = await _client.DeleteAsync("/api/users/me/images/profile");
        Assert.Equal(HttpStatusCode.Unauthorized, deletion.StatusCode);

        var session = await LoginAsync("investitor@buildconnect.hr", "invest123");
        using var forbiddenOwnerRequest = new HttpRequestMessage(HttpMethod.Delete, "/api/users/other-user/images/profile");
        forbiddenOwnerRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", session.AccessToken);
        using var forbiddenOwnerResponse = await _client.SendAsync(forbiddenOwnerRequest);
        Assert.Equal(HttpStatusCode.MethodNotAllowed, forbiddenOwnerResponse.StatusCode);

        using var ownUpload = await UploadAsync(session, "profile", UserProfileImageTestData.Png, "image/png");
        Assert.Equal(HttpStatusCode.OK, ownUpload.StatusCode);
        var ownProfile = await ownUpload.ReadRequiredJsonAsync<UserProfileResponse>();
        Assert.StartsWith($"/api/users/{session.User.Id}/images/profile?v=", ownProfile.ProfileImageUrl!);
        using var ownDelete = await DeleteAsync(session, "profile");
        Assert.Equal(HttpStatusCode.OK, ownDelete.StatusCode);
    }

    [Theory]
    [InlineData("image/svg+xml", false)]
    [InlineData("image/jpeg", false)]
    [InlineData("image/png", true)]
    public async Task InvalidUploads_ReturnBadRequest(string contentType, bool empty)
    {
        var session = await LoginAsync("izvodjac@buildconnect.hr", "izvodjac123");
        using var response = await UploadAsync(session, "profile", empty ? [] : UserProfileImageTestData.Png, contentType);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task OversizedUpload_ReturnsBadRequest()
    {
        var session = await LoginAsync("izvodjac@buildconnect.hr", "izvodjac123");
        var bytes = new byte[UserProfileImageLimits.MaxFileSize + 1];
        UserProfileImageTestData.Png.CopyTo(bytes, 0);
        using var response = await UploadAsync(session, "cover", bytes, "image/png");
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task MissingFileInvalidKindAndUnknownImage_ReturnExpectedErrors()
    {
        var session = await LoginAsync("izvodjac@buildconnect.hr", "izvodjac123");
        using var missingFile = new HttpRequestMessage(HttpMethod.Post, "/api/users/me/images/profile")
        {
            Content = new MultipartFormDataContent()
        };
        missingFile.Headers.Authorization = new AuthenticationHeaderValue("Bearer", session.AccessToken);
        using var response = await _client.SendAsync(missingFile);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        using var invalid = await UploadAsync(session, "gallery", UserProfileImageTestData.Png, "image/png");
        Assert.Equal(HttpStatusCode.BadRequest, invalid.StatusCode);
        using var missing = await _client.GetAsync("/api/users/missing-user/images/profile");
        Assert.Equal(HttpStatusCode.NotFound, missing.StatusCode);
    }

    private async Task<AuthenticatedSessionResponse> LoginAsync(string email, string password)
    {
        using var response = await _client.PostAsJsonAsync("/api/auth/login", new LoginRequest(email, password));
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        return await response.ReadRequiredJsonAsync<AuthenticatedSessionResponse>();
    }

    private async Task<HttpResponseMessage> UploadAsync(AuthenticatedSessionResponse? session, string kind, byte[] bytes, string contentType)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, $"/api/users/me/images/{kind}")
        {
            Content = new MultipartFormDataContent()
        };
        var file = new ByteArrayContent(bytes);
        file.Headers.ContentType = new MediaTypeHeaderValue(contentType);
        ((MultipartFormDataContent)request.Content).Add(file, "file", "test-image");
        if (session is not null)
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", session.AccessToken);
        }

        return await _client.SendAsync(request);
    }

    private async Task<HttpResponseMessage> DeleteAsync(AuthenticatedSessionResponse session, string kind)
    {
        using var request = new HttpRequestMessage(HttpMethod.Delete, $"/api/users/me/images/{kind}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", session.AccessToken);
        return await _client.SendAsync(request);
    }

    private async Task AssertImageAsync(string url, byte[] expectedContent, string contentType)
    {
        using var response = await _client.GetAsync(url);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(contentType, response.Content.Headers.ContentType?.MediaType);
        Assert.Equal(expectedContent, await response.Content.ReadAsByteArrayAsync());
        Assert.Equal("nosniff", Assert.Single(response.Headers.GetValues("X-Content-Type-Options")));
        Assert.True(response.Headers.CacheControl?.Public);
        Assert.Equal(TimeSpan.FromHours(1), response.Headers.CacheControl?.MaxAge);
    }

    private static void AssertProfileUnchanged(UserProfileResponse expected, UserProfileResponse actual)
    {
        Assert.Equal(expected.Id, actual.Id);
        Assert.Equal(expected.DisplayName, actual.DisplayName);
        Assert.Equal(expected.Role, actual.Role);
        Assert.Equal(expected.LegalType, actual.LegalType);
        Assert.Equal(expected.Email, actual.Email);
        Assert.Equal(expected.Bio, actual.Bio);
        Assert.Equal(expected.Location, actual.Location);
        Assert.Equal(expected.JoinedAt, actual.JoinedAt);
        Assert.Equal(expected.ServiceCategories, actual.ServiceCategories);
        Assert.Equal(expected.Phone, actual.Phone);
        Assert.Equal(expected.IsPhoneVisible, actual.IsPhoneVisible);
    }
}
