using System.Net;
using System.Net.Http.Json;
using BuildConnect.DAL;
using BuildConnect.Model;
using BuildConnect.Service.Common;
using BuildConnect.Tests.Support;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace BuildConnect.Tests;

public sealed class EmailVerificationIntegrationTests : IClassFixture<IntegrationTestWebApplicationFactory>
{
    private readonly IntegrationTestWebApplicationFactory _factory;
    private readonly HttpClient _client;
    private readonly FakeEmailVerificationSender _emailVerificationSender;

    public EmailVerificationIntegrationTests(IntegrationTestWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
        _emailVerificationSender = (FakeEmailVerificationSender)factory.Services.GetRequiredService<IEmailVerificationSender>();
    }

    [Fact]
    public async Task Register_RequiresValidEmailVerification_BeforeLogin()
    {
        var email = $"email-verification-{Guid.NewGuid():N}@test.hr";
        const string password = "secret123";
        using var registrationResponse = await _client.PostAsJsonAsync("/api/auth/register", new RegisterRequest(
            "Ivan", "Ivic", email, password, "0912345678", BuildConnectRoles.Izvodjac, BuildConnectLegalTypes.Firma));

        Assert.Equal(HttpStatusCode.Created, registrationResponse.StatusCode);
        var registration = await registrationResponse.ReadRequiredJsonAsync<RegistrationResponse>();
        Assert.Equal(email, registration.Email);
        Assert.True(registration.IsVerificationEmailSent);
        var verificationEmail = Assert.Single(_emailVerificationSender.SentEmails, sentEmail => sentEmail.Email == email);
        Assert.False(string.IsNullOrWhiteSpace(verificationEmail.Token));

        using var unverifiedLoginResponse = await _client.PostAsJsonAsync("/api/auth/login", new LoginRequest(email, password));
        Assert.Equal(HttpStatusCode.Unauthorized, unverifiedLoginResponse.StatusCode);

        using var invalidVerificationResponse = await _client.PostAsJsonAsync("/api/auth/verify-email", new VerifyEmailRequest(email, "wrong-token"));
        Assert.Equal(HttpStatusCode.BadRequest, invalidVerificationResponse.StatusCode);

        using var rejectedLoginResponse = await _client.PostAsJsonAsync("/api/auth/login", new LoginRequest(email, password));
        Assert.Equal(HttpStatusCode.Unauthorized, rejectedLoginResponse.StatusCode);

        using var validVerificationResponse = await _client.PostAsJsonAsync("/api/auth/verify-email", new VerifyEmailRequest(email, verificationEmail.Token));
        Assert.Equal(HttpStatusCode.NoContent, validVerificationResponse.StatusCode);

        using var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", new LoginRequest(email, password));
        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);
        var session = await loginResponse.ReadRequiredJsonAsync<AuthenticatedSessionResponse>();
        Assert.Equal(email, session.User.Email);
        Assert.False(string.IsNullOrWhiteSpace(session.AccessToken));

        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<BuildConnectDbContext>();
        var user = await dbContext.Users.SingleAsync(user => user.Email == email);
        Assert.True(user.IsEmailVerified);
        Assert.Null(user.EmailVerificationToken);
        Assert.Null(user.EmailVerificationTokenExpiresAt);
    }

    [Fact]
    public async Task ResendVerificationEmail_ReplacesExpiredToken_AndAllowsVerification()
    {
        var email = $"email-expiry-{Guid.NewGuid():N}@test.hr";
        const string password = "secret123";
        using var registrationResponse = await _client.PostAsJsonAsync("/api/auth/register", new RegisterRequest(
            "Ana", "Anic", email, password, "0912345678", BuildConnectRoles.Investitor, null));

        Assert.Equal(HttpStatusCode.Created, registrationResponse.StatusCode);
        var originalEmail = Assert.Single(_emailVerificationSender.SentEmails, sentEmail => sentEmail.Email == email);

        using (var scope = _factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<BuildConnectDbContext>();
            var user = await dbContext.Users.SingleAsync(user => user.Email == email);
            user.EmailVerificationTokenExpiresAt = DateTimeOffset.UtcNow.AddHours(-1);
            await dbContext.SaveChangesAsync();
        }

        using var expiredVerificationResponse = await _client.PostAsJsonAsync("/api/auth/verify-email", new VerifyEmailRequest(email, originalEmail.Token));
        Assert.Equal(HttpStatusCode.BadRequest, expiredVerificationResponse.StatusCode);

        using var resendResponse = await _client.PostAsJsonAsync("/api/auth/resend-verification-email", new ResendVerificationEmailRequest(email));
        Assert.Equal(HttpStatusCode.OK, resendResponse.StatusCode);
        var registration = await resendResponse.ReadRequiredJsonAsync<RegistrationResponse>();
        Assert.Equal(email, registration.Email);
        Assert.True(registration.IsVerificationEmailSent);
        var sentEmails = _emailVerificationSender.SentEmails.Where(sentEmail => sentEmail.Email == email).ToArray();
        Assert.Equal(2, sentEmails.Length);
        var replacementEmail = sentEmails[1];
        Assert.NotEqual(originalEmail.Token, replacementEmail.Token);

        using var oldTokenResponse = await _client.PostAsJsonAsync("/api/auth/verify-email", new VerifyEmailRequest(email, originalEmail.Token));
        Assert.Equal(HttpStatusCode.BadRequest, oldTokenResponse.StatusCode);

        using var unverifiedLoginResponse = await _client.PostAsJsonAsync("/api/auth/login", new LoginRequest(email, password));
        Assert.Equal(HttpStatusCode.Unauthorized, unverifiedLoginResponse.StatusCode);

        using var newTokenResponse = await _client.PostAsJsonAsync("/api/auth/verify-email", new VerifyEmailRequest(email, replacementEmail.Token));
        Assert.Equal(HttpStatusCode.NoContent, newTokenResponse.StatusCode);

        using var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", new LoginRequest(email, password));
        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);
        var session = await loginResponse.ReadRequiredJsonAsync<AuthenticatedSessionResponse>();
        Assert.Equal(email, session.User.Email);
        Assert.False(string.IsNullOrWhiteSpace(session.AccessToken));
    }
}
