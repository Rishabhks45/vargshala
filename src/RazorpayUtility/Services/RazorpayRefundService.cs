using Microsoft.Extensions.Logging;
using Razorpay.Api;
using RazorpayUtility.Interfaces;
using RazorpayUtility.Models;

namespace RazorpayUtility.Services;

/// <summary>
/// Service for initiating and checking Razorpay refunds using the official Razorpay SDK.
/// </summary>
public class RazorpayRefundService : IRazorpayRefundService
{
    private readonly IRazorpayClientProvider _clientProvider;
    private readonly ILogger<RazorpayRefundService> _logger;

    public RazorpayRefundService(
        IRazorpayClientProvider clientProvider,
        ILogger<RazorpayRefundService> logger)
    {
        _clientProvider = clientProvider;
        _logger = logger;
    }

    public async Task<RazorpayRefundResponse> CreateRefundAsync(RazorpayRefundRequest request, CancellationToken cancellationToken = default)
    {
        RazorpayRefundResponse response = new();

        if (string.IsNullOrWhiteSpace(request.PaymentId))
        {
            response.IsSuccess = false;
            response.ErrorMessage = "PaymentId is required to issue a refund.";
            return response;
        }

        var client = await _clientProvider.GetClientAsync(cancellationToken);
        Payment payment = client.Payment.Fetch(request.PaymentId);
        if (payment == null)
        {
            response.IsSuccess = false;
            response.ErrorMessage = $"Payment not found for Id: {request.PaymentId}";
            return response;
        }

        var options = new Dictionary<string, object>();
        if (request.Amount.HasValue && request.Amount.Value > 0)
        {
            options["amount"] = (long)Math.Round(request.Amount.Value * 100m);
        }

        if (request.Notes != null && request.Notes.Count > 0)
        {
            options["notes"] = request.Notes;
        }

        Refund refund = payment.Refund(options);
        if (refund == null)
        {
            response.IsSuccess = false;
            response.ErrorMessage = "Failed to process refund via Razorpay SDK.";
            return response;
        }

        long amountPaise = refund["amount"] != null ? Convert.ToInt64(refund["amount"]) : 0;
        response.IsSuccess = true;
        response.RefundId = refund["id"]?.ToString() ?? string.Empty;
        response.PaymentId = refund["payment_id"]?.ToString() ?? request.PaymentId;
        response.Amount = amountPaise / 100m;
        response.Currency = refund["currency"]?.ToString() ?? "INR";
        response.Status = refund["status"]?.ToString() ?? "processed";

        return response;
    }

    public async Task<RazorpayRefundResponse?> GetRefundAsync(string refundId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(refundId))
            return null;

        var client = await _clientProvider.GetClientAsync(cancellationToken);
        Refund refund = client.Refund.Fetch(refundId);
        if (refund == null)
            return null;

        long amountPaise = refund["amount"] != null ? Convert.ToInt64(refund["amount"]) : 0;
        return new RazorpayRefundResponse
        {
            RefundId = refund["id"]?.ToString() ?? refundId,
            PaymentId = refund["payment_id"]?.ToString() ?? string.Empty,
            Amount = amountPaise / 100m,
            Currency = refund["currency"]?.ToString() ?? "INR",
            Status = refund["status"]?.ToString() ?? string.Empty
        };
    }
}