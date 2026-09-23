using MediatR;
using Vargshala.Application.Abstractions.CurrentUser;
using Vargshala.Application.Abstractions.Pdf;
using Vargshala.Application.Features.OrganizationSubscriptions.Infrastructure;
using Vargshala.Contracts.Common;
using Vargshala.Contracts.Subscriptions;

namespace Vargshala.Application.Features.OrganizationSubscriptions.Queries.GetSubscriptionPaymentReceiptPdf;

public record GetSubscriptionPaymentReceiptPdfQuery(Guid PaymentId) : IRequest<ApiResponse<SubscriptionReceiptPdfResult>>;

public class GetSubscriptionPaymentReceiptPdfQueryHandler
    : IRequestHandler<GetSubscriptionPaymentReceiptPdfQuery, ApiResponse<SubscriptionReceiptPdfResult>>
{
    private readonly IOrganizationSubscriptionRepository _repository;
    private readonly ICurrentUser _currentUser;
    private readonly ISubscriptionReceiptPdfGenerator _pdfGenerator;

    public GetSubscriptionPaymentReceiptPdfQueryHandler(
        IOrganizationSubscriptionRepository repository,
        ICurrentUser currentUser,
        ISubscriptionReceiptPdfGenerator pdfGenerator)
    {
        _repository = repository;
        _currentUser = currentUser;
        _pdfGenerator = pdfGenerator;
    }

    public async Task<ApiResponse<SubscriptionReceiptPdfResult>> Handle(
        GetSubscriptionPaymentReceiptPdfQuery request,
        CancellationToken cancellationToken)
    {
        var receipt = await _repository.GetSubscriptionPaymentReceiptAsync(request.PaymentId, cancellationToken);
        if (receipt == null)
        {
            return ApiResponse<SubscriptionReceiptPdfResult>.FailureResponse("Subscription payment receipt not found.");
        }

        // Multi-tenancy check
        if (_currentUser.OrganizationId.HasValue && 
            _currentUser.OrganizationId.Value != Guid.Empty &&
            !_currentUser.IsSuperAdmin)
        {
            if (receipt.OrganizationId != _currentUser.OrganizationId.Value)
            {
                return ApiResponse<SubscriptionReceiptPdfResult>.FailureResponse("Access denied to this payment receipt.");
            }
        }

        var pdfBytes = _pdfGenerator.GenerateSubscriptionReceiptPdf(receipt);
        var cleanReceiptNum = string.IsNullOrWhiteSpace(receipt.ReceiptNumber) 
            ? receipt.PaymentId.ToString()[..8].ToUpperInvariant() 
            : receipt.ReceiptNumber.Replace('/', '-');

        var result = new SubscriptionReceiptPdfResult
        {
            FileBytes = pdfBytes,
            FileName = $"Subscription_Receipt_{cleanReceiptNum}.pdf",
            ContentType = "application/pdf"
        };

        return ApiResponse<SubscriptionReceiptPdfResult>.SuccessResponse(result);
    }
}
