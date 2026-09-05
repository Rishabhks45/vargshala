using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using Vargshala.Contracts.Common;
using Vargshala.Contracts.Coupons;

namespace Vargshala.Web.Services;

public class CouponService : ICouponService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<CouponService> _logger;

    public CouponService(
        IHttpClientFactory httpClientFactory,
        ILogger<CouponService> logger)
    {
        _httpClient = httpClientFactory.CreateClient("VargshalaApi");
        _logger = logger;
    }

    public async Task<ApiResponse<PagedResponse<CouponDto>>> GetCouponsPagedAsync(
        PagedRequest? request = null,
        CampaignCategory? category = null,
        DiscountType? discountType = null,
        ApplicablePlan? plan = null,
        bool? isActive = null,
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

            if (category.HasValue) queryParams.Add($"category={(int)category.Value}");
            if (discountType.HasValue) queryParams.Add($"discountType={(int)discountType.Value}");
            if (plan.HasValue) queryParams.Add($"plan={(int)plan.Value}");
            if (isActive.HasValue) queryParams.Add($"isActive={isActive.Value}");

            var url = "api/v1/coupons?" + string.Join("&", queryParams);

            var response = await _httpClient.GetFromJsonAsync<ApiResponse<PagedResponse<CouponDto>>>(url, cancellationToken);
            if (response != null)
            {
                return response;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load coupons paged from API.");
            return ApiResponse<PagedResponse<CouponDto>>.FailureResponse($"Failed to load coupons: {ex.Message}");
        }

        return ApiResponse<PagedResponse<CouponDto>>.SuccessResponse(new PagedResponse<CouponDto> { PageNumber = req.PageNumber, PageSize = req.PageSize });
    }

    public async Task<CouponDto?> GetCouponByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.GetFromJsonAsync<ApiResponse<CouponDto>>($"api/v1/coupons/{id}", cancellationToken);
            if (response != null && response.Success && response.Data != null)
            {
                return response.Data;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load coupon {Id} from API.", id);
        }

        return null;
    }

    public async Task<ApiResponse<Guid>> CreateCouponAsync(CreateCouponRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/v1/coupons", request, cancellationToken);
            var result = await response.Content.ReadFromJsonAsync<ApiResponse<Guid>>(cancellationToken);
            if (result != null) return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create coupon via API.");
            return ApiResponse<Guid>.FailureResponse($"API error: {ex.Message}");
        }

        return ApiResponse<Guid>.FailureResponse("Failed to create coupon.");
    }

    public async Task<ApiResponse<bool>> UpdateCouponAsync(UpdateCouponRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.PutAsJsonAsync($"api/v1/coupons/{request.Id}", request, cancellationToken);
            var result = await response.Content.ReadFromJsonAsync<ApiResponse<bool>>(cancellationToken);
            if (result != null) return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update coupon via API.");
            return ApiResponse<bool>.FailureResponse($"API error: {ex.Message}");
        }

        return ApiResponse<bool>.FailureResponse("Failed to update coupon.");
    }

    public async Task<ApiResponse<bool>> ToggleStatusAsync(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.PatchAsync($"api/v1/coupons/{id}/toggle-status", null, cancellationToken);
            var result = await response.Content.ReadFromJsonAsync<ApiResponse<bool>>(cancellationToken);
            if (result != null) return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to toggle coupon status via API.");
            return ApiResponse<bool>.FailureResponse($"API error: {ex.Message}");
        }

        return ApiResponse<bool>.FailureResponse("Failed to toggle coupon status.");
    }

    public async Task<ApiResponse<bool>> DeleteCouponAsync(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.DeleteAsync($"api/v1/coupons/{id}", cancellationToken);
            var result = await response.Content.ReadFromJsonAsync<ApiResponse<bool>>(cancellationToken);
            if (result != null) return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete coupon via API.");
            return ApiResponse<bool>.FailureResponse($"API error: {ex.Message}");
        }

        return ApiResponse<bool>.FailureResponse("Failed to delete coupon.");
    }
}
