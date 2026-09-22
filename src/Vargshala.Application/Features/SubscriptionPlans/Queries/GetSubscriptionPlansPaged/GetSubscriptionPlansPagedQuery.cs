using MediatR;
using Vargshala.Contracts.Common;
using Vargshala.Contracts.Subscriptions;
using Vargshala.SharedKernel.Enums;

namespace Vargshala.Application.Features.SubscriptionPlans.Queries.GetSubscriptionPlansPaged;

public record GetSubscriptionPlansPagedQuery(
    PagedRequest Request,
    bool? IsActive = null,
    BillingCycle? BillingCycle = null) : IRequest<ApiResponse<PagedResponse<SubscriptionPlanDto>>>;
