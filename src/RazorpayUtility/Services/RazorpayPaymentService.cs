using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RazorpayUtility.Configuration;
using RazorpayUtility.Interfaces;
using RazorpayUtility.Models;

namespace RazorpayUtility.Services;

/// <summary>
/// Service for verifying Razorpay payment signatures and querying payment details.
/// </summary>
public class RazorpayPaymentService : IRazorpayPaymentService
{
    private readonly HttpClient _httpClient;
    private readonly IRazorpaySettingsRepository _settingsRepo;
    private readonly ILogger<RazorpayPaymentService> _logger;

    public RazorpayPaymentService(
        HttpClient httpClient,
        IRazorpaySettingsRepository settingsRepo,
        ILogger<RazorpayPaymentService> logger)
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

    public async Task<RazorpayPaymentVerifyResult> VerifyPaymentSignatureAsync(RazorpayPaymentVerifyRequest request, CancellationToken cancellationToken = default)
    {
        RazorpayPaymentVerifyResult result = new()
        {
            OrderId = request.RazorpayOrderId,
            PaymentId = request.RazorpayPaymentId
        };

        try
        {
            (RazorpaySettings settings, AuthenticationHeaderValue auth) = await GetAuthContextAsync(cancellationToken);

            string message = $"{request.RazorpayOrderId}|{request.RazorpayPaymentId}";
            byte[] keyBytes = Encoding.UTF8.GetBytes(settings.KeySecret);
            byte[] messageBytes = Encoding.UTF8.GetBytes(message);

            using var hmac = new HMACSHA256(keyBytes);
            byte[] hash = hmac.ComputeHash(messageBytes);
            string generatedSignature = Convert.ToHexString(hash).ToLowerInvariant();

            result.IsValidSignature = string.Equals(generatedSignature, request.RazorpaySignature, StringComparison.OrdinalIgnoreCase);

            if (!result.IsValidSignature)
            {
                result.IsSuccess = false;
                result.ErrorMessage = "Invalid payment signature. Potential tampering detected.";
                _logger.LogWarning("Razorpay signature mismatch for Order: {OrderId}, Payment: {PaymentId}", request.RazorpayOrderId, request.RazorpayPaymentId);
                return result;
            }

            // Signature verified successfully; now fetch live payment details from Razorpay
            RazorpayPaymentDetails? paymentDetails = await GetPaymentAsync(request.RazorpayPaymentId, cancellationToken);
            if (paymentDetails != null)
            {
                result.Amount = paymentDetails.Amount;
                result.Status = paymentDetails.Status;
                result.Method = paymentDetails.Method;
                result.Vpa = paymentDetails.Vpa;
                result.Bank = paymentDetails.Bank;
                result.Wallet = paymentDetails.Wallet;
                result.Email = paymentDetails.Email;
                result.Contact = paymentDetails.Contact;
                result.Notes = paymentDetails.Notes;
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verifying Razorpay payment signature: {Message}", ex.Message);
            result.IsSuccess = false;
            result.ErrorMessage = ex.Message;
            return result;
        }
    }

    public async Task<RazorpayPaymentDetails?> GetPaymentAsync(string paymentId, CancellationToken cancellationToken = default)
    {
        try
        {
            (_, AuthenticationHeaderValue auth) = await GetAuthContextAsync(cancellationToken);

            using HttpRequestMessage httpRequest = new(HttpMethod.Get, $"https://api.razorpay.com/v1/payments/{paymentId}")
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
            return new RazorpayPaymentDetails
            {
                Id = json["id"]?.ToString() ?? paymentId,
                OrderId = json["order_id"]?.ToString(),
                Amount = amountPaise / 100m,
                Currency = json["currency"]?.ToString() ?? "INR",
                Status = json["status"]?.ToString() ?? string.Empty,
                Method = json["method"]?.ToString() ?? string.Empty,
                Description = json["description"]?.ToString(),
                Vpa = json["vpa"]?.ToString(),
                Bank = json["bank"]?.ToString(),
                Wallet = json["wallet"]?.ToString(),
                Email = json["email"]?.ToString(),
                Contact = json["contact"]?.ToString(),
                ErrorCode = json["error_code"]?.ToString(),
                ErrorDescription = json["error_description"]?.ToString()
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching Razorpay payment {PaymentId}: {Message}", paymentId, ex.Message);
            return null;
        }
    }

    public async Task<bool> CapturePaymentAsync(string paymentId, decimal amount, string currency = "INR", CancellationToken cancellationToken = default)
    {
        try
        {
            (_, AuthenticationHeaderValue auth) = await GetAuthContextAsync(cancellationToken);

            long amountPaise = (long)Math.Round(amount * 100m);
            var payload = new Dictionary<string, object>
            {
                { "amount", amountPaise },
                { "currency", currency }
            };

            using HttpRequestMessage httpRequest = new(HttpMethod.Post, $"https://api.razorpay.com/v1/payments/{paymentId}/capture")
            {
                Headers = { Authorization = auth },
                Content = new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json")
            };

            using HttpResponseMessage httpResponse = await _httpClient.SendAsync(httpRequest, cancellationToken);
            return httpResponse.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error capturing Razorpay payment {PaymentId}: {Message}", paymentId, ex.Message);
            return false;
        }
    }
}
