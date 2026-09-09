using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using Vargshala.Contracts.Classes;
using Vargshala.Contracts.Common;

namespace Vargshala.Web.Services;

public class ClassService : IClassService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ClassService> _logger;

    public ClassService(
        IHttpClientFactory httpClientFactory,
        ILogger<ClassService> logger)
    {
        _httpClient = httpClientFactory.CreateClient("VargshalaApi");
        _logger = logger;
    }

    public async Task<ApiResponse<PagedResponse<ClassDto>>> GetClassesPagedAsync(
        PagedRequest? request = null,
        Guid? branchId = null,
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
            if (branchId.HasValue)
                queryParams += $"&branchId={branchId.Value}";
            if (isActive.HasValue)
                queryParams += $"&isActive={isActive.Value.ToString().ToLowerInvariant()}";

            var response = await _httpClient.GetAsync($"api/v1/orgadmin/classes{queryParams}", cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                var errorResponse = await response.Content.ReadFromJsonAsync<ApiResponse<PagedResponse<ClassDto>>>(cancellationToken: cancellationToken);
                return errorResponse ?? ApiResponse<PagedResponse<ClassDto>>.FailureResponse("Failed to retrieve classes.");
            }

            var result = await response.Content.ReadFromJsonAsync<ApiResponse<PagedResponse<ClassDto>>>(cancellationToken: cancellationToken);
            return result ?? ApiResponse<PagedResponse<ClassDto>>.FailureResponse("Received empty response.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching paged classes");
            return ApiResponse<PagedResponse<ClassDto>>.FailureResponse(ex.Message);
        }
    }

    public async Task<ApiResponse<List<ClassLookupDto>>> GetAllActiveClassesAsync(
        Guid? branchId = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var url = "api/v1/orgadmin/classes/all-active";
            if (branchId.HasValue)
                url += $"?branchId={branchId.Value}";

            var response = await _httpClient.GetAsync(url, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return ApiResponse<List<ClassLookupDto>>.FailureResponse("Failed to retrieve active classes.");
            }

            var result = await response.Content.ReadFromJsonAsync<ApiResponse<List<ClassLookupDto>>>(cancellationToken: cancellationToken);
            return result ?? ApiResponse<List<ClassLookupDto>>.FailureResponse("Received empty response.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching active classes");
            return ApiResponse<List<ClassLookupDto>>.FailureResponse(ex.Message);
        }
    }

    public async Task<ApiResponse<ClassDto>> GetClassByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.GetAsync($"api/v1/orgadmin/classes/{id}", cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadFromJsonAsync<ApiResponse<ClassDto>>(cancellationToken: cancellationToken);
                return error ?? ApiResponse<ClassDto>.FailureResponse("Class not found.");
            }

            var result = await response.Content.ReadFromJsonAsync<ApiResponse<ClassDto>>(cancellationToken: cancellationToken);
            return result ?? ApiResponse<ClassDto>.FailureResponse("Received empty response.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching class by id {ClassId}", id);
            return ApiResponse<ClassDto>.FailureResponse(ex.Message);
        }
    }

    public async Task<ApiResponse<ClassDto>> CreateClassAsync(
        CreateClassRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/v1/orgadmin/classes", request, cancellationToken);
            var result = await response.Content.ReadFromJsonAsync<ApiResponse<ClassDto>>(cancellationToken: cancellationToken);
            return result ?? ApiResponse<ClassDto>.FailureResponse("Failed to create class.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating class");
            return ApiResponse<ClassDto>.FailureResponse(ex.Message);
        }
    }

    public async Task<ApiResponse<ClassDto>> UpdateClassAsync(
        UpdateClassRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.PutAsJsonAsync($"api/v1/orgadmin/classes/{request.Id}", request, cancellationToken);
            var result = await response.Content.ReadFromJsonAsync<ApiResponse<ClassDto>>(cancellationToken: cancellationToken);
            return result ?? ApiResponse<ClassDto>.FailureResponse("Failed to update class.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating class {ClassId}", request.Id);
            return ApiResponse<ClassDto>.FailureResponse(ex.Message);
        }
    }

    public async Task<ApiResponse<bool>> DeleteClassAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.DeleteAsync($"api/v1/orgadmin/classes/{id}", cancellationToken);
            var result = await response.Content.ReadFromJsonAsync<ApiResponse<bool>>(cancellationToken: cancellationToken);
            return result ?? ApiResponse<bool>.FailureResponse("Failed to delete class.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting class {ClassId}", id);
            return ApiResponse<bool>.FailureResponse(ex.Message);
        }
    }

    public async Task<ApiResponse<bool>> ToggleClassStatusAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.PatchAsync($"api/v1/orgadmin/classes/{id}/toggle-status", null, cancellationToken);
            var result = await response.Content.ReadFromJsonAsync<ApiResponse<bool>>(cancellationToken: cancellationToken);
            return result ?? ApiResponse<bool>.FailureResponse("Failed to toggle class status.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error toggling status for class {ClassId}", id);
            return ApiResponse<bool>.FailureResponse(ex.Message);
        }
    }
}
