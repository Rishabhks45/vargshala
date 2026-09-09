using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Vargshala.Application.Abstractions.Email;
using Vargshala.Infrastructure.Settings;

namespace Vargshala.Infrastructure.Services;

public class SendGridEmailService : IEmailService
{
    private readonly HttpClient _httpClient;
    private readonly SendGridOptions _options;
    private readonly ILogger<SendGridEmailService> _logger;

    public SendGridEmailService(
        HttpClient httpClient,
        IOptions<SendGridOptions> options,
        ILogger<SendGridEmailService> logger)
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
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            _logger.LogWarning("SendGrid API Key is not configured in appsettings.json. Email to [{Recipients}] with subject '{Subject}' was not dispatched.",
                string.Join(", ", request.To), request.Subject);
            return false;
        }

        try
        {
            var fromEmail = !string.IsNullOrWhiteSpace(request.From) ? request.From : _options.DefaultFromEmail;
            var fromName = _options.DefaultFromName;

            var toList = request.To.Select(email => new SendGridEmailAddress { Email = email }).ToList();

            var contentList = new List<SendGridContent>();
            if (!string.IsNullOrWhiteSpace(request.TextBody))
            {
                contentList.Add(new SendGridContent { Type = "text/plain", Value = request.TextBody });
            }
            if (!string.IsNullOrWhiteSpace(request.HtmlBody))
            {
                contentList.Add(new SendGridContent { Type = "text/html", Value = request.HtmlBody });
            }
            if (contentList.Count == 0)
            {
                contentList.Add(new SendGridContent { Type = "text/html", Value = "<p></p>" });
            }

            var payload = new SendGridPayload
            {
                Personalizations = [new SendGridPersonalization { To = toList }],
                From = new SendGridEmailAddress { Email = fromEmail, Name = fromName },
                Subject = request.Subject,
                Content = contentList
            };

            if (!string.IsNullOrWhiteSpace(request.ReplyTo))
            {
                payload.ReplyTo = new SendGridEmailAddress { Email = request.ReplyTo };
            }

            using var httpRequest = new HttpRequestMessage(HttpMethod.Post, "v3/mail/send");
            httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
            httpRequest.Content = JsonContent.Create(payload, options: new JsonSerializerOptions
            {
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            });

            var response = await _httpClient.SendAsync(httpRequest, cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Email successfully dispatched via SendGrid to [{Recipients}]. Status: {StatusCode}",
                    string.Join(", ", request.To), response.StatusCode);
                return true;
            }

            var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogError("SendGrid API returned error {StatusCode} for recipients [{Recipients}]. Error: {Error}",
                response.StatusCode, string.Join(", ", request.To), errorContent);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception occurred while sending email via SendGrid to [{Recipients}]: {Message}",
                string.Join(", ", request.To), ex.Message);
            return false;
        }
    }

    private sealed class SendGridPayload
    {
        [JsonPropertyName("personalizations")]
        public List<SendGridPersonalization> Personalizations { get; set; } = new();

        [JsonPropertyName("from")]
        public SendGridEmailAddress From { get; set; } = new();

        [JsonPropertyName("reply_to")]
        public SendGridEmailAddress? ReplyTo { get; set; }

        [JsonPropertyName("subject")]
        public string Subject { get; set; } = string.Empty;

        [JsonPropertyName("content")]
        public List<SendGridContent> Content { get; set; } = new();
    }

    private sealed class SendGridPersonalization
    {
        [JsonPropertyName("to")]
        public List<SendGridEmailAddress> To { get; set; } = new();
    }

    private sealed class SendGridEmailAddress
    {
        [JsonPropertyName("email")]
        public string Email { get; set; } = string.Empty;

        [JsonPropertyName("name")]
        public string? Name { get; set; }
    }

    private sealed class SendGridContent
    {
        [JsonPropertyName("type")]
        public string Type { get; set; } = string.Empty;

        [JsonPropertyName("value")]
        public string Value { get; set; } = string.Empty;
    }
}
