using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using RazorpayUtility.Interfaces;
using Vargshala.Application.Features.OrganizationSubscriptions.Infrastructure;

namespace Vargshala.Application.Services;

/// <summary>
/// Domain-level event handler for Razorpay Webhook events.
/// Handles subscription activation, payment recording, failure logging, and refunds upon webhook notification.
/// Fast, exception-free execution using guard clauses and early exits.
/// </summary>
public class RazorpayWebhookEventHandler : IRazorpayWebhookEventHandler
{
    private readonly ILogger<RazorpayWebhookEventHandler> _logger;
    private readonly IOrganizationSubscriptionRepository _repository;
    private readonly IRazorpayClientProvider _clientProvider;

    public RazorpayWebhookEventHandler(
        ILogger<RazorpayWebhookEventHandler> logger,
        IOrganizationSubscriptionRepository repository,
        IRazorpayClientProvider clientProvider)
    {
        _logger = logger;
        _repository = repository;
        _clientProvider = clientProvider;
    }

    public async Task HandlePaymentCapturedAsync(JObject paymentPayload, CancellationToken cancellationToken = default)
    {
        string paymentId = paymentPayload["id"]?.ToString() ?? string.Empty;
        string orderId = paymentPayload["order_id"]?.ToString() ?? string.Empty;
        long amountPaise = paymentPayload["amount"]?.Value<long>() ?? 0;
        decimal amount = amountPaise / 100m;
        string method = paymentPayload["method"]?.ToString() ?? "Razorpay";
        JObject? notes = paymentPayload["notes"] as JObject;

        _logger.LogInformation("Razorpay Webhook: Payment captured: {PaymentId}, Order: {OrderId}, Amount: ₹{Amount}",
            paymentId, orderId, amount);

        // Fallback to order notes if payment notes are missing
        if ((notes == null || notes["OrganizationId"] == null) && !string.IsNullOrWhiteSpace(orderId))
        {
            var client = await _clientProvider.GetClientAsync(cancellationToken);
            var rzpOrder = client.Order.Fetch(orderId);
            object? rawNotes = rzpOrder?["notes"];
            if (rawNotes != null)
            {
                notes = JObject.FromObject(rawNotes);
            }
        }

        if (notes == null)
        {
            _logger.LogWarning("Razorpay Webhook: No metadata notes found for payment {PaymentId}. Cannot activate subscription.", paymentId);
            return;
        }

        string? orgIdStr = notes["OrganizationId"]?.ToString();
        string? planIdStr = notes["PlanId"]?.ToString();
        string? couponCode = notes["CouponCode"]?.ToString();

        if (!Guid.TryParse(orgIdStr, out var orgId) || !Guid.TryParse(planIdStr, out var planId))
        {
            _logger.LogWarning("Razorpay Webhook: Invalid OrgId ({OrgId}) or PlanId ({PlanId}) in notes for payment {PaymentId}",
                orgIdStr, planIdStr, paymentId);
            return;
        }

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

        _logger.LogInformation("Subscription successfully activated/verified via webhook for Org: {OrgId}, Plan: {PlanId}",
            orgId, planId);
    }

    public async Task HandleOrderPaidAsync(JObject orderPayload, CancellationToken cancellationToken = default)
    {
        string orderId = orderPayload["id"]?.ToString() ?? string.Empty;
        long amountPaise = orderPayload["amount_paid"]?.Value<long>() ?? orderPayload["amount"]?.Value<long>() ?? 0;
        decimal amount = amountPaise / 100m;
        JObject? notes = orderPayload["notes"] as JObject;

        _logger.LogInformation("Razorpay Webhook: Order marked paid: {OrderId}, Amount: ₹{Amount}", orderId, amount);

        if (notes == null) return;

        string? orgIdStr = notes["OrganizationId"]?.ToString();
        string? planIdStr = notes["PlanId"]?.ToString();
        string? couponCode = notes["CouponCode"]?.ToString();

        if (!Guid.TryParse(orgIdStr, out var orgId) || !Guid.TryParse(planIdStr, out var planId)) return;

        if (!string.IsNullOrWhiteSpace(couponCode))
        {
            var coupon = await _repository.GetCouponByCodeAsync(couponCode.Trim(), cancellationToken);
            if (coupon != null)
            {
                await _repository.IncrementCouponUsedCountAsync(coupon.Id, cancellationToken);
            }
        }

        string remarks = $"Webhook Order Paid: {orderId}";
        await _repository.CreateOrRenewSubscriptionAsync(
            orgId,
            planId,
            amount,
            "Razorpay",
            orderId,
            orderId,
            remarks,
            cancellationToken);
    }

    public async Task HandlePaymentFailedAsync(JObject paymentPayload, CancellationToken cancellationToken = default)
    {
        string paymentId = paymentPayload["id"]?.ToString() ?? string.Empty;
        string orderId = paymentPayload["order_id"]?.ToString() ?? string.Empty;
        long amountPaise = paymentPayload["amount"]?.Value<long>() ?? 0;
        decimal amount = amountPaise / 100m;
        string method = paymentPayload["method"]?.ToString() ?? "Razorpay";
        string? errorCode = paymentPayload["error_code"]?.ToString();
        string? errorDesc = paymentPayload["error_description"]?.ToString();
        string? errorReason = paymentPayload["error_reason"]?.ToString();
        JObject? notes = paymentPayload["notes"] as JObject;

        if ((notes == null || notes["OrganizationId"] == null) && !string.IsNullOrWhiteSpace(orderId))
        {
            var client = await _clientProvider.GetClientAsync(cancellationToken);
            var rzpOrder = client.Order.Fetch(orderId);
            object? rawNotes = rzpOrder?["notes"];
            if (rawNotes != null)
            {
                notes = JObject.FromObject(rawNotes);
            }
        }

        Guid? orgId = null;
        if (notes != null && Guid.TryParse(notes["OrganizationId"]?.ToString(), out var parsedOrgId))
        {
            orgId = parsedOrgId;
        }

        string failureReason = $"Payment Failed: {errorCode} ({errorReason}) - {errorDesc}".Trim();
        _logger.LogWarning("Razorpay Webhook: Payment failed: {PaymentId}, Order: {OrderId}, Reason: {FailureReason}",
            paymentId, orderId, failureReason);

        await _repository.RecordFailedPaymentAsync(
            orgId,
            amount,
            method,
            paymentId,
            orderId,
            failureReason,
            cancellationToken);
    }

    public async Task HandleRefundProcessedAsync(JObject refundPayload, CancellationToken cancellationToken = default)
    {
        string refundId = refundPayload["id"]?.ToString() ?? string.Empty;
        string paymentId = refundPayload["payment_id"]?.ToString() ?? string.Empty;
        long amountPaise = refundPayload["amount"]?.Value<long>() ?? 0;
        decimal refundAmount = amountPaise / 100m;

        _logger.LogInformation("Razorpay Webhook: Refund processed: {RefundId} for Payment: {PaymentId}, Amount: ₹{Amount}",
            refundId, paymentId, refundAmount);

        await _repository.RecordRefundAsync(paymentId, refundId, refundAmount, cancellationToken);
    }
}
