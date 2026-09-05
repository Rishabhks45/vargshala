using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Vargshala.Application.Abstractions.Email;
using Vargshala.Infrastructure.Settings;

namespace Vargshala.Infrastructure.Services;

public class ResendEmailService : IEmailService
{
    private readonly HttpClient _httpClient;
    private readonly ResendOptions _options;
    private readonly ILogger<ResendEmailService> _logger;

    public ResendEmailService(
        HttpClient httpClient,
        IOptions<ResendOptions> options,
        ILogger<ResendEmailService> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;
    }

    public Task<bool> SendEmailAsync(
        string to,
        string subject,
        string htmlBody,
        string? from = null,
        CancellationToken cancellationToken = default)
    {
        return SendEmailAsync(new EmailMessageRequest
        {
            To = [to],
            Subject = subject,
            HtmlBody = htmlBody,
            From = from
        }, cancellationToken);
    }

    public async Task<bool> SendEmailAsync(
        EmailMessageRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request.To == null || request.To.Count == 0)
        {
            _logger.LogWarning("Cannot send email: No recipients specified in 'To' list.");
            return false;
        }

        var apiKey = _options.ApiKey?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(apiKey) || apiKey.Contains("YOUR_API_KEY", StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogWarning("Resend API Key is not configured in appsettings.json. Email to [{Recipients}] with subject '{Subject}' was not dispatched.",
                string.Join(", ", request.To), request.Subject);
            return false;
        }

        try
        {
            var fromAddress = !string.IsNullOrWhiteSpace(request.From)
                ? request.From
                : $"{_options.DefaultFromName} <{_options.DefaultFromEmail}>";

            var payload = new ResendEmailPayload
            {
                From = fromAddress,
                To = request.To,
                Subject = request.Subject,
                Html = request.HtmlBody,
                Text = request.TextBody,
                Cc = request.Cc is { Count: > 0 } ? request.Cc : null,
                Bcc = request.Bcc is { Count: > 0 } ? request.Bcc : null,
                ReplyTo = !string.IsNullOrWhiteSpace(request.ReplyTo) ? request.ReplyTo : null
            };

            using var httpRequest = new HttpRequestMessage(HttpMethod.Post, "emails");
            httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
            httpRequest.Content = JsonContent.Create(payload, options: new JsonSerializerOptions
            {
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            });

            var response = await _httpClient.SendAsync(httpRequest, cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogInformation("Email successfully dispatched via Resend to [{Recipients}]. Response: {Response}",
                    string.Join(", ", request.To), responseContent);
                return true;
            }

            var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogError("Resend API returned error {StatusCode} for recipients [{Recipients}]. Error: {Error}",
                response.StatusCode, string.Join(", ", request.To), errorContent);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception occurred while sending email via Resend to [{Recipients}]: {Message}",
                string.Join(", ", request.To), ex.Message);
            return false;
        }
    }

    private sealed class ResendEmailPayload
    {
        [JsonPropertyName("from")]
        public string From { get; set; } = string.Empty;

        [JsonPropertyName("to")]
        public List<string> To { get; set; } = new();

        [JsonPropertyName("subject")]
        public string Subject { get; set; } = string.Empty;

        [JsonPropertyName("html")]
        public string Html { get; set; } = string.Empty;

        [JsonPropertyName("text")]
        public string? Text { get; set; }

        [JsonPropertyName("cc")]
        public List<string>? Cc { get; set; }

        [JsonPropertyName("bcc")]
        public List<string>? Bcc { get; set; }

        [JsonPropertyName("reply_to")]
        public string? ReplyTo { get; set; }
    }
}
