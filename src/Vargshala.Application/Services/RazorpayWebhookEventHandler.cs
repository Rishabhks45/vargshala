using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using RazorpayUtility.Interfaces;
using Vargshala.Application.Features.OrganizationSubscriptions.Infrastructure;

namespace Vargshala.Application.Services;

/// <summary>
/// Domain-level event handler for Razorpay Webhook events.
/// Handles subscription activation, payment recording, and coupon usage upon webhook notification.
/// </summary>
public class RazorpayWebhookEventHandler : IRazorpayWebhookEventHandler
{
    private readonly ILogger<RazorpayWebhookEventHandler> _logger;
    private readonly IOrganizationSubscriptionRepository _repository;

    public RazorpayWebhookEventHandler(
        ILogger<RazorpayWebhookEventHandler> logger,
        IOrganizationSubscriptionRepository repository)
    {
        _logger = logger;
        _repository = repository;
    }

    public async Task HandlePaymentCapturedAsync(JObject paymentPayload, CancellationToken cancellationToken = default)
    {
        try
        {
            string paymentId = paymentPayload["id"]?.ToString() ?? string.Empty;
            string orderId = paymentPayload["order_id"]?.ToString() ?? string.Empty;
            long amountPaise = paymentPayload["amount"]?.Value<long>() ?? 0;
            decimal amount = amountPaise / 100m;
            string method = paymentPayload["method"]?.ToString() ?? "Razorpay";
            JObject? notes = paymentPayload["notes"] as JObject;

            _logger.LogInformation("Razorpay Webhook: Payment captured: {PaymentId}, Order: {OrderId}, Amount: ₹{Amount}",
                paymentId, orderId, amount);

            if (notes != null)
            {
                string? orgIdStr = notes["OrganizationId"]?.ToString();
                string? planIdStr = notes["PlanId"]?.ToString();
                string? couponCode = notes["CouponCode"]?.ToString();

                if (Guid.TryParse(orgIdStr, out var orgId) && Guid.TryParse(planIdStr, out var planId))
                {
                    if (!string.IsNullOrWhiteSpace(couponCode))
                    {
                        var coupon = await _repository.GetCouponByCodeAsync(couponCode.Trim(), cancellationToken);
                        if (coupon != null)
                        {
                            await _repository.IncrementCouponUsedCountAsync(coupon.Id, cancellationToken);
                        }
                    }

                    string remarks = $"Webhook Confirmed: {paymentId}";
                    await _repository.CreateOrRenewSubscriptionAsync(
                        orgId,
                        planId,
                        amount,
                        method,
                        paymentId,
                        orderId,
                        remarks,
                        cancellationToken);

                    _logger.LogInformation("Subscription successfully activated via webhook for Org: {OrgId}, Plan: {PlanId}",
                        orgId, planId);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling Razorpay payment.captured webhook");
            throw;
        }
    }

    public Task HandleOrderPaidAsync(JObject orderPayload, CancellationToken cancellationToken = default)
    {
        string orderId = orderPayload["id"]?.ToString() ?? string.Empty;
        _logger.LogInformation("Razorpay Webhook: Order marked paid: {OrderId}", orderId);
        return Task.CompletedTask;
    }

    public Task HandlePaymentFailedAsync(JObject paymentPayload, CancellationToken cancellationToken = default)
    {
        string paymentId = paymentPayload["id"]?.ToString() ?? string.Empty;
        string? errorCode = paymentPayload["error_code"]?.ToString();
        string? errorDesc = paymentPayload["error_description"]?.ToString();

        _logger.LogWarning("Razorpay Webhook: Payment failed: {PaymentId}, Code: {ErrorCode}, Reason: {ErrorDesc}",
            paymentId, errorCode, errorDesc);

        return Task.CompletedTask;
    }

    public Task HandleRefundProcessedAsync(JObject refundPayload, CancellationToken cancellationToken = default)
    {
        string refundId = refundPayload["id"]?.ToString() ?? string.Empty;
        string paymentId = refundPayload["payment_id"]?.ToString() ?? string.Empty;
        long amountPaise = refundPayload["amount"]?.Value<long>() ?? 0;

        _logger.LogInformation("Razorpay Webhook: Refund processed: {RefundId} for Payment: {PaymentId}, Amount: ₹{Amount}",
            refundId, paymentId, amountPaise / 100m);

        return Task.CompletedTask;
    }
}
