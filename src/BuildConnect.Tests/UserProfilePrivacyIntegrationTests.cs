using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using BuildConnect.Model;
using BuildConnect.Tests.Support;

namespace BuildConnect.Tests;

public sealed class UserProfilePrivacyIntegrationTests : IClassFixture<IntegrationTestWebApplicationFactory>
{
    private readonly HttpClient _client;

    public UserProfilePrivacyIntegrationTests(IntegrationTestWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task HiddenPhone_IsAvailableToOwner_AndRedactedFromPublicAndOtherUserResponses()
    {
        using var contractorLoginResponse = await _client.PostAsJsonAsync("/api/auth/login", new LoginRequest(
            "izvodjac@buildconnect.hr",
            "izvodjac123"));
        Assert.Equal(HttpStatusCode.OK, contractorLoginResponse.StatusCode);
        var contractorSession = await contractorLoginResponse.ReadRequiredJsonAsync<AuthenticatedSessionResponse>();
        Assert.Equal(BuildConnectRoles.Izvodjac, contractorSession.User.Role);

        const string phone = "0919876543";
        using var updateRequest = new HttpRequestMessage(HttpMethod.Put, "/api/users/me")
        {
            Content = JsonContent.Create(new UpdateUserProfileRequest(
                contractorSession.User.DisplayName,
                contractorSession.User.LegalType,
                "Profil s privatnim telefonom.",
                "Zagreb",
                ["Fasade"],
                phone,
                false))
        };
        updateRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", contractorSession.AccessToken);

        using var updateResponse = await _client.SendAsync(updateRequest);
        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);
        var updatedProfile = await updateResponse.ReadRequiredJsonAsync<UserProfileResponse>();
        Assert.Equal(contractorSession.User.Id, updatedProfile.Id);
        Assert.Equal(phone, updatedProfile.Phone);
        Assert.False(updatedProfile.IsPhoneVisible);

        var profileUrl = $"/api/users/{contractorSession.User.Id}";
        using var publicResponse = await _client.GetAsync(profileUrl);
        Assert.Equal(HttpStatusCode.OK, publicResponse.StatusCode);
        var publicProfile = await publicResponse.ReadRequiredJsonAsync<UserProfileResponse>();
        Assert.Equal(contractorSession.User.Id, publicProfile.Id);
        Assert.Null(publicProfile.Phone);
        Assert.False(publicProfile.IsPhoneVisible);

        using var usersResponse = await _client.GetAsync("/api/users");
        Assert.Equal(HttpStatusCode.OK, usersResponse.StatusCode);
        var users = await usersResponse.ReadRequiredJsonAsync<UserProfileResponse[]>();
        var listedUser = Assert.Single(users, user => user.Id == contractorSession.User.Id);
        Assert.Null(listedUser.Phone);
        Assert.False(listedUser.IsPhoneVisible);

        using var contractorsResponse = await _client.GetAsync("/api/contractors");
        Assert.Equal(HttpStatusCode.OK, contractorsResponse.StatusCode);
        var contractors = await contractorsResponse.ReadRequiredJsonAsync<UserProfileResponse[]>();
        var listedContractor = Assert.Single(contractors, user => user.Id == contractorSession.User.Id);
        Assert.Null(listedContractor.Phone);
        Assert.False(listedContractor.IsPhoneVisible);

        using var ownerRequest = new HttpRequestMessage(HttpMethod.Get, profileUrl);
        ownerRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", contractorSession.AccessToken);
        using var ownerResponse = await _client.SendAsync(ownerRequest);
        Assert.Equal(HttpStatusCode.OK, ownerResponse.StatusCode);
        var ownerProfile = await ownerResponse.ReadRequiredJsonAsync<UserProfileResponse>();
        Assert.Equal(contractorSession.User.Id, ownerProfile.Id);
        Assert.Equal(phone, ownerProfile.Phone);
        Assert.False(ownerProfile.IsPhoneVisible);

        using var investorLoginResponse = await _client.PostAsJsonAsync("/api/auth/login", new LoginRequest(
            "investitor@buildconnect.hr",
            "invest123"));
        Assert.Equal(HttpStatusCode.OK, investorLoginResponse.StatusCode);
        var investorSession = await investorLoginResponse.ReadRequiredJsonAsync<AuthenticatedSessionResponse>();
        Assert.Equal(BuildConnectRoles.Investitor, investorSession.User.Role);
        Assert.NotEqual(contractorSession.User.Id, investorSession.User.Id);

        using var otherUserRequest = new HttpRequestMessage(HttpMethod.Get, profileUrl);
        otherUserRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", investorSession.AccessToken);
        using var otherUserResponse = await _client.SendAsync(otherUserRequest);
        Assert.Equal(HttpStatusCode.OK, otherUserResponse.StatusCode);
        var otherUserProfile = await otherUserResponse.ReadRequiredJsonAsync<UserProfileResponse>();
        Assert.Equal(contractorSession.User.Id, otherUserProfile.Id);
        Assert.Null(otherUserProfile.Phone);
        Assert.False(otherUserProfile.IsPhoneVisible);
    }
}
