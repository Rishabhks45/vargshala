using Newtonsoft.Json.Linq;

namespace RazorpayUtility.Interfaces;

/// <summary>
/// Application-level callback interface for Razorpay webhook business logic handling.
/// </summary>
public interface IRazorpayWebhookEventHandler
{
    Task HandlePaymentCapturedAsync(JObject paymentPayload, CancellationToken cancellationToken = default);
    Task HandlePaymentFailedAsync(JObject paymentPayload, CancellationToken cancellationToken = default);
    Task HandleOrderPaidAsync(JObject orderPayload, CancellationToken cancellationToken = default);
    Task HandleRefundProcessedAsync(JObject refundPayload, CancellationToken cancellationToken = default);
}
