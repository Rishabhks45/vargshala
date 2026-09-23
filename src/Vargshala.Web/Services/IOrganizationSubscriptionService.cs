using Vargshala.Contracts.Common;
using Vargshala.Contracts.Subscriptions;

namespace Vargshala.Web.Services;

public interface IOrganizationSubscriptionService
{
    Task<ApiResponse<QuotaUsageDto>> GetCurrentSubscriptionAsync(CancellationToken cancellationToken = default);
    Task<ApiResponse<List<SubscriptionBillingHistoryDto>>> GetSubscriptionHistoryAsync(CancellationToken cancellationToken = default);
    Task<ApiResponse<SubscriptionCheckoutResponse>> CheckoutAsync(SubscriptionCheckoutRequest request, CancellationToken cancellationToken = default);
    Task<ApiResponse<OrganizationSubscriptionDto>> ConfirmPaymentAsync(ConfirmSubscriptionPaymentRequest request, CancellationToken cancellationToken = default);
}
