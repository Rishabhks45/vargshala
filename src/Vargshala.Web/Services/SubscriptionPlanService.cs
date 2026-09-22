using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using Vargshala.Contracts.Common;
using Vargshala.Contracts.Subscriptions;
using Vargshala.SharedKernel.Enums;

namespace Vargshala.Web.Services;

public class SubscriptionPlanService : ISubscriptionPlanService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<SubscriptionPlanService> _logger;

    public SubscriptionPlanService(
        IHttpClientFactory httpClientFactory,
        ILogger<SubscriptionPlanService> logger)
    {
        _httpClient = httpClientFactory.CreateClient("VargshalaApi");
        _logger = logger;
    }

    public async Task<ApiResponse<PagedResponse<SubscriptionPlanDto>>> GetPlansPagedAsync(
        PagedRequest? request = null,
        bool? isActive = null,
        BillingCycle? billingCycle = null,
        CancellationToken cancellationToken = default)
    {
        var req = request ?? new PagedRequest();
        try
        {
            var queryParams = new List<string>
            {
                $"PageNumber={req.PageNumber}",
                $"PageSize={req.PageSize}"
            };

            if (!string.IsNullOrWhiteSpace(req.Search))
                queryParams.Add($"Search={Uri.EscapeDataString(req.Search)}");

            if (!string.IsNullOrWhiteSpace(req.SortBy))
                queryParams.Add($"SortBy={Uri.EscapeDataString(req.SortBy)}");

            if (!string.IsNullOrWhiteSpace(req.SortDirection))
                queryParams.Add($"SortDirection={Uri.EscapeDataString(req.SortDirection)}");

            if (isActive.HasValue)
                queryParams.Add($"isActive={isActive.Value}");

            if (billingCycle.HasValue)
                queryParams.Add($"billingCycle={(int)billingCycle.Value}");

            var url = "api/v1/subscription-plans?" + string.Join("&", queryParams);
            var response = await _httpClient.GetFromJsonAsync<ApiResponse<PagedResponse<SubscriptionPlanDto>>>(url, cancellationToken);
            if (response != null)
            {
                return response;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching subscription plans");
            return ApiResponse<PagedResponse<SubscriptionPlanDto>>.FailureResponse(ex.Message);
        }

        return ApiResponse<PagedResponse<SubscriptionPlanDto>>.FailureResponse("Failed to fetch subscription plans");
    }

    public async Task<ApiResponse<List<SubscriptionPlanDto>>> GetActivePlansAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.GetFromJsonAsync<ApiResponse<List<SubscriptionPlanDto>>>("api/v1/subscription-plans/active", cancellationToken);
            if (response != null)
            {
                return response;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching active subscription plans");
            return ApiResponse<List<SubscriptionPlanDto>>.FailureResponse(ex.Message);
        }

        return ApiResponse<List<SubscriptionPlanDto>>.FailureResponse("Failed to fetch active subscription plans");
    }

    public async Task<ApiResponse<SubscriptionPlanDto>> GetPlanByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.GetFromJsonAsync<ApiResponse<SubscriptionPlanDto>>($"api/v1/subscription-plans/{id}", cancellationToken);
            if (response != null)
            {
                return response;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching plan by ID {Id}", id);
            return ApiResponse<SubscriptionPlanDto>.FailureResponse(ex.Message);
        }

        return ApiResponse<SubscriptionPlanDto>.FailureResponse("Failed to fetch subscription plan");
    }

    public async Task<ApiResponse<SubscriptionPlanDto>> CreatePlanAsync(CreateSubscriptionPlanRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var res = await _httpClient.PostAsJsonAsync("api/v1/subscription-plans", request, cancellationToken);
            var response = await res.Content.ReadFromJsonAsync<ApiResponse<SubscriptionPlanDto>>(cancellationToken: cancellationToken);
            if (response != null)
            {
                return response;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating subscription plan");
            return ApiResponse<SubscriptionPlanDto>.FailureResponse(ex.Message);
        }

        return ApiResponse<SubscriptionPlanDto>.FailureResponse("Failed to create subscription plan");
    }

    public async Task<ApiResponse<SubscriptionPlanDto>> UpdatePlanAsync(Guid id, UpdateSubscriptionPlanRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var res = await _httpClient.PutAsJsonAsync($"api/v1/subscription-plans/{id}", request, cancellationToken);
            var response = await res.Content.ReadFromJsonAsync<ApiResponse<SubscriptionPlanDto>>(cancellationToken: cancellationToken);
            if (response != null)
            {
                return response;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating subscription plan {Id}", id);
            return ApiResponse<SubscriptionPlanDto>.FailureResponse(ex.Message);
        }

        return ApiResponse<SubscriptionPlanDto>.FailureResponse("Failed to update subscription plan");
    }

    public async Task<ApiResponse<SubscriptionPlanDto>> TogglePlanStatusAsync(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            var res = await _httpClient.PatchAsync($"api/v1/subscription-plans/{id}/toggle-status", null, cancellationToken);
            var response = await res.Content.ReadFromJsonAsync<ApiResponse<SubscriptionPlanDto>>(cancellationToken: cancellationToken);
            if (response != null)
            {
                return response;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error toggling subscription plan status {Id}", id);
            return ApiResponse<SubscriptionPlanDto>.FailureResponse(ex.Message);
        }

        return ApiResponse<SubscriptionPlanDto>.FailureResponse("Failed to toggle subscription plan status");
    }

    public async Task<ApiResponse<bool>> DeletePlanAsync(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            var res = await _httpClient.DeleteAsync($"api/v1/subscription-plans/{id}", cancellationToken);
            var response = await res.Content.ReadFromJsonAsync<ApiResponse<bool>>(cancellationToken: cancellationToken);
            if (response != null)
            {
                return response;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting subscription plan {Id}", id);
            return ApiResponse<bool>.FailureResponse(ex.Message);
        }

        return ApiResponse<bool>.FailureResponse("Failed to delete subscription plan");
    }
}
