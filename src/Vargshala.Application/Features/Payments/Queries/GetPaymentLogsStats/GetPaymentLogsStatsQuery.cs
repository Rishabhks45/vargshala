using MediatR;
using Vargshala.Application.Features.Payments.Infrastructure;
using Vargshala.Contracts.Common;
using Vargshala.Contracts.Payments;

namespace Vargshala.Application.Features.Payments.Queries.GetPaymentLogsStats;

public record GetPaymentLogsStatsQuery : IRequest<ApiResponse<PaymentLogsStatsDto>>;

public class GetPaymentLogsStatsQueryHandler
    : IRequestHandler<GetPaymentLogsStatsQuery, ApiResponse<PaymentLogsStatsDto>>
{
    private readonly IPaymentLogRepository _repository;

    public GetPaymentLogsStatsQueryHandler(IPaymentLogRepository repository)
    {
        _repository = repository;
    }

    public async Task<ApiResponse<PaymentLogsStatsDto>> Handle(
        GetPaymentLogsStatsQuery request,
        CancellationToken cancellationToken)
    {
        var stats = await _repository.GetPaymentLogsStatsAsync(cancellationToken);
        return ApiResponse<PaymentLogsStatsDto>.SuccessResponse(stats);
    }
}
