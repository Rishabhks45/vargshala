using Vargshala.Contracts.Common;
using Vargshala.Contracts.Payments;
using Vargshala.Contracts.Subscriptions;

namespace Vargshala.Web.Services;

public interface IPaymentLogService
{
    Task<ApiResponse<PagedResponse<PaymentLogDto>>> GetPaymentLogsPagedAsync(
        PagedRequest request,
        string? method = null,
        string? status = null,
        CancellationToken cancellationToken = default);

    Task<ApiResponse<PaymentLogsStatsDto>> GetPaymentLogsStatsAsync(
        CancellationToken cancellationToken = default);

    Task<byte[]?> GetSubscriptionReceiptPdfAsync(
        Guid paymentId,
        CancellationToken cancellationToken = default);

    Task<ApiResponse<SubscriptionPaymentReceiptDto>> GetSubscriptionReceiptAsync(
        Guid paymentId,
        CancellationToken cancellationToken = default);
}
