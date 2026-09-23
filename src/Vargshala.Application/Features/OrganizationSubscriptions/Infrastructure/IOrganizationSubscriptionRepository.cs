using Vargshala.Contracts.Subscriptions;
using Vargshala.Domain.Entities;

namespace Vargshala.Application.Features.OrganizationSubscriptions.Infrastructure;

public interface IOrganizationSubscriptionRepository
{
    Task<OrganizationSubscription?> GetCurrentActiveSubscriptionAsync(Guid organizationId, CancellationToken cancellationToken = default);
    Task<QuotaUsageDto> GetQuotaUsageAsync(Guid organizationId, CancellationToken cancellationToken = default);
    Task<List<SubscriptionBillingHistoryDto>> GetBillingHistoryAsync(Guid organizationId, CancellationToken cancellationToken = default);
    Task<SubscriptionPlan?> GetPlanByIdAsync(Guid planId, CancellationToken cancellationToken = default);
    Task<Coupon?> GetCouponByCodeAsync(string code, CancellationToken cancellationToken = default);
    Task IncrementCouponUsedCountAsync(Guid couponId, CancellationToken cancellationToken = default);
    Task<SubscriptionPaymentReceiptDto?> GetSubscriptionPaymentReceiptAsync(Guid paymentId, CancellationToken cancellationToken = default);
    Task<OrganizationSubscription> CreateOrRenewSubscriptionAsync(
        Guid organizationId,
        Guid planId,
        decimal amount,
        string paymentMethod,
        string transactionRef,
        string receiptNumber,
        string? remarks,
        CancellationToken cancellationToken = default);
}
