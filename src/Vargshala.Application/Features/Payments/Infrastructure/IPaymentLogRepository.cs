using Vargshala.Contracts.Common;
using Vargshala.Contracts.Payments;
using Vargshala.Contracts.Subscriptions;
using Vargshala.Domain.Entities;

namespace Vargshala.Application.Features.Payments.Infrastructure;

public interface IPaymentLogRepository
{
    Task<PagedResponse<PaymentLogDto>> GetPaymentLogsPagedAsync(
        PagedRequest request,
        string? method,
        string? status,
        PaymentType? paymentType,
        CancellationToken cancellationToken = default);

    Task<PaymentLogsStatsDto> GetPaymentLogsStatsAsync(CancellationToken cancellationToken = default);

    Task<SubscriptionPaymentReceiptDto?> GetPaymentReceiptByIdAsync(Guid paymentId, CancellationToken cancellationToken = default);
}
