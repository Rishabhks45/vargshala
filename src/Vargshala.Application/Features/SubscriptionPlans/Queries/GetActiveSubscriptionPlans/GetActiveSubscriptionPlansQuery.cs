using MediatR;
using Vargshala.Application.Features.SubscriptionPlans.Infrastructure;
using Vargshala.Contracts.Common;
using Vargshala.Contracts.Subscriptions;

namespace Vargshala.Application.Features.SubscriptionPlans.Queries.GetActiveSubscriptionPlans;

public record GetActiveSubscriptionPlansQuery : IRequest<ApiResponse<List<SubscriptionPlanDto>>>;

public class GetActiveSubscriptionPlansQueryHandler : IRequestHandler<GetActiveSubscriptionPlansQuery, ApiResponse<List<SubscriptionPlanDto>>>
{
    private readonly ISubscriptionPlanRepository _repository;

    public GetActiveSubscriptionPlansQueryHandler(ISubscriptionPlanRepository repository)
    {
        _repository = repository;
    }

    public async Task<ApiResponse<List<SubscriptionPlanDto>>> Handle(
        GetActiveSubscriptionPlansQuery query,
        CancellationToken cancellationToken)
    {
        var plans = await _repository.GetAllActiveAsync(cancellationToken);
        var dtos = plans.Select(p => p.ToDto()).ToList();
        return ApiResponse<List<SubscriptionPlanDto>>.SuccessResponse(dtos);
    }
}
