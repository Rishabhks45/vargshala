using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using Vargshala.Contracts.Classes;
using Vargshala.Contracts.Common;

namespace Vargshala.Web.Services.BranchAdmin;

public interface IBranchClassClientService
{
    Task<ApiResponse<PagedResponse<ClassDto>>> GetClassesPagedAsync(PagedRequest? request = null, bool? isActive = null, CancellationToken cancellationToken = default);
    Task<ApiResponse<List<ClassLookupDto>>> GetAllActiveClassesAsync(CancellationToken cancellationToken = default);
    Task<ApiResponse<ClassDto>> GetClassByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ApiResponse<ClassDto>> CreateClassAsync(CreateClassRequest request, CancellationToken cancellationToken = default);
    Task<ApiResponse<ClassDto>> UpdateClassAsync(Guid id, UpdateClassRequest request, CancellationToken cancellationToken = default);
    Task<ApiResponse<bool>> DeleteClassAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ApiResponse<bool>> ToggleClassStatusAsync(Guid id, CancellationToken cancellationToken = default);
}

public class BranchClassClientService : IBranchClassClientService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<BranchClassClientService> _logger;

    public BranchClassClientService(IHttpClientFactory httpClientFactory, ILogger<BranchClassClientService> logger)
    {
        _httpClient = httpClientFactory.CreateClient("VargshalaApi");
        _logger = logger;
    }

    public async Task<ApiResponse<PagedResponse<ClassDto>>> GetClassesPagedAsync(
        PagedRequest? request = null,
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
            if (isActive.HasValue)
                queryParams += $"&isActive={isActive.Value.ToString().ToLowerInvariant()}";

            var response = await _httpClient.GetAsync($"api/v1/branchadmin/classes{queryParams}", cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadFromJsonAsync<ApiResponse<PagedResponse<ClassDto>>>(cancellationToken: cancellationToken);
                return error ?? ApiResponse<PagedResponse<ClassDto>>.FailureResponse("Failed to retrieve classes.");
            }

            var result = await response.Content.ReadFromJsonAsync<ApiResponse<PagedResponse<ClassDto>>>(cancellationToken: cancellationToken);
            return result ?? ApiResponse<PagedResponse<ClassDto>>.FailureResponse("Received empty response.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching branch classes");
            return ApiResponse<PagedResponse<ClassDto>>.FailureResponse(ex.Message);
        }
    }

    public async Task<ApiResponse<List<ClassLookupDto>>> GetAllActiveClassesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.GetAsync("api/v1/branchadmin/classes/all-active", cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadFromJsonAsync<ApiResponse<List<ClassLookupDto>>>(cancellationToken: cancellationToken);
                return error ?? ApiResponse<List<ClassLookupDto>>.FailureResponse("Failed to retrieve classes lookup.");
            }

            var result = await response.Content.ReadFromJsonAsync<ApiResponse<List<ClassLookupDto>>>(cancellationToken: cancellationToken);
            return result ?? ApiResponse<List<ClassLookupDto>>.FailureResponse("Received empty response.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching active branch classes");
            return ApiResponse<List<ClassLookupDto>>.FailureResponse(ex.Message);
        }
    }

    public async Task<ApiResponse<ClassDto>> GetClassByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.GetAsync($"api/v1/branchadmin/classes/{id}", cancellationToken);
            var result = await response.Content.ReadFromJsonAsync<ApiResponse<ClassDto>>(cancellationToken: cancellationToken);
            return result ?? ApiResponse<ClassDto>.FailureResponse("Failed to retrieve class.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching branch class by id");
            return ApiResponse<ClassDto>.FailureResponse(ex.Message);
        }
    }

    public async Task<ApiResponse<ClassDto>> CreateClassAsync(CreateClassRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/v1/branchadmin/classes", request, cancellationToken);
            var result = await response.Content.ReadFromJsonAsync<ApiResponse<ClassDto>>(cancellationToken: cancellationToken);
            return result ?? ApiResponse<ClassDto>.FailureResponse("Failed to create class.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating branch class");
            return ApiResponse<ClassDto>.FailureResponse(ex.Message);
        }
    }

    public async Task<ApiResponse<ClassDto>> UpdateClassAsync(Guid id, UpdateClassRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.PutAsJsonAsync($"api/v1/branchadmin/classes/{id}", request, cancellationToken);
            var result = await response.Content.ReadFromJsonAsync<ApiResponse<ClassDto>>(cancellationToken: cancellationToken);
            return result ?? ApiResponse<ClassDto>.FailureResponse("Failed to update class.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating branch class");
            return ApiResponse<ClassDto>.FailureResponse(ex.Message);
        }
    }

    public async Task<ApiResponse<bool>> DeleteClassAsync(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.DeleteAsync($"api/v1/branchadmin/classes/{id}", cancellationToken);
            var result = await response.Content.ReadFromJsonAsync<ApiResponse<bool>>(cancellationToken: cancellationToken);
            return result ?? ApiResponse<bool>.FailureResponse("Failed to delete class.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting branch class");
            return ApiResponse<bool>.FailureResponse(ex.Message);
        }
    }

    public async Task<ApiResponse<bool>> ToggleClassStatusAsync(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.PatchAsync($"api/v1/branchadmin/classes/{id}/toggle-status", null, cancellationToken);
            var result = await response.Content.ReadFromJsonAsync<ApiResponse<bool>>(cancellationToken: cancellationToken);
            return result ?? ApiResponse<bool>.FailureResponse("Failed to toggle class status.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error toggling branch class status");
            return ApiResponse<bool>.FailureResponse(ex.Message);
        }
    }
}
