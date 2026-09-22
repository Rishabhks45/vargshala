using System.Net.Http.Headers;
using System.Text;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RazorpayUtility.Configuration;
using RazorpayUtility.Interfaces;
using RazorpayUtility.Models;

namespace RazorpayUtility.Services;

/// <summary>
/// Service for generating and managing Razorpay Dynamic UPI QR Codes.
/// </summary>
public class RazorpayQrCodeService : IRazorpayQrCodeService
{
    private readonly HttpClient _httpClient;
    private readonly IRazorpaySettingsRepository _settingsRepo;
    private readonly ILogger<RazorpayQrCodeService> _logger;

    public RazorpayQrCodeService(
        HttpClient httpClient,
        IRazorpaySettingsRepository settingsRepo,
        ILogger<RazorpayQrCodeService> logger)
    {
        _httpClient = httpClient;
        _settingsRepo = settingsRepo;
        _logger = logger;
    }

    private async Task<(RazorpaySettings settings, AuthenticationHeaderValue auth)> GetAuthContextAsync(CancellationToken cancellationToken)
    {
        RazorpaySettings? settings = await _settingsRepo.GetSettingsAsync(cancellationToken);
        if (settings == null || !settings.IsValid())
        {
            throw new InvalidOperationException("Razorpay settings are missing or incomplete in the database.");
        }

        string rawAuth = $"{settings.KeyId}:{settings.KeySecret}";
        string base64Auth = Convert.ToBase64String(Encoding.ASCII.GetBytes(rawAuth));
        return (settings, new AuthenticationHeaderValue("Basic", base64Auth));
    }

    public async Task<RazorpayQrCodeResponse> CreateUpiQrCodeAsync(RazorpayCreateQrCodeRequest request, CancellationToken cancellationToken = default)
    {
        RazorpayQrCodeResponse response = new();
        try
        {
            (RazorpaySettings settings, AuthenticationHeaderValue auth) = await GetAuthContextAsync(cancellationToken);

            long amountPaise = (long)Math.Round(request.Amount * 100m);
            if (amountPaise <= 0)
            {
                response.IsSuccess = false;
                response.ErrorMessage = "QR code payment amount must be greater than 0.";
                return response;
            }

            var payload = new Dictionary<string, object>
            {
                { "type", "upi_qr" },
                { "name", string.IsNullOrWhiteSpace(request.Name) ? settings.CompanyName : request.Name },
                { "usage", "single_use" },
                { "fixed_amount", true },
                { "payment_amount", amountPaise },
                { "description", request.Description ?? "Fee Payment" }
            };

            if (request.Notes != null && request.Notes.Count > 0)
            {
                payload["notes"] = request.Notes;
            }

            // Expiry / CloseBy handling (default to 30 mins if specified)
            if (request.CloseBy.HasValue)
            {
                long epoch = new DateTimeOffset(request.CloseBy.Value.ToUniversalTime()).ToUnixTimeSeconds();
                payload["close_by"] = epoch;
            }

            using HttpRequestMessage httpRequest = new(HttpMethod.Post, "https://api.razorpay.com/v1/payments/qr_codes")
            {
                Headers = { Authorization = auth },
                Content = new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json")
            };

            using HttpResponseMessage httpResponse = await _httpClient.SendAsync(httpRequest, cancellationToken);
            string content = await httpResponse.Content.ReadAsStringAsync(cancellationToken);

            if (!httpResponse.IsSuccessStatusCode)
            {
                _logger.LogError("Razorpay QR creation failed ({StatusCode}): {Response}", httpResponse.StatusCode, content);
                response.IsSuccess = false;
                response.ErrorMessage = $"Razorpay QR Error ({httpResponse.StatusCode}): {content}";
                return response;
            }

            JObject json = JObject.Parse(content);
            response.QrCodeId = json["id"]?.ToString() ?? string.Empty;
            response.ImageUrl = json["image_url"]?.ToString() ?? string.Empty;
            response.Status = json["status"]?.ToString() ?? "active";
            response.Amount = request.Amount;
            response.Notes = request.Notes;
            response.CloseBy = request.CloseBy;

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error generating Razorpay QR Code: {Message}", ex.Message);
            response.IsSuccess = false;
            response.ErrorMessage = ex.Message;
            return response;
        }
    }

    public async Task<RazorpayQrCodeResponse?> GetQrCodeAsync(string qrCodeId, CancellationToken cancellationToken = default)
    {
        try
        {
            (_, AuthenticationHeaderValue auth) = await GetAuthContextAsync(cancellationToken);

            using HttpRequestMessage httpRequest = new(HttpMethod.Get, $"https://api.razorpay.com/v1/payments/qr_codes/{qrCodeId}")
            {
                Headers = { Authorization = auth }
            };

            using HttpResponseMessage httpResponse = await _httpClient.SendAsync(httpRequest, cancellationToken);
            if (!httpResponse.IsSuccessStatusCode)
            {
                return null;
            }

            string content = await httpResponse.Content.ReadAsStringAsync(cancellationToken);
            JObject json = JObject.Parse(content);

            long amountPaise = json["payment_amount"]?.Value<long>() ?? 0;
            long receivedPaise = json["payments_amount_received"]?.Value<long>() ?? 0;

            return new RazorpayQrCodeResponse
            {
                QrCodeId = json["id"]?.ToString() ?? qrCodeId,
                ImageUrl = json["image_url"]?.ToString() ?? string.Empty,
                Status = json["status"]?.ToString() ?? string.Empty,
                Amount = amountPaise / 100m,
                PaymentsAmountReceived = receivedPaise / 100m
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching Razorpay QR code {QrId}: {Message}", qrCodeId, ex.Message);
            return null;
        }
    }

    public async Task<bool> CloseQrCodeAsync(string qrCodeId, CancellationToken cancellationToken = default)
    {
        try
        {
            (_, AuthenticationHeaderValue auth) = await GetAuthContextAsync(cancellationToken);

            using HttpRequestMessage httpRequest = new(HttpMethod.Post, $"https://api.razorpay.com/v1/payments/qr_codes/{qrCodeId}/close")
            {
                Headers = { Authorization = auth }
            };

            using HttpResponseMessage httpResponse = await _httpClient.SendAsync(httpRequest, cancellationToken);
            return httpResponse.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error closing Razorpay QR code {QrId}: {Message}", qrCodeId, ex.Message);
            return false;
        }
    }
}
