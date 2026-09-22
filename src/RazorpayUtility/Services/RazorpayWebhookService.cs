using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using RazorpayUtility.Configuration;
using RazorpayUtility.Interfaces;

namespace RazorpayUtility.Services;

/// <summary>
/// Service for verifying Razorpay webhooks and dispatching events to handlers.
/// </summary>
public class RazorpayWebhookService : IRazorpayWebhookService
{
    private readonly IRazorpaySettingsRepository _settingsRepo;
    private readonly IRazorpayWebhookEventHandler? _eventHandler;
    private readonly ILogger<RazorpayWebhookService> _logger;

    public RazorpayWebhookService(
        IRazorpaySettingsRepository settingsRepo,
        ILogger<RazorpayWebhookService> logger,
        IRazorpayWebhookEventHandler? eventHandler = null)
    {
        _settingsRepo = settingsRepo;
        _logger = logger;
        _eventHandler = eventHandler;
    }

    public bool VerifySignature(string jsonPayload, string signatureHeader, string webhookSecret)
    {
        if (string.IsNullOrWhiteSpace(jsonPayload) || string.IsNullOrWhiteSpace(signatureHeader) || string.IsNullOrWhiteSpace(webhookSecret))
        {
            return false;
        }

        try
        {
            byte[] keyBytes = Encoding.UTF8.GetBytes(webhookSecret);
            byte[] payloadBytes = Encoding.UTF8.GetBytes(jsonPayload);

            using var hmac = new HMACSHA256(keyBytes);
            byte[] hash = hmac.ComputeHash(payloadBytes);
            string computedSignature = Convert.ToHexString(hash).ToLowerInvariant();

            return string.Equals(computedSignature, signatureHeader, StringComparison.OrdinalIgnoreCase);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception verifying Razorpay webhook signature: {Message}", ex.Message);
            return false;
        }
    }

    public async Task<bool> ProcessWebhookAsync(string jsonPayload, string signatureHeader, CancellationToken cancellationToken = default)
    {
        RazorpaySettings? settings = await _settingsRepo.GetSettingsAsync(cancellationToken);
        if (settings == null)
        {
            _logger.LogError("Razorpay settings not found while processing webhook.");
            return false;
        }

        // If WebhookSecret is configured, verify HMAC signature
        if (!string.IsNullOrWhiteSpace(settings.WebhookSecret))
        {
            if (!VerifySignature(jsonPayload, signatureHeader, settings.WebhookSecret))
            {
                _logger.LogWarning("Invalid Razorpay webhook signature. Dropping event.");
                return false;
            }
        }

        try
        {
            JObject json = JObject.Parse(jsonPayload);
            string? eventType = json["event"]?.ToString();
            _logger.LogInformation("Processing Razorpay webhook event: {EventType}", eventType);

            if (_eventHandler == null)
            {
                _logger.LogInformation("No custom IRazorpayWebhookEventHandler registered. Event acknowledged.");
                return true;
            }

            JObject? payload = json["payload"] as JObject;
            if (payload == null)
            {
                return true;
            }

            switch (eventType)
            {
                case "payment.captured":
                case "payment.authorized":
                    JObject? paymentPayload = payload["payment"]?["entity"] as JObject;
                    if (paymentPayload != null)
                    {
                        await _eventHandler.HandlePaymentCapturedAsync(paymentPayload, cancellationToken);
                    }
                    break;

                case "payment.failed":
                    JObject? failedPayload = payload["payment"]?["entity"] as JObject;
                    if (failedPayload != null)
                    {
                        await _eventHandler.HandlePaymentFailedAsync(failedPayload, cancellationToken);
                    }
                    break;

                case "order.paid":
                    JObject? orderPayload = payload["order"]?["entity"] as JObject;
                    if (orderPayload != null)
                    {
                        await _eventHandler.HandleOrderPaidAsync(orderPayload, cancellationToken);
                    }
                    break;

                case "refund.processed":
                    JObject? refundPayload = payload["refund"]?["entity"] as JObject;
                    if (refundPayload != null)
                    {
                        await _eventHandler.HandleRefundProcessedAsync(refundPayload, cancellationToken);
                    }
                    break;

                default:
                    _logger.LogInformation("Unhandled Razorpay webhook event: {EventType}", eventType);
                    break;
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing Razorpay webhook payload: {Message}", ex.Message);
            return false;
        }
    }
}