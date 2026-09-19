using MediatR;
using Vargshala.Application.Abstractions.CurrentUser;
using Vargshala.Application.Features.OrgAdmin.Fees.Infrastructure;
using Vargshala.Contracts.Common;
using Vargshala.Contracts.Fees;

namespace Vargshala.Application.Features.OrgAdmin.Fees.Queries.GetPaymentReceipt;

public record GetPaymentReceiptQuery(Guid PaymentId) : IRequest<ApiResponse<PaymentDto>>;

public class GetPaymentReceiptQueryHandler : IRequestHandler<GetPaymentReceiptQuery, ApiResponse<PaymentDto>>
{
    private readonly IFeeRepository _feeRepository;
    private readonly ICurrentUser _currentUser;

    public GetPaymentReceiptQueryHandler(IFeeRepository feeRepository, ICurrentUser currentUser)
    {
        _feeRepository = feeRepository;
        _currentUser = currentUser;
    }

    public async Task<ApiResponse<PaymentDto>> Handle(GetPaymentReceiptQuery query, CancellationToken cancellationToken)
    {
        var orgId = _currentUser.OrganizationId;
        if (!orgId.HasValue || orgId.Value == Guid.Empty)
        {
            return ApiResponse<PaymentDto>.FailureResponse("No active organization context found.");
        }

        var payment = await _feeRepository.GetPaymentReceiptByIdAsync(query.PaymentId, orgId.Value, cancellationToken);
        if (payment == null)
        {
            return ApiResponse<PaymentDto>.FailureResponse("Payment receipt not found.");
        }

        return ApiResponse<PaymentDto>.SuccessResponse(payment.ToDto());
    }
}
