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
/// Service for Razorpay Order creation and status tracking.
/// </summary>
public class RazorpayOrderService : IRazorpayOrderService
{
    private readonly HttpClient _httpClient;
    private readonly IRazorpaySettingsRepository _settingsRepo;
    private readonly ILogger<RazorpayOrderService> _logger;

    public RazorpayOrderService(
        HttpClient httpClient,
        IRazorpaySettingsRepository settingsRepo,
        ILogger<RazorpayOrderService> logger)
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

    public async Task<RazorpayOrderResponse> CreateOrderAsync(RazorpayCreateOrderRequest request, CancellationToken cancellationToken = default)
    {
        RazorpayOrderResponse response = new();
        try
        {
            (RazorpaySettings settings, AuthenticationHeaderValue auth) = await GetAuthContextAsync(cancellationToken);

            long amountPaise = (long)Math.Round(request.Amount * 100m);
            if (amountPaise <= 0)
            {
                response.IsSuccess = false;
                response.ErrorMessage = "Order amount must be greater than 0.";
                return response;
            }

            var payload = new Dictionary<string, object>
            {
                { "amount", amountPaise },
                { "currency", string.IsNullOrWhiteSpace(request.Currency) ? settings.Currency : request.Currency },
                { "receipt", string.IsNullOrWhiteSpace(request.Receipt) ? $"rcpt_{Guid.NewGuid():N}"[..20] : request.Receipt }
            };

            if (request.Notes != null && request.Notes.Count > 0)
            {
                payload["notes"] = request.Notes;
            }

            using HttpRequestMessage httpRequest = new(HttpMethod.Post, "https://api.razorpay.com/v1/orders")
            {
                Headers = { Authorization = auth },
                Content = new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json")
            };

            using HttpResponseMessage httpResponse = await _httpClient.SendAsync(httpRequest, cancellationToken);
            string content = await httpResponse.Content.ReadAsStringAsync(cancellationToken);

            if (!httpResponse.IsSuccessStatusCode)
            {
                _logger.LogError("Razorpay create order failed with status {StatusCode}: {Response}", httpResponse.StatusCode, content);
                response.IsSuccess = false;
                response.ErrorMessage = $"Razorpay Error ({httpResponse.StatusCode}): {content}";
                return response;
            }

            JObject json = JObject.Parse(content);
            response.OrderId = json["id"]?.ToString() ?? string.Empty;
            response.Amount = request.Amount;
            response.AmountPaise = json["amount"]?.Value<long>() ?? amountPaise;
            response.Currency = json["currency"]?.ToString() ?? settings.Currency;
            response.Receipt = json["receipt"]?.ToString() ?? string.Empty;
            response.Status = json["status"]?.ToString() ?? "created";
            response.KeyId = settings.KeyId;
            response.CompanyName = settings.CompanyName;
            response.ThemeColor = settings.ThemeColor;
            response.Notes = request.Notes;

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error creating Razorpay order: {Message}", ex.Message);
            response.IsSuccess = false;
            response.ErrorMessage = ex.Message;
            return response;
        }
    }

    public async Task<RazorpayOrderResponse?> GetOrderAsync(string orderId, CancellationToken cancellationToken = default)
    {
        try
        {
            (RazorpaySettings settings, AuthenticationHeaderValue auth) = await GetAuthContextAsync(cancellationToken);

            using HttpRequestMessage httpRequest = new(HttpMethod.Get, $"https://api.razorpay.com/v1/orders/{orderId}")
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
            return new RazorpayOrderResponse
            {
                OrderId = json["id"]?.ToString() ?? orderId,
                Amount = amountPaise / 100m,
                AmountPaise = amountPaise,
                Currency = json["currency"]?.ToString() ?? "INR",
                Receipt = json["receipt"]?.ToString() ?? string.Empty,
                Status = json["status"]?.ToString() ?? string.Empty,
                KeyId = settings.KeyId,
                CompanyName = settings.CompanyName,
                ThemeColor = settings.ThemeColor
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching Razorpay order {OrderId}: {Message}", orderId, ex.Message);
            return null;
        }
    }

    public async Task<List<RazorpayPaymentDetails>> GetOrderPaymentsAsync(string orderId, CancellationToken cancellationToken = default)
    {
        List<RazorpayPaymentDetails> payments = new();
        try
        {
            (_, AuthenticationHeaderValue auth) = await GetAuthContextAsync(cancellationToken);

            using HttpRequestMessage httpRequest = new(HttpMethod.Get, $"https://api.razorpay.com/v1/orders/{orderId}/payments")
            {
                Headers = { Authorization = auth }
            };

            using HttpResponseMessage httpResponse = await _httpClient.SendAsync(httpRequest, cancellationToken);
            if (!httpResponse.IsSuccessStatusCode)
            {
                return payments;
            }

            string content = await httpResponse.Content.ReadAsStringAsync(cancellationToken);
            JObject json = JObject.Parse(content);
            JArray? items = json["items"] as JArray;

            if (items != null)
            {
                foreach (JObject item in items)
                {
                    long amtPaise = item["amount"]?.Value<long>() ?? 0;
                    payments.Add(new RazorpayPaymentDetails
                    {
                        Id = item["id"]?.ToString() ?? string.Empty,
                        OrderId = item["order_id"]?.ToString(),
                        Amount = amtPaise / 100m,
                        Currency = item["currency"]?.ToString() ?? "INR",
                        Status = item["status"]?.ToString() ?? string.Empty,
                        Method = item["method"]?.ToString() ?? string.Empty,
                        Vpa = item["vpa"]?.ToString(),
                        Bank = item["bank"]?.ToString(),
                        Wallet = item["wallet"]?.ToString(),
                        Email = item["email"]?.ToString(),
                        Contact = item["contact"]?.ToString()
                    });
                }
            }

            return payments;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching payments for order {OrderId}: {Message}", orderId, ex.Message);
            return payments;
        }
    }
}
