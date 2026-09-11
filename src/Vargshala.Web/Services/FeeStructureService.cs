using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using Vargshala.Contracts.Common;
using Vargshala.Contracts.FeeStructures;

namespace Vargshala.Web.Services;

public class FeeStructureService : IFeeStructureService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<FeeStructureService> _logger;

    public FeeStructureService(
        IHttpClientFactory httpClientFactory,
        ILogger<FeeStructureService> logger)
    {
        _httpClient = httpClientFactory.CreateClient("VargshalaApi");
        _logger = logger;
    }

    public async Task<ApiResponse<PagedResponse<FeeStructureDto>>> GetFeeStructuresPagedAsync(
        PagedRequest? request = null,
        Guid? branchId = null,
        Guid? classId = null,
        string? session = null,
        bool? isActive = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var req = request ?? new PagedRequest();
            var queryParams = $"?pageNumber={req.PageNumber}&pageSize={req.PageSize}&sortDirection={req.SortDirection}";
            if (!string.IsNullOrWhiteSpace(req.Search))
                queryParams += $"&search={Uri.EscapeDataString(req.Search)}";
            if (!string.IsNullOrWhiteSpace(req.SortBy))
                queryParams += $"&sortBy={Uri.EscapeDataString(req.SortBy)}";
            if (branchId.HasValue && branchId.Value != Guid.Empty)
                queryParams += $"&branchId={branchId.Value}";
            if (classId.HasValue && classId.Value != Guid.Empty)
                queryParams += $"&classId={classId.Value}";
            if (!string.IsNullOrWhiteSpace(session))
                queryParams += $"&session={Uri.EscapeDataString(session)}";
            if (isActive.HasValue)
                queryParams += $"&isActive={isActive.Value.ToString().ToLowerInvariant()}";

            var response = await _httpClient.GetAsync($"api/v1/orgadmin/fee-structures{queryParams}", cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                var errorResponse = await response.Content.ReadFromJsonAsync<ApiResponse<PagedResponse<FeeStructureDto>>>(cancellationToken: cancellationToken);
                return errorResponse ?? ApiResponse<PagedResponse<FeeStructureDto>>.FailureResponse("Failed to retrieve fee structures.");
            }

            var result = await response.Content.ReadFromJsonAsync<ApiResponse<PagedResponse<FeeStructureDto>>>(cancellationToken: cancellationToken);
            return result ?? ApiResponse<PagedResponse<FeeStructureDto>>.FailureResponse("Received empty response.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching paged fee structures");
            return ApiResponse<PagedResponse<FeeStructureDto>>.FailureResponse(ex.Message);
        }
    }

    public async Task<ApiResponse<List<FeeStructureLookupDto>>> GetAllActiveFeeStructuresAsync(
        Guid? branchId = null,
        Guid? classId = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var url = "api/v1/orgadmin/fee-structures/all-active";
            var queryParams = new List<string>();
            if (branchId.HasValue && branchId.Value != Guid.Empty)
                queryParams.Add($"branchId={branchId.Value}");
            if (classId.HasValue && classId.Value != Guid.Empty)
                queryParams.Add($"classId={classId.Value}");

            if (queryParams.Any())
                url += "?" + string.Join("&", queryParams);

            var response = await _httpClient.GetAsync(url, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return ApiResponse<List<FeeStructureLookupDto>>.FailureResponse("Failed to retrieve active fee structures.");
            }

            var result = await response.Content.ReadFromJsonAsync<ApiResponse<List<FeeStructureLookupDto>>>(cancellationToken: cancellationToken);
            return result ?? ApiResponse<List<FeeStructureLookupDto>>.FailureResponse("Received empty response.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching active fee structures");
            return ApiResponse<List<FeeStructureLookupDto>>.FailureResponse(ex.Message);
        }
    }

    public async Task<ApiResponse<FeeStructureDto>> GetFeeStructureByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.GetAsync($"api/v1/orgadmin/fee-structures/{id}", cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadFromJsonAsync<ApiResponse<FeeStructureDto>>(cancellationToken: cancellationToken);
                return error ?? ApiResponse<FeeStructureDto>.FailureResponse("Fee structure not found.");
            }

            var result = await response.Content.ReadFromJsonAsync<ApiResponse<FeeStructureDto>>(cancellationToken: cancellationToken);
            return result ?? ApiResponse<FeeStructureDto>.FailureResponse("Received empty response.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching fee structure by id {Id}", id);
            return ApiResponse<FeeStructureDto>.FailureResponse(ex.Message);
        }
    }

    public async Task<ApiResponse<FeeStructureDto>> CreateFeeStructureAsync(
        CreateFeeStructureRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/v1/orgadmin/fee-structures", request, cancellationToken);
            var result = await response.Content.ReadFromJsonAsync<ApiResponse<FeeStructureDto>>(cancellationToken: cancellationToken);
            return result ?? ApiResponse<FeeStructureDto>.FailureResponse("Failed to create fee structure.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating fee structure");
            return ApiResponse<FeeStructureDto>.FailureResponse(ex.Message);
        }
    }

    public async Task<ApiResponse<FeeStructureDto>> UpdateFeeStructureAsync(
        UpdateFeeStructureRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.PutAsJsonAsync($"api/v1/orgadmin/fee-structures/{request.Id}", request, cancellationToken);
            var result = await response.Content.ReadFromJsonAsync<ApiResponse<FeeStructureDto>>(cancellationToken: cancellationToken);
            return result ?? ApiResponse<FeeStructureDto>.FailureResponse("Failed to update fee structure.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating fee structure {Id}", request.Id);
            return ApiResponse<FeeStructureDto>.FailureResponse(ex.Message);
        }
    }

    public async Task<ApiResponse<bool>> DeleteFeeStructureAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.DeleteAsync($"api/v1/orgadmin/fee-structures/{id}", cancellationToken);
            var result = await response.Content.ReadFromJsonAsync<ApiResponse<bool>>(cancellationToken: cancellationToken);
            return result ?? ApiResponse<bool>.FailureResponse("Failed to delete fee structure.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting fee structure {Id}", id);
            return ApiResponse<bool>.FailureResponse(ex.Message);
        }
    }

    public async Task<ApiResponse<bool>> ToggleFeeStructureStatusAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.PatchAsync($"api/v1/orgadmin/fee-structures/{id}/toggle-status", null, cancellationToken);
            var result = await response.Content.ReadFromJsonAsync<ApiResponse<bool>>(cancellationToken: cancellationToken);
            return result ?? ApiResponse<bool>.FailureResponse("Failed to toggle fee structure status.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error toggling status for fee structure {Id}", id);
            return ApiResponse<bool>.FailureResponse(ex.Message);
        }
    }
}
