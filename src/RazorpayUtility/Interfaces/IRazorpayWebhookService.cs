namespace RazorpayUtility.Interfaces;

/// <summary>
/// Interface for processing and verifying incoming Razorpay webhooks.
/// </summary>
public interface IRazorpayWebhookService
{
    Task<bool> ProcessWebhookAsync(string jsonPayload, string signatureHeader, CancellationToken cancellationToken = default);
    bool VerifySignature(string jsonPayload, string signatureHeader, string webhookSecret);
}
