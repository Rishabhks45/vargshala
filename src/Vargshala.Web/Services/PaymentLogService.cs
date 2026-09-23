using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using Vargshala.Contracts.Common;
using Vargshala.Contracts.Payments;
using Vargshala.Contracts.Subscriptions;

namespace Vargshala.Web.Services;

public class PaymentLogService : IPaymentLogService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<PaymentLogService> _logger;

    public PaymentLogService(
        IHttpClientFactory httpClientFactory,
        ILogger<PaymentLogService> logger)
    {
        _httpClient = httpClientFactory.CreateClient("VargshalaApi");
        _logger = logger;
    }

    public async Task<ApiResponse<PagedResponse<PaymentLogDto>>> GetPaymentLogsPagedAsync(
        PagedRequest request,
        string? method = null,
        string? status = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var queryParams = new List<string>
            {
                $"PageNumber={request.PageNumber}",
                $"PageSize={request.PageSize}"
            };

            if (!string.IsNullOrWhiteSpace(request.Search))
                queryParams.Add($"Search={Uri.EscapeDataString(request.Search)}");

            if (!string.IsNullOrWhiteSpace(request.SortBy))
                queryParams.Add($"SortBy={Uri.EscapeDataString(request.SortBy)}");

            if (!string.IsNullOrWhiteSpace(request.SortDirection))
                queryParams.Add($"SortDirection={Uri.EscapeDataString(request.SortDirection)}");

            if (!string.IsNullOrWhiteSpace(method))
                queryParams.Add($"method={Uri.EscapeDataString(method)}");

            if (!string.IsNullOrWhiteSpace(status))
                queryParams.Add($"status={Uri.EscapeDataString(status)}");

            var url = $"api/v1/controlpanel/payments?{string.Join("&", queryParams)}";
            var res = await _httpClient.GetAsync(url, cancellationToken);

            var response = await res.Content.ReadFromJsonAsync<ApiResponse<PagedResponse<PaymentLogDto>>>(cancellationToken: cancellationToken);
            return response ?? ApiResponse<PagedResponse<PaymentLogDto>>.FailureResponse("Failed to fetch payment logs.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching payment logs from control panel");
            return ApiResponse<PagedResponse<PaymentLogDto>>.FailureResponse(ex.Message);
        }
    }

    public async Task<ApiResponse<PaymentLogsStatsDto>> GetPaymentLogsStatsAsync(
        CancellationToken cancellationToken = default)
    {
        try
        {
            var res = await _httpClient.GetAsync("api/v1/controlpanel/payments/stats", cancellationToken);
            var response = await res.Content.ReadFromJsonAsync<ApiResponse<PaymentLogsStatsDto>>(cancellationToken: cancellationToken);
            return response ?? ApiResponse<PaymentLogsStatsDto>.FailureResponse("Failed to fetch payment stats.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching payment stats from control panel");
            return ApiResponse<PaymentLogsStatsDto>.FailureResponse(ex.Message);
        }
    }

    public async Task<byte[]?> GetSubscriptionReceiptPdfAsync(
        Guid paymentId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var res = await _httpClient.GetAsync($"api/v1/controlpanel/payments/{paymentId}/receipt/pdf", cancellationToken);
            if (res.IsSuccessStatusCode)
            {
                return await res.Content.ReadAsByteArrayAsync(cancellationToken);
            }
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error downloading subscription payment receipt PDF {PaymentId}", paymentId);
            return null;
        }
    }

    public async Task<ApiResponse<SubscriptionPaymentReceiptDto>> GetSubscriptionReceiptAsync(
        Guid paymentId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var res = await _httpClient.GetAsync($"api/v1/organization-subscriptions/payments/{paymentId}/receipt", cancellationToken);
            var response = await res.Content.ReadFromJsonAsync<ApiResponse<SubscriptionPaymentReceiptDto>>(cancellationToken: cancellationToken);
            return response ?? ApiResponse<SubscriptionPaymentReceiptDto>.FailureResponse("Receipt not found.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching subscription payment receipt {PaymentId}", paymentId);
            return ApiResponse<SubscriptionPaymentReceiptDto>.FailureResponse(ex.Message);
        }
    }
}
