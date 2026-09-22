using MediatR;
using Vargshala.Application.Features.SubscriptionPlans.Infrastructure;
using Vargshala.Contracts.Common;
using Vargshala.Contracts.Subscriptions;

namespace Vargshala.Application.Features.SubscriptionPlans.Queries.GetSubscriptionPlanById;

public record GetSubscriptionPlanByIdQuery(Guid Id) : IRequest<ApiResponse<SubscriptionPlanDto>>;

public class GetSubscriptionPlanByIdQueryHandler : IRequestHandler<GetSubscriptionPlanByIdQuery, ApiResponse<SubscriptionPlanDto>>
{
    private readonly ISubscriptionPlanRepository _repository;

    public GetSubscriptionPlanByIdQueryHandler(ISubscriptionPlanRepository repository)
    {
        _repository = repository;
    }

    public async Task<ApiResponse<SubscriptionPlanDto>> Handle(
        GetSubscriptionPlanByIdQuery query,
        CancellationToken cancellationToken)
    {
        var plan = await _repository.GetByIdAsync(query.Id, cancellationToken);
        if (plan == null)
        {
            return ApiResponse<SubscriptionPlanDto>.FailureResponse("Subscription plan not found.");
        }

        return ApiResponse<SubscriptionPlanDto>.SuccessResponse(plan.ToDto());
    }
}
