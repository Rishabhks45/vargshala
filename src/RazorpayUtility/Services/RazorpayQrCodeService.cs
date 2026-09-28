using Microsoft.Extensions.Logging;
using Razorpay.Api;
using RazorpayUtility.Interfaces;
using RazorpayUtility.Models;

namespace RazorpayUtility.Services;

/// <summary>
/// Service for generating and managing Razorpay Dynamic UPI QR Codes using the official Razorpay SDK.
/// </summary>
public class RazorpayQrCodeService : IRazorpayQrCodeService
{
    private readonly IRazorpayClientProvider _clientProvider;
    private readonly ILogger<RazorpayQrCodeService> _logger;

    public RazorpayQrCodeService(
        IRazorpayClientProvider clientProvider,
        ILogger<RazorpayQrCodeService> logger)
    {
        _clientProvider = clientProvider;
        _logger = logger;
    }

    public async Task<RazorpayQrCodeResponse> CreateUpiQrCodeAsync(RazorpayCreateQrCodeRequest request, CancellationToken cancellationToken = default)
    {
        long amountPaise = (long)Math.Round(request.Amount * 100m);
        if (amountPaise <= 0)
        {
            return new RazorpayQrCodeResponse
            {
                IsSuccess = false,
                ErrorMessage = "QR code payment amount must be greater than 0."
            };
        }

        var client = await _clientProvider.GetClientAsync(cancellationToken);
        var settings = await _clientProvider.GetSettingsAsync(cancellationToken);

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

        if (request.CloseBy.HasValue)
        {
            long epoch = new DateTimeOffset(request.CloseBy.Value.ToUniversalTime()).ToUnixTimeSeconds();
            payload["close_by"] = epoch;
        }

        QrCode qrCode = client.QrCode.Create(payload);
        if (qrCode == null)
        {
            return new RazorpayQrCodeResponse
            {
                IsSuccess = false,
                ErrorMessage = "Failed to create QR code via Razorpay SDK."
            };
        }

        return new RazorpayQrCodeResponse
        {
            IsSuccess = true,
            QrCodeId = qrCode["id"]?.ToString() ?? string.Empty,
            ImageUrl = qrCode["image_url"]?.ToString() ?? string.Empty,
            Status = qrCode["status"]?.ToString() ?? "active",
            Amount = request.Amount,
            Notes = request.Notes,
            CloseBy = request.CloseBy
        };
    }

    public async Task<RazorpayQrCodeResponse?> GetQrCodeAsync(string qrCodeId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(qrCodeId))
            return null;

        var client = await _clientProvider.GetClientAsync(cancellationToken);
        QrCode qrCode = client.QrCode.Fetch(qrCodeId);
        if (qrCode == null)
            return null;

        long amountPaise = qrCode["payment_amount"] != null ? Convert.ToInt64(qrCode["payment_amount"]) : 0;
        long receivedPaise = qrCode["payments_amount_received"] != null ? Convert.ToInt64(qrCode["payments_amount_received"]) : 0;

        return new RazorpayQrCodeResponse
        {
            QrCodeId = qrCode["id"]?.ToString() ?? qrCodeId,
            ImageUrl = qrCode["image_url"]?.ToString() ?? string.Empty,
            Status = qrCode["status"]?.ToString() ?? string.Empty,
            Amount = amountPaise / 100m,
            PaymentsAmountReceived = receivedPaise / 100m
        };
    }

    public async Task<bool> CloseQrCodeAsync(string qrCodeId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(qrCodeId))
            return false;

        var client = await _clientProvider.GetClientAsync(cancellationToken);
        QrCode qrCode = client.QrCode.Fetch(qrCodeId);
        if (qrCode == null)
            return false;

        QrCode closed = qrCode.Close();
        return closed != null && string.Equals(closed["status"]?.ToString(), "closed", StringComparison.OrdinalIgnoreCase);
    }
}
