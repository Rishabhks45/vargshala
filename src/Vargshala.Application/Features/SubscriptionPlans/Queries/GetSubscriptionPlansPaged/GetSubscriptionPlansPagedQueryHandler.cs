using MediatR;
using Vargshala.Application.Features.SubscriptionPlans.Infrastructure;
using Vargshala.Contracts.Common;
using Vargshala.Contracts.Subscriptions;

namespace Vargshala.Application.Features.SubscriptionPlans.Queries.GetSubscriptionPlansPaged;

public class GetSubscriptionPlansPagedQueryHandler : IRequestHandler<GetSubscriptionPlansPagedQuery, ApiResponse<PagedResponse<SubscriptionPlanDto>>>
{
    private readonly ISubscriptionPlanRepository _repository;

    public GetSubscriptionPlansPagedQueryHandler(ISubscriptionPlanRepository repository)
    {
        _repository = repository;
    }

    public async Task<ApiResponse<PagedResponse<SubscriptionPlanDto>>> Handle(
        GetSubscriptionPlansPagedQuery query,
        CancellationToken cancellationToken)
    {
        var pagedRequest = query.Request ?? new PagedRequest();
        var (items, totalRecords) = await _repository.GetPagedAsync(
            pagedRequest,
            query.IsActive,
            query.BillingCycle,
            cancellationToken);

        var dtos = items.Select(p => p.ToDto()).ToList();
        var response = PagedResponse<SubscriptionPlanDto>.Create(dtos, totalRecords, pagedRequest.PageNumber, pagedRequest.PageSize);
        return ApiResponse<PagedResponse<SubscriptionPlanDto>>.SuccessResponse(response);
    }
}
