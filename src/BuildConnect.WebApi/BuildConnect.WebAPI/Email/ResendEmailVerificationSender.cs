using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using BuildConnect.Service.Common;

namespace BuildConnect.WebAPI.Email;

public sealed class ResendEmailVerificationSender : IEmailVerificationSender
{
    private readonly IConfiguration _configuration;
    private readonly HttpClient _httpClient;
    private readonly ILogger<ResendEmailVerificationSender> _logger;

    public ResendEmailVerificationSender(
        IConfiguration configuration,
        HttpClient httpClient,
        ILogger<ResendEmailVerificationSender> logger)
    {
        _configuration = configuration;
        _httpClient = httpClient;
        _logger = logger;
    }

    public bool SendVerificationEmail(string email, string displayName, string token)
    {
        var apiKey = _configuration["Resend:ApiKey"];
        var from = _configuration["Resend:From"];
        var clientBaseUrl = _configuration["Email:ClientBaseUrl"] ?? "http://localhost:3000";

        if (string.IsNullOrWhiteSpace(apiKey) || string.IsNullOrWhiteSpace(from))
        {
            _logger.LogWarning("Email verification mail was not sent because Resend:ApiKey or Resend:From is not configured.");
            return false;
        }

        var verificationUrl = $"{clientBaseUrl.TrimEnd('/')}/verify-email?email={WebUtility.UrlEncode(email)}&token={WebUtility.UrlEncode(token)}";
        var request = new ResendEmailRequest(
            from,
            [email],
            "Potvrdite BuildConnect račun",
            CreateHtmlBody(displayName, verificationUrl),
            CreateTextBody(displayName, verificationUrl));

        try
        {
            using var httpRequest = new HttpRequestMessage(HttpMethod.Post, "emails")
            {
                Content = JsonContent.Create(request),
            };
            httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

            using var response = _httpClient.Send(httpRequest);
            if (response.IsSuccessStatusCode)
            {
                return true;
            }

            var responseBody = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            _logger.LogError(
                "Resend email verification request failed with status {StatusCode}: {ResponseBody}",
                response.StatusCode,
                responseBody);

            return false;
        }
        catch (HttpRequestException exception)
        {
            _logger.LogError(exception, "Resend email verification request failed for {Email}.", email);
            return false;
        }
        catch (OperationCanceledException exception)
        {
            _logger.LogError(exception, "Resend email verification request timed out or was cancelled for {Email}.", email);
            return false;
        }
    }

    private static string CreateHtmlBody(string displayName, string verificationUrl)
    {
        return $"""
        <p>Pozdrav {WebUtility.HtmlEncode(displayName)},</p>
        <p>Hvala na registraciji na BuildConnect.</p>
        <p>
          <a href="{WebUtility.HtmlEncode(verificationUrl)}">Potvrdite svoju email adresu</a>
        </p>
        <p>Link vrijedi 24 sata.</p>
        """;
    }

    private static string CreateTextBody(string displayName, string verificationUrl)
    {
        return $"""
        Pozdrav {displayName},

        hvala na registraciji na BuildConnect.

        Potvrdite svoju email adresu klikom na link:
        {verificationUrl}

        Link vrijedi 24 sata.
        """;
    }

    private sealed record ResendEmailRequest(
        [property: JsonPropertyName("from")] string From,
        [property: JsonPropertyName("to")] IReadOnlyCollection<string> To,
        [property: JsonPropertyName("subject")] string Subject,
        [property: JsonPropertyName("html")] string Html,
        [property: JsonPropertyName("text")] string Text);
}
