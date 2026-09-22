using RazorpayUtility.Models;

namespace RazorpayUtility.Interfaces;

/// <summary>
/// Interface for creating and managing Dynamic UPI QR Codes via Razorpay QR Code API.
/// </summary>
public interface IRazorpayQrCodeService
{
    Task<RazorpayQrCodeResponse> CreateUpiQrCodeAsync(RazorpayCreateQrCodeRequest request, CancellationToken cancellationToken = default);
    Task<RazorpayQrCodeResponse?> GetQrCodeAsync(string qrCodeId, CancellationToken cancellationToken = default);
    Task<bool> CloseQrCodeAsync(string qrCodeId, CancellationToken cancellationToken = default);
}
