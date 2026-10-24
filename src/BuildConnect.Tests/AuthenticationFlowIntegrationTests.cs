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
    public async Task GetJobs_FiltersJobs_ByQueryAndCategory()
    {
        var response = await _client.GetAsync("/api/jobs?q=fasad&category=Fasade");
        var jobs = await response.ReadRequiredJsonAsync<JobResponse[]>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotEmpty(jobs);
        Assert.All(jobs, job => Assert.Equal("Fasade", job.Category));
        Assert.All(jobs, job =>
            Assert.Contains(
                "fasad",
                string.Join(' ', job.Title, job.Description, job.Location, job.Category),
                StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task GetContractors_FiltersContractors_ByQueryCategoryLocationAndLegalType()
    {
        var response = await _client.GetAsync("/api/contractors?q=elektro&category=Elektro&location=Zadar&legalType=FIRMA");
        var contractors = await response.ReadRequiredJsonAsync<UserProfileResponse[]>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotEmpty(contractors);
        Assert.All(contractors, contractor => Assert.Equal("Zadar", contractor.Location));
        Assert.All(contractors, contractor => Assert.Equal(BuildConnectLegalTypes.Firma, contractor.LegalType));
        Assert.All(contractors, contractor => Assert.Contains("Elektro", contractor.ServiceCategories ?? []));
    }

    [Fact]
    public async Task GetDashboard_ReturnsInvestorDashboard_ForAuthenticatedUser()
    {
        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", new LoginRequest(
            "investitor@buildconnect.hr",
            "invest123"));
        var session = await loginResponse.ReadRequiredJsonAsync<AuthenticatedSessionResponse>();

        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/dashboard");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", session.AccessToken);

        var response = await _client.SendAsync(request);
        var dashboard = await response.ReadRequiredJsonAsync<DashboardResponse>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.True(dashboard.Summary.JobsCount > 0);
        Assert.NotNull(dashboard.JobActivities);
        Assert.NotEmpty(dashboard.JobActivities!);
        Assert.NotNull(dashboard.RecommendedContractors);
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

    [Fact]
    public async Task ProtectedEndpoint_UpdatesCurrentUserProfile_WithValidJwtToken()
    {
        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", new LoginRequest(
            "izvodjac@buildconnect.hr",
            "izvodjac123"));
        var session = await loginResponse.ReadRequiredJsonAsync<AuthenticatedSessionResponse>();

        using var request = new HttpRequestMessage(HttpMethod.Put, "/api/users/me")
        {
            Content = JsonContent.Create(new UpdateUserProfileRequest(
                "Novo Ime Izvodjaca",
                BuildConnectLegalTypes.Firma,
                "Azurirani profil za integration test.",
                "Osijek",
                ["Elektro", "Fasade"],
                "0910001111",
                true))
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", session.AccessToken);

        var response = await _client.SendAsync(request);
        var updatedProfile = await response.ReadRequiredJsonAsync<UserProfileResponse>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("Novo Ime Izvodjaca", updatedProfile.DisplayName);
        Assert.Equal("0910001111", updatedProfile.Phone);
        Assert.True(updatedProfile.IsPhoneVisible);
        Assert.Equal("Osijek", updatedProfile.Location);
        Assert.Equal(["Elektro", "Fasade"], updatedProfile.ServiceCategories);
    }

    [Fact]
    public async Task ProtectedEndpoint_UpdatesOwnedJob_WithValidJwtToken()
    {
        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", new LoginRequest(
            "investitor@buildconnect.hr",
            "invest123"));
        var session = await loginResponse.ReadRequiredJsonAsync<AuthenticatedSessionResponse>();

        using var request = new HttpRequestMessage(HttpMethod.Put, "/api/jobs/posao-3")
        {
            Content = JsonContent.Create(new UpdateJobRequest(
                "Azuriran integration posao",
                "Azurirani detaljan opis za integration test.",
                "Renovacija",
                "Velika Gorica",
                4700m,
                "2026-08-01"))
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", session.AccessToken);

        var response = await _client.SendAsync(request);
        var updatedJob = await response.ReadRequiredJsonAsync<JobResponse>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("Azuriran integration posao", updatedJob.Title);
        Assert.Equal("Renovacija", updatedJob.Category);
        Assert.Equal("Velika Gorica", updatedJob.Location);
        Assert.Equal(session.User.Id, updatedJob.InvestitorId);
    }

    [Fact]
    public async Task ProtectedEndpoints_CloseAndDeleteOwnedOpenJob_WithValidJwtToken()
    {
        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", new LoginRequest(
            "investitor@buildconnect.hr",
            "invest123"));
        var session = await loginResponse.ReadRequiredJsonAsync<AuthenticatedSessionResponse>();

        using var createRequest = new HttpRequestMessage(HttpMethod.Post, "/api/jobs")
        {
            Content = JsonContent.Create(new CreateJobRequest(
                "Lifecycle integration posao",
                "Detaljan opis oglasa za close i delete integration test.",
                "Fasade",
                "Zapresic",
                3100m,
                "2026-09-01"))
        };
        createRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", session.AccessToken);

        var createResponse = await _client.SendAsync(createRequest);
        var createdJob = await createResponse.ReadRequiredJsonAsync<JobResponse>();

        using var closeRequest = new HttpRequestMessage(HttpMethod.Post, $"/api/jobs/{createdJob.Id}/close")
        {
            Content = JsonContent.Create(new { })
        };
        closeRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", session.AccessToken);

        var closeResponse = await _client.SendAsync(closeRequest);
        var closedJob = await closeResponse.ReadRequiredJsonAsync<JobResponse>();

        using var deleteRequest = new HttpRequestMessage(HttpMethod.Delete, $"/api/jobs/{createdJob.Id}");
        deleteRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", session.AccessToken);

        var deleteResponse = await _client.SendAsync(deleteRequest);

        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        Assert.Equal(JobStatuses.Open, createdJob.Status);
        Assert.Equal(HttpStatusCode.OK, closeResponse.StatusCode);
        Assert.Equal(JobStatuses.Closed, closedJob.Status);
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);
    }

    [Fact]
    public async Task ProtectedEndpoints_AllowOwnerReview_OnlyAfterAcceptedContractorCompletesJob()
    {
        var investorLoginResponse = await _client.PostAsJsonAsync("/api/auth/login", new LoginRequest(
            "investitor@buildconnect.hr",
            "invest123"));
        var investorSession = await investorLoginResponse.ReadRequiredJsonAsync<AuthenticatedSessionResponse>();

        using var createJobRequest = new HttpRequestMessage(HttpMethod.Post, "/api/jobs")
        {
            Content = JsonContent.Create(new CreateJobRequest(
                "Integration posao za zavrsetak",
                "Detaljan opis oglasa za complete flow integration test.",
                "Fasade",
                "Karlovac",
                3600m,
                "2026-10-01"))
        };
        createJobRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", investorSession.AccessToken);

        var createJobResponse = await _client.SendAsync(createJobRequest);
        var createdJob = await createJobResponse.ReadRequiredJsonAsync<JobResponse>();

        var contractorLoginResponse = await _client.PostAsJsonAsync("/api/auth/login", new LoginRequest(
            "izvodjac@buildconnect.hr",
            "izvodjac123"));
        var contractorSession = await contractorLoginResponse.ReadRequiredJsonAsync<AuthenticatedSessionResponse>();

        using var createBidRequest = new HttpRequestMessage(HttpMethod.Post, $"/api/jobs/{createdJob.Id}/bids")
        {
            Content = JsonContent.Create(new CreateBidRequest(
                3400m,
                12,
                "Mozemo preuzeti posao vrlo brzo i kvalitetno ga zavrsiti."))
        };
        createBidRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", contractorSession.AccessToken);

        var createBidResponse = await _client.SendAsync(createBidRequest);
        var createdBid = await createBidResponse.ReadRequiredJsonAsync<BidResponse>();

        using var acceptBidRequest = new HttpRequestMessage(HttpMethod.Post, $"/api/bids/{createdBid.Id}/accept")
        {
            Content = JsonContent.Create(new { })
        };
        acceptBidRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", investorSession.AccessToken);

        var acceptBidResponse = await _client.SendAsync(acceptBidRequest);

        Assert.Equal(HttpStatusCode.Created, createJobResponse.StatusCode);
        Assert.Equal(HttpStatusCode.Created, createBidResponse.StatusCode);
        Assert.Equal(HttpStatusCode.OK, acceptBidResponse.StatusCode);

        var reviewRequest = new CreateReviewRequest(5, "Vrlo kvalitetno i profesionalno odradjen posao.");
        using var earlyReviewRequest = new HttpRequestMessage(HttpMethod.Post, $"/api/jobs/{createdJob.Id}/reviews")
        {
            Content = JsonContent.Create(reviewRequest)
        };
        earlyReviewRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", investorSession.AccessToken);

        using var earlyReviewResponse = await _client.SendAsync(earlyReviewRequest);
        Assert.Equal(HttpStatusCode.BadRequest, earlyReviewResponse.StatusCode);
        var earlyReviewError = await earlyReviewResponse.ReadRequiredJsonAsync<Dictionary<string, string>>();
        Assert.Equal("Recenziju mozete ostaviti tek nakon zavrsetka posla.", earlyReviewError["message"]);

        using var inProgressResponse = await _client.GetAsync($"/api/jobs/{createdJob.Id}");
        var inProgressJob = await inProgressResponse.ReadRequiredJsonAsync<JobResponse>();
        Assert.Equal(JobStatuses.InProgress, inProgressJob.Status);
        using var reviewsBeforeCompletionResponse = await _client.GetAsync($"/api/reviews?jobId={createdJob.Id}");
        Assert.Empty(await reviewsBeforeCompletionResponse.ReadRequiredJsonAsync<ReviewResponse[]>());

        using var request = new HttpRequestMessage(HttpMethod.Post, $"/api/jobs/{createdJob.Id}/complete")
        {
            Content = JsonContent.Create(new { })
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", contractorSession.AccessToken);

        var response = await _client.SendAsync(request);
        var completedJob = await response.ReadRequiredJsonAsync<JobResponse>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(JobStatuses.Completed, completedJob.Status);

        using var completedReviewRequest = new HttpRequestMessage(HttpMethod.Post, $"/api/jobs/{createdJob.Id}/reviews")
        {
            Content = JsonContent.Create(reviewRequest)
        };
        completedReviewRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", investorSession.AccessToken);

        using var completedReviewResponse = await _client.SendAsync(completedReviewRequest);
        Assert.Equal(HttpStatusCode.Created, completedReviewResponse.StatusCode);
        var review = await completedReviewResponse.ReadRequiredJsonAsync<ReviewResponse>();
        Assert.Equal(createdJob.Id, review.JobId);
        Assert.Equal(investorSession.User.Id, review.ReviewerId);
        Assert.Equal(contractorSession.User.Id, review.RevieweeId);
        Assert.Equal(reviewRequest.Rating, review.Rating);
        Assert.Equal(reviewRequest.Comment, review.Comment);

        using var duplicateReviewRequest = new HttpRequestMessage(HttpMethod.Post, $"/api/jobs/{createdJob.Id}/reviews")
        {
            Content = JsonContent.Create(reviewRequest)
        };
        duplicateReviewRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", investorSession.AccessToken);
        using var duplicateReviewResponse = await _client.SendAsync(duplicateReviewRequest);
        Assert.Equal(HttpStatusCode.BadRequest, duplicateReviewResponse.StatusCode);

        using var reviewsResponse = await _client.GetAsync($"/api/reviews?jobId={createdJob.Id}");
        var savedReview = Assert.Single(await reviewsResponse.ReadRequiredJsonAsync<ReviewResponse[]>());
        Assert.Equal(review, savedReview);
        using var jobAfterReviewResponse = await _client.GetAsync($"/api/jobs/{createdJob.Id}");
        Assert.Equal(completedJob, await jobAfterReviewResponse.ReadRequiredJsonAsync<JobResponse>());
    }
}
