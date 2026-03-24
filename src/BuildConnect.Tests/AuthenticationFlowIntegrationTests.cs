using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using BuildConnect.Model;
using BuildConnect.Tests.Support;

namespace BuildConnect.Tests;

public sealed class AuthenticationFlowIntegrationTests : IClassFixture<IntegrationTestWebApplicationFactory>
{
    private readonly HttpClient _client;

    public AuthenticationFlowIntegrationTests(IntegrationTestWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Login_ReturnsJwtSession_ForSeededUser()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/login", new LoginRequest(
            "investitor@buildconnect.hr",
            "invest123"));

        var session = await response.ReadRequiredJsonAsync<AuthenticatedSessionResponse>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.False(string.IsNullOrWhiteSpace(session.AccessToken));
        Assert.Equal("investitor@buildconnect.hr", session.User.Email);
        Assert.Equal(BuildConnectRoles.Investitor, session.User.Role);
    }

    [Fact]
    public async Task ProtectedEndpoint_ReturnsUnauthorized_WithoutToken()
    {
        var response = await _client.PostAsJsonAsync("/api/jobs", new CreateJobRequest(
            "Test posao",
            "Detaljan opis posla za integration test.",
            "Fasade",
            "Zagreb",
            2500m,
            "2026-06-01"));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task ProtectedEndpoint_CreatesJob_WithValidJwtToken()
    {
        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", new LoginRequest(
            "investitor@buildconnect.hr",
            "invest123"));
        var session = await loginResponse.ReadRequiredJsonAsync<AuthenticatedSessionResponse>();

        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/jobs")
        {
            Content = JsonContent.Create(new CreateJobRequest(
                "Integration posao",
                "Detaljan opis integration oglasa za posao.",
                "Fasade",
                "Zagreb",
                4200m,
                "2026-06-15"))
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", session.AccessToken);

        var response = await _client.SendAsync(request);
        var createdJob = await response.ReadRequiredJsonAsync<JobResponse>();

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.Equal("Integration posao", createdJob.Title);
        Assert.Equal(session.User.Id, createdJob.InvestitorId);
    }
}
