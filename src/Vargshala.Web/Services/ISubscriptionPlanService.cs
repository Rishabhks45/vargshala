using Vargshala.Contracts.Common;
using Vargshala.Contracts.Subscriptions;
using Vargshala.SharedKernel.Enums;

namespace Vargshala.Web.Services;

public interface ISubscriptionPlanService
{
    Task<ApiResponse<PagedResponse<SubscriptionPlanDto>>> GetPlansPagedAsync(
        PagedRequest? request = null,
        bool? isActive = null,
        BillingCycle? billingCycle = null,
        CancellationToken cancellationToken = default);

    Task<ApiResponse<List<SubscriptionPlanDto>>> GetActivePlansAsync(CancellationToken cancellationToken = default);

    Task<ApiResponse<SubscriptionPlanDto>> GetPlanByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<ApiResponse<SubscriptionPlanDto>> CreatePlanAsync(CreateSubscriptionPlanRequest request, CancellationToken cancellationToken = default);

    Task<ApiResponse<SubscriptionPlanDto>> UpdatePlanAsync(Guid id, UpdateSubscriptionPlanRequest request, CancellationToken cancellationToken = default);

    Task<ApiResponse<SubscriptionPlanDto>> TogglePlanStatusAsync(Guid id, CancellationToken cancellationToken = default);

    Task<ApiResponse<bool>> DeletePlanAsync(Guid id, CancellationToken cancellationToken = default);
}
