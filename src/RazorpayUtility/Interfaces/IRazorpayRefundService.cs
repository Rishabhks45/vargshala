using RazorpayUtility.Models;

namespace RazorpayUtility.Interfaces;

/// <summary>
/// Interface for creating and tracking Razorpay refunds.
/// </summary>
public interface IRazorpayRefundService
{
    Task<RazorpayRefundResponse> CreateRefundAsync(RazorpayRefundRequest request, CancellationToken cancellationToken = default);
    Task<RazorpayRefundResponse?> GetRefundAsync(string refundId, CancellationToken cancellationToken = default);
}
