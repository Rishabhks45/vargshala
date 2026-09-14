using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using Vargshala.Contracts.Common;
using Vargshala.Contracts.FeeStructures;

namespace Vargshala.Web.Services.BranchAdmin;

public interface IBranchFeeStructureClientService
{
    Task<ApiResponse<PagedResponse<FeeStructureDto>>> GetFeeStructuresPagedAsync(
        PagedRequest? request = null,
        Guid? classId = null,
        string? session = null,
        bool? isActive = null,
        CancellationToken cancellationToken = default);

    Task<ApiResponse<List<FeeStructureLookupDto>>> GetAllActiveFeeStructuresAsync(
        Guid? classId = null,
        CancellationToken cancellationToken = default);

    Task<ApiResponse<FeeStructureDto>> GetFeeStructureByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<ApiResponse<FeeStructureDto>> CreateFeeStructureAsync(
        CreateFeeStructureRequest request,
        CancellationToken cancellationToken = default);

    Task<ApiResponse<FeeStructureDto>> UpdateFeeStructureAsync(
        Guid id,
        UpdateFeeStructureRequest request,
        CancellationToken cancellationToken = default);

    Task<ApiResponse<bool>> DeleteFeeStructureAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<ApiResponse<bool>> ToggleFeeStructureStatusAsync(
        Guid id,
        CancellationToken cancellationToken = default);
}

public class BranchFeeStructureClientService : IBranchFeeStructureClientService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<BranchFeeStructureClientService> _logger;

    public BranchFeeStructureClientService(
        IHttpClientFactory httpClientFactory,
        ILogger<BranchFeeStructureClientService> logger)
    {
        _httpClient = httpClientFactory.CreateClient("VargshalaApi");
        _logger = logger;
    }

    public async Task<ApiResponse<PagedResponse<FeeStructureDto>>> GetFeeStructuresPagedAsync(
        PagedRequest? request = null,
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
            if (classId.HasValue && classId.Value != Guid.Empty)
                queryParams += $"&classId={classId.Value}";
            if (!string.IsNullOrWhiteSpace(session))
                queryParams += $"&session={Uri.EscapeDataString(session)}";
            if (isActive.HasValue)
                queryParams += $"&isActive={isActive.Value.ToString().ToLowerInvariant()}";

            var response = await _httpClient.GetAsync($"api/v1/branchadmin/fee-structures{queryParams}", cancellationToken);
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
            _logger.LogError(ex, "Error fetching branch paged fee structures");
            return ApiResponse<PagedResponse<FeeStructureDto>>.FailureResponse(ex.Message);
        }
    }

    public async Task<ApiResponse<List<FeeStructureLookupDto>>> GetAllActiveFeeStructuresAsync(
        Guid? classId = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var url = "api/v1/branchadmin/fee-structures/all-active";
            if (classId.HasValue && classId.Value != Guid.Empty)
                url += $"?classId={classId.Value}";

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
            _logger.LogError(ex, "Error fetching branch active fee structures");
            return ApiResponse<List<FeeStructureLookupDto>>.FailureResponse(ex.Message);
        }
    }

    public async Task<ApiResponse<FeeStructureDto>> GetFeeStructureByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.GetAsync($"api/v1/branchadmin/fee-structures/{id}", cancellationToken);
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
            _logger.LogError(ex, "Error fetching branch fee structure by id {Id}", id);
            return ApiResponse<FeeStructureDto>.FailureResponse(ex.Message);
        }
    }

    public async Task<ApiResponse<FeeStructureDto>> CreateFeeStructureAsync(
        CreateFeeStructureRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/v1/branchadmin/fee-structures", request, cancellationToken);
            var result = await response.Content.ReadFromJsonAsync<ApiResponse<FeeStructureDto>>(cancellationToken: cancellationToken);
            return result ?? ApiResponse<FeeStructureDto>.FailureResponse("Failed to create fee structure.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating branch fee structure");
            return ApiResponse<FeeStructureDto>.FailureResponse(ex.Message);
        }
    }

    public async Task<ApiResponse<FeeStructureDto>> UpdateFeeStructureAsync(
        Guid id,
        UpdateFeeStructureRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.PutAsJsonAsync($"api/v1/branchadmin/fee-structures/{id}", request, cancellationToken);
            var result = await response.Content.ReadFromJsonAsync<ApiResponse<FeeStructureDto>>(cancellationToken: cancellationToken);
            return result ?? ApiResponse<FeeStructureDto>.FailureResponse("Failed to update fee structure.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating branch fee structure {Id}", id);
            return ApiResponse<FeeStructureDto>.FailureResponse(ex.Message);
        }
    }

    public async Task<ApiResponse<bool>> DeleteFeeStructureAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.DeleteAsync($"api/v1/branchadmin/fee-structures/{id}", cancellationToken);
            var result = await response.Content.ReadFromJsonAsync<ApiResponse<bool>>(cancellationToken: cancellationToken);
            return result ?? ApiResponse<bool>.FailureResponse("Failed to delete fee structure.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting branch fee structure {Id}", id);
            return ApiResponse<bool>.FailureResponse(ex.Message);
        }
    }

    public async Task<ApiResponse<bool>> ToggleFeeStructureStatusAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.PatchAsync($"api/v1/branchadmin/fee-structures/{id}/toggle-status", null, cancellationToken);
            var result = await response.Content.ReadFromJsonAsync<ApiResponse<bool>>(cancellationToken: cancellationToken);
            return result ?? ApiResponse<bool>.FailureResponse("Failed to toggle fee structure status.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error toggling status for branch fee structure {Id}", id);
            return ApiResponse<bool>.FailureResponse(ex.Message);
        }
    }
}
