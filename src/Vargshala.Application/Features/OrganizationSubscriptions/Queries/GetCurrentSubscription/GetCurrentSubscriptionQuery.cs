using MediatR;
using Vargshala.Application.Abstractions.CurrentUser;
using Vargshala.Application.Features.OrganizationSubscriptions.Infrastructure;
using Vargshala.Contracts.Common;
using Vargshala.Contracts.Subscriptions;

namespace Vargshala.Application.Features.OrganizationSubscriptions.Queries.GetCurrentSubscription;

public record GetCurrentSubscriptionQuery : IRequest<ApiResponse<QuotaUsageDto>>;

public class GetCurrentSubscriptionQueryHandler : IRequestHandler<GetCurrentSubscriptionQuery, ApiResponse<QuotaUsageDto>>
{
    private readonly IOrganizationSubscriptionRepository _repository;
    private readonly ICurrentUser _currentUser;

    public GetCurrentSubscriptionQueryHandler(
        IOrganizationSubscriptionRepository repository,
        ICurrentUser currentUser)
    {
        _repository = repository;
        _currentUser = currentUser;
    }

    public async Task<ApiResponse<QuotaUsageDto>> Handle(
        GetCurrentSubscriptionQuery request,
        CancellationToken cancellationToken)
    {
        if (!_currentUser.OrganizationId.HasValue)
        {
            return ApiResponse<QuotaUsageDto>.FailureResponse("User does not belong to an organization.");
        }

        var quotaUsage = await _repository.GetQuotaUsageAsync(_currentUser.OrganizationId.Value, cancellationToken);
        return ApiResponse<QuotaUsageDto>.SuccessResponse(quotaUsage);
    }
}
