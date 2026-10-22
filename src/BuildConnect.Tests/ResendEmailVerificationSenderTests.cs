using System.Net;
using BuildConnect.WebAPI.Email;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;

namespace BuildConnect.Tests;

public sealed class ResendEmailVerificationSenderTests
{
    [Fact]
    public void SendVerificationEmail_ReturnsTrueAndDisposesResponse_WhenRequestSucceeds()
    {
        var content = new TrackingStringContent("{\"id\":\"test-email\"}");
        using var handler = new StubHttpMessageHandler(() => new HttpResponseMessage(HttpStatusCode.OK) { Content = content });
        using var httpClient = CreateHttpClient(handler);
        var sender = CreateSender(httpClient);

        var result = sender.SendVerificationEmail("user@test.hr", "Test User", "verification-token");

        Assert.True(result);
        Assert.Equal(1, handler.SendCount);
        Assert.True(content.IsDisposed);
    }

    [Fact]
    public void SendVerificationEmail_ReturnsFalseAndDisposesResponse_WhenRequestIsRejected()
    {
        var content = new TrackingStringContent("{\"message\":\"Invalid API key\"}");
        using var handler = new StubHttpMessageHandler(() => new HttpResponseMessage(HttpStatusCode.Unauthorized) { Content = content });
        using var httpClient = CreateHttpClient(handler);
        var sender = CreateSender(httpClient);

        var result = sender.SendVerificationEmail("user@test.hr", "Test User", "verification-token");

        Assert.False(result);
        Assert.Equal(1, handler.SendCount);
        Assert.True(content.IsDisposed);
    }

    [Fact]
    public void SendVerificationEmail_ReturnsFalse_WhenRequestTimesOut()
    {
        using var handler = new StubHttpMessageHandler(() => throw new TaskCanceledException("Simulated delivery timeout."));
        using var httpClient = CreateHttpClient(handler);
        var sender = CreateSender(httpClient);

        var result = sender.SendVerificationEmail("user@test.hr", "Test User", "verification-token");

        Assert.False(result);
        Assert.Equal(1, handler.SendCount);
    }

    [Fact]
    public void SendVerificationEmail_ReturnsFalse_WhenConnectionFails()
    {
        using var handler = new StubHttpMessageHandler(() => throw new HttpRequestException("Simulated connection failure."));
        using var httpClient = CreateHttpClient(handler);
        var sender = CreateSender(httpClient);

        var result = sender.SendVerificationEmail("user@test.hr", "Test User", "verification-token");

        Assert.False(result);
        Assert.Equal(1, handler.SendCount);
    }

    private static HttpClient CreateHttpClient(HttpMessageHandler handler)
    {
        return new HttpClient(handler) { BaseAddress = new Uri("https://api.resend.com/") };
    }

    private static ResendEmailVerificationSender CreateSender(HttpClient httpClient)
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Resend:ApiKey"] = "test-api-key",
                ["Resend:From"] = "BuildConnect <no-reply@test.hr>",
                ["Email:ClientBaseUrl"] = "https://buildconnect.test",
            })
            .Build();

        return new ResendEmailVerificationSender(configuration, httpClient, NullLogger<ResendEmailVerificationSender>.Instance);
    }

    private sealed class StubHttpMessageHandler(Func<HttpResponseMessage> send) : HttpMessageHandler
    {
        public int SendCount { get; private set; }

        protected override HttpResponseMessage Send(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            SendCount++;
            return send();
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            throw new NotSupportedException("These tests exercise the synchronous email sender.");
        }
    }

    private sealed class TrackingStringContent(string content) : StringContent(content)
    {
        public bool IsDisposed { get; private set; }

        protected override void Dispose(bool disposing)
        {
            IsDisposed = true;
            base.Dispose(disposing);
        }
    }
}
