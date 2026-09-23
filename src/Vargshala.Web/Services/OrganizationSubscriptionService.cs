using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using Vargshala.Contracts.Common;
using Vargshala.Contracts.Subscriptions;

namespace Vargshala.Web.Services;

public class OrganizationSubscriptionService : IOrganizationSubscriptionService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<OrganizationSubscriptionService> _logger;

    public OrganizationSubscriptionService(
        IHttpClientFactory httpClientFactory,
        ILogger<OrganizationSubscriptionService> logger)
    {
        _httpClient = httpClientFactory.CreateClient("VargshalaApi");
        _logger = logger;
    }

    public async Task<ApiResponse<QuotaUsageDto>> GetCurrentSubscriptionAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.GetFromJsonAsync<ApiResponse<QuotaUsageDto>>(
                "api/v1/organization-subscriptions/current",
                cancellationToken);

            if (response != null)
            {
                return response;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching current subscription and quota usage");
            return ApiResponse<QuotaUsageDto>.FailureResponse(ex.Message);
        }

        return ApiResponse<QuotaUsageDto>.FailureResponse("Failed to fetch subscription data.");
    }

    public async Task<ApiResponse<List<SubscriptionBillingHistoryDto>>> GetSubscriptionHistoryAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.GetFromJsonAsync<ApiResponse<List<SubscriptionBillingHistoryDto>>>(
                "api/v1/organization-subscriptions/history",
                cancellationToken);

            if (response != null)
            {
                return response;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching subscription billing history");
            return ApiResponse<List<SubscriptionBillingHistoryDto>>.FailureResponse(ex.Message);
        }

        return ApiResponse<List<SubscriptionBillingHistoryDto>>.FailureResponse("Failed to fetch billing history.");
    }

    public async Task<ApiResponse<SubscriptionCheckoutResponse>> CheckoutAsync(
        SubscriptionCheckoutRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var res = await _httpClient.PostAsJsonAsync(
                "api/v1/organization-subscriptions/checkout",
                request,
                cancellationToken);

            var response = await res.Content.ReadFromJsonAsync<ApiResponse<SubscriptionCheckoutResponse>>(cancellationToken: cancellationToken);
            if (response != null)
            {
                return response;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error initiating subscription checkout");
            return ApiResponse<SubscriptionCheckoutResponse>.FailureResponse(ex.Message);
        }

        return ApiResponse<SubscriptionCheckoutResponse>.FailureResponse("Failed to initiate subscription checkout.");
    }

    public async Task<ApiResponse<OrganizationSubscriptionDto>> ConfirmPaymentAsync(
        ConfirmSubscriptionPaymentRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var res = await _httpClient.PostAsJsonAsync(
                "api/v1/organization-subscriptions/confirm",
                request,
                cancellationToken);

            var response = await res.Content.ReadFromJsonAsync<ApiResponse<OrganizationSubscriptionDto>>(cancellationToken: cancellationToken);
            if (response != null)
            {
                return response;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error confirming subscription payment");
            return ApiResponse<OrganizationSubscriptionDto>.FailureResponse(ex.Message);
        }

        return ApiResponse<OrganizationSubscriptionDto>.FailureResponse("Failed to confirm subscription payment.");
    }
}
