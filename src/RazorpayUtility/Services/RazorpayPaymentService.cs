using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Logging;
using Razorpay.Api;
using RazorpayUtility.Interfaces;
using RazorpayUtility.Models;

namespace RazorpayUtility.Services;

/// <summary>
/// Service for verifying Razorpay payment signatures and querying payment details using the official Razorpay SDK.
/// </summary>
public class RazorpayPaymentService : IRazorpayPaymentService
{
    private readonly IRazorpayClientProvider _clientProvider;
    private readonly ILogger<RazorpayPaymentService> _logger;

    public RazorpayPaymentService(
        IRazorpayClientProvider clientProvider,
        ILogger<RazorpayPaymentService> logger)
    {
        _clientProvider = clientProvider;
        _logger = logger;
    }

    public async Task<RazorpayPaymentVerifyResult> VerifyPaymentSignatureAsync(RazorpayPaymentVerifyRequest request, CancellationToken cancellationToken = default)
    {
        RazorpayPaymentVerifyResult result = new()
        {
            OrderId = request.RazorpayOrderId,
            PaymentId = request.RazorpayPaymentId
        };

        if (string.IsNullOrWhiteSpace(request.RazorpayOrderId) ||
            string.IsNullOrWhiteSpace(request.RazorpayPaymentId) ||
            string.IsNullOrWhiteSpace(request.RazorpaySignature))
        {
            result.IsSuccess = false;
            result.IsValidSignature = false;
            result.ErrorMessage = "Missing required signature verification parameters.";
            return result;
        }

        var settings = await _clientProvider.GetSettingsAsync(cancellationToken);

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

        // Fetch live payment details from Razorpay SDK
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

    public async Task<RazorpayPaymentDetails?> GetPaymentAsync(string paymentId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(paymentId))
            return null;

        var client = await _clientProvider.GetClientAsync(cancellationToken);
        Payment payment = client.Payment.Fetch(paymentId);
        if (payment == null)
            return null;

        long amountPaise = payment["amount"] != null ? Convert.ToInt64(payment["amount"]) : 0;
        return new RazorpayPaymentDetails
        {
            Id = payment["id"]?.ToString() ?? paymentId,
            OrderId = payment["order_id"]?.ToString(),
            Amount = amountPaise / 100m,
            Currency = payment["currency"]?.ToString() ?? "INR",
            Status = payment["status"]?.ToString() ?? string.Empty,
            Method = payment["method"]?.ToString() ?? string.Empty,
            Description = payment["description"]?.ToString(),
            Vpa = payment["vpa"]?.ToString(),
            Bank = payment["bank"]?.ToString(),
            Wallet = payment["wallet"]?.ToString(),
            Email = payment["email"]?.ToString(),
            Contact = payment["contact"]?.ToString(),
            ErrorCode = payment["error_code"]?.ToString(),
            ErrorDescription = payment["error_description"]?.ToString()
        };
    }

    public async Task<bool> CapturePaymentAsync(string paymentId, decimal amount, string currency = "INR", CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(paymentId) || amount <= 0)
            return false;

        var client = await _clientProvider.GetClientAsync(cancellationToken);
        Payment payment = client.Payment.Fetch(paymentId);
        if (payment == null)
            return false;

        var options = new Dictionary<string, object>
        {
            { "amount", (long)Math.Round(amount * 100m) },
            { "currency", currency }
        };

        Payment captured = payment.Capture(options);
        return captured != null && string.Equals(captured["status"]?.ToString(), "captured", StringComparison.OrdinalIgnoreCase);
    }
}
