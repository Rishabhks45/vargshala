using MediatR;
using Vargshala.Application.Abstractions.CurrentUser;
using Vargshala.Application.Features.OrganizationSubscriptions.Infrastructure;
using Vargshala.Contracts.Common;
using Vargshala.Contracts.Subscriptions;

namespace Vargshala.Application.Features.OrganizationSubscriptions.Queries.GetSubscriptionPaymentReceipt;

public record GetSubscriptionPaymentReceiptQuery(Guid PaymentId) : IRequest<ApiResponse<SubscriptionPaymentReceiptDto>>;

public class GetSubscriptionPaymentReceiptQueryHandler
    : IRequestHandler<GetSubscriptionPaymentReceiptQuery, ApiResponse<SubscriptionPaymentReceiptDto>>
{
    private readonly IOrganizationSubscriptionRepository _repository;
    private readonly ICurrentUser _currentUser;

    public GetSubscriptionPaymentReceiptQueryHandler(
        IOrganizationSubscriptionRepository repository,
        ICurrentUser currentUser)
    {
        _repository = repository;
        _currentUser = currentUser;
    }

    public async Task<ApiResponse<SubscriptionPaymentReceiptDto>> Handle(
        GetSubscriptionPaymentReceiptQuery request,
        CancellationToken cancellationToken)
    {
        var receipt = await _repository.GetSubscriptionPaymentReceiptAsync(request.PaymentId, cancellationToken);
        if (receipt == null)
        {
            return ApiResponse<SubscriptionPaymentReceiptDto>.FailureResponse("Subscription payment receipt not found.");
        }

        // Multi-tenancy check: If user is OrganizationAdmin, enforce organization boundary
        if (_currentUser.OrganizationId.HasValue && 
            _currentUser.OrganizationId.Value != Guid.Empty &&
            !_currentUser.IsSuperAdmin)
        {
            if (receipt.OrganizationId != _currentUser.OrganizationId.Value)
            {
                return ApiResponse<SubscriptionPaymentReceiptDto>.FailureResponse("Access denied to this payment receipt.");
            }
        }

        return ApiResponse<SubscriptionPaymentReceiptDto>.SuccessResponse(receipt);
    }
}
