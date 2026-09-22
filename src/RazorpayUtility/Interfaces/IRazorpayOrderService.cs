using RazorpayUtility.Models;

namespace RazorpayUtility.Interfaces;

/// <summary>
/// Interface for creating and querying Razorpay Orders.
/// </summary>
public interface IRazorpayOrderService
{
    Task<RazorpayOrderResponse> CreateOrderAsync(RazorpayCreateOrderRequest request, CancellationToken cancellationToken = default);
    Task<RazorpayOrderResponse?> GetOrderAsync(string orderId, CancellationToken cancellationToken = default);
    Task<List<RazorpayPaymentDetails>> GetOrderPaymentsAsync(string orderId, CancellationToken cancellationToken = default);
}
