using RazorpayUtility.Models;

namespace RazorpayUtility.Interfaces;

/// <summary>
/// Interface for payment signature verification, capture, and payment lookup.
/// </summary>
public interface IRazorpayPaymentService
{
    Task<RazorpayPaymentVerifyResult> VerifyPaymentSignatureAsync(RazorpayPaymentVerifyRequest request, CancellationToken cancellationToken = default);
    Task<RazorpayPaymentDetails?> GetPaymentAsync(string paymentId, CancellationToken cancellationToken = default);
    Task<bool> CapturePaymentAsync(string paymentId, decimal amount, string currency = "INR", CancellationToken cancellationToken = default);
}
