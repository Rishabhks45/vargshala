using MediatR;
using Vargshala.Application.Abstractions.CurrentUser;
using Vargshala.Application.Features.OrganizationSubscriptions.Infrastructure;
using Vargshala.Contracts.Common;
using Vargshala.Contracts.Subscriptions;

namespace Vargshala.Application.Features.OrganizationSubscriptions.Queries.GetSubscriptionHistory;

public record GetSubscriptionHistoryQuery : IRequest<ApiResponse<List<SubscriptionBillingHistoryDto>>>;

public class GetSubscriptionHistoryQueryHandler : IRequestHandler<GetSubscriptionHistoryQuery, ApiResponse<List<SubscriptionBillingHistoryDto>>>
{
    private readonly IOrganizationSubscriptionRepository _repository;
    private readonly ICurrentUser _currentUser;

    public GetSubscriptionHistoryQueryHandler(
        IOrganizationSubscriptionRepository repository,
        ICurrentUser currentUser)
    {
        _repository = repository;
        _currentUser = currentUser;
    }

    public async Task<ApiResponse<List<SubscriptionBillingHistoryDto>>> Handle(
        GetSubscriptionHistoryQuery request,
        CancellationToken cancellationToken)
    {
        if (!_currentUser.OrganizationId.HasValue)
        {
            return ApiResponse<List<SubscriptionBillingHistoryDto>>.FailureResponse("User does not belong to an organization.");
        }

        var history = await _repository.GetBillingHistoryAsync(_currentUser.OrganizationId.Value, cancellationToken);
        return ApiResponse<List<SubscriptionBillingHistoryDto>>.SuccessResponse(history);
    }
}
