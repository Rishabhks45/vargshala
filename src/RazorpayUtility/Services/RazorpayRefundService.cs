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
/// Service for initiating and checking Razorpay refunds.
/// </summary>
public class RazorpayRefundService : IRazorpayRefundService
{
    private readonly HttpClient _httpClient;
    private readonly IRazorpaySettingsRepository _settingsRepo;
    private readonly ILogger<RazorpayRefundService> _logger;

    public RazorpayRefundService(
        HttpClient httpClient,
        IRazorpaySettingsRepository settingsRepo,
        ILogger<RazorpayRefundService> logger)
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

    public async Task<RazorpayRefundResponse> CreateRefundAsync(RazorpayRefundRequest request, CancellationToken cancellationToken = default)
    {
        RazorpayRefundResponse response = new();
        try
        {
            (_, AuthenticationHeaderValue auth) = await GetAuthContextAsync(cancellationToken);

            var payload = new Dictionary<string, object>();
            if (request.Amount.HasValue && request.Amount.Value > 0)
            {
                payload["amount"] = (long)Math.Round(request.Amount.Value * 100m);
            }

            if (request.Notes != null && request.Notes.Count > 0)
            {
                payload["notes"] = request.Notes;
            }

            using HttpRequestMessage httpRequest = new(HttpMethod.Post, $"https://api.razorpay.com/v1/payments/{request.PaymentId}/refund")
            {
                Headers = { Authorization = auth },
                Content = new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json")
            };

            using HttpResponseMessage httpResponse = await _httpClient.SendAsync(httpRequest, cancellationToken);
            string content = await httpResponse.Content.ReadAsStringAsync(cancellationToken);

            if (!httpResponse.IsSuccessStatusCode)
            {
                _logger.LogError("Razorpay refund failed ({StatusCode}): {Response}", httpResponse.StatusCode, content);
                response.IsSuccess = false;
                response.ErrorMessage = $"Razorpay Refund Error ({httpResponse.StatusCode}): {content}";
                return response;
            }

            JObject json = JObject.Parse(content);
            long amountPaise = json["amount"]?.Value<long>() ?? 0;

            response.RefundId = json["id"]?.ToString() ?? string.Empty;
            response.PaymentId = json["payment_id"]?.ToString() ?? request.PaymentId;
            response.Amount = amountPaise / 100m;
            response.Currency = json["currency"]?.ToString() ?? "INR";
            response.Status = json["status"]?.ToString() ?? "processed";

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error creating Razorpay refund: {Message}", ex.Message);
            response.IsSuccess = false;
            response.ErrorMessage = ex.Message;
            return response;
        }
    }

    public async Task<RazorpayRefundResponse?> GetRefundAsync(string refundId, CancellationToken cancellationToken = default)
    {
        try
        {
            (_, AuthenticationHeaderValue auth) = await GetAuthContextAsync(cancellationToken);

            using HttpRequestMessage httpRequest = new(HttpMethod.Get, $"https://api.razorpay.com/v1/refunds/{refundId}")
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
            long amountPaise = json["amount"]?.Value<long>() ?? 0;

            return new RazorpayRefundResponse
            {
                RefundId = json["id"]?.ToString() ?? refundId,
                PaymentId = json["payment_id"]?.ToString() ?? string.Empty,
                Amount = amountPaise / 100m,
                Currency = json["currency"]?.ToString() ?? "INR",
                Status = json["status"]?.ToString() ?? string.Empty
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching Razorpay refund {RefundId}: {Message}", refundId, ex.Message);
            return null;
        }
    }
}