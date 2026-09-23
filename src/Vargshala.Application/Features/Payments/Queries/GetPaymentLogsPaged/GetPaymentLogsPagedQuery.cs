using MediatR;
using Vargshala.Application.Features.Payments.Infrastructure;
using Vargshala.Contracts.Common;
using Vargshala.Contracts.Payments;
using Vargshala.Domain.Entities;

namespace Vargshala.Application.Features.Payments.Queries.GetPaymentLogsPaged;

public record GetPaymentLogsPagedQuery(
    PagedRequest Request,
    string? Method = null,
    string? Status = null,
    PaymentType? PaymentType = null) : IRequest<ApiResponse<PagedResponse<PaymentLogDto>>>;

public class GetPaymentLogsPagedQueryHandler
    : IRequestHandler<GetPaymentLogsPagedQuery, ApiResponse<PagedResponse<PaymentLogDto>>>
{
    private readonly IPaymentLogRepository _repository;

    public GetPaymentLogsPagedQueryHandler(IPaymentLogRepository repository)
    {
        _repository = repository;
    }

    public async Task<ApiResponse<PagedResponse<PaymentLogDto>>> Handle(
        GetPaymentLogsPagedQuery request,
        CancellationToken cancellationToken)
    {
        var result = await _repository.GetPaymentLogsPagedAsync(
            request.Request,
            request.Method,
            request.Status,
            request.PaymentType,
            cancellationToken);

        return ApiResponse<PagedResponse<PaymentLogDto>>.SuccessResponse(result);
    }
}
