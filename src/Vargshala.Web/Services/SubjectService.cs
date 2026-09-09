using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using Vargshala.Contracts.Common;
using Vargshala.Contracts.Subjects;

namespace Vargshala.Web.Services;

public class SubjectService : ISubjectService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<SubjectService> _logger;

    public SubjectService(
        IHttpClientFactory httpClientFactory,
        ILogger<SubjectService> logger)
    {
        _httpClient = httpClientFactory.CreateClient("VargshalaApi");
        _logger = logger;
    }

    public async Task<ApiResponse<PagedResponse<SubjectDto>>> GetSubjectsPagedAsync(
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

            var response = await _httpClient.GetAsync($"api/v1/subjects{queryParams}", cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                var errorResponse = await response.Content.ReadFromJsonAsync<ApiResponse<PagedResponse<SubjectDto>>>(cancellationToken: cancellationToken);
                return errorResponse ?? ApiResponse<PagedResponse<SubjectDto>>.FailureResponse("Failed to fetch subjects.");
            }

            var result = await response.Content.ReadFromJsonAsync<ApiResponse<PagedResponse<SubjectDto>>>(cancellationToken: cancellationToken);
            return result ?? ApiResponse<PagedResponse<SubjectDto>>.FailureResponse("Empty response from server.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching subjects");
            return ApiResponse<PagedResponse<SubjectDto>>.FailureResponse($"Network or server error: {ex.Message}");
        }
    }

    public async Task<ApiResponse<List<SubjectDto>>> GetAllActiveSubjectsAsync(
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.GetAsync("api/v1/subjects/all-active", cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadFromJsonAsync<ApiResponse<List<SubjectDto>>>(cancellationToken: cancellationToken);
                return error ?? ApiResponse<List<SubjectDto>>.FailureResponse("Failed to fetch active subjects.");
            }

            var result = await response.Content.ReadFromJsonAsync<ApiResponse<List<SubjectDto>>>(cancellationToken: cancellationToken);
            return result ?? ApiResponse<List<SubjectDto>>.FailureResponse("Empty response.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching all active subjects");
            return ApiResponse<List<SubjectDto>>.FailureResponse(ex.Message);
        }
    }

    public async Task<ApiResponse<SubjectDto>> GetSubjectByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.GetAsync($"api/v1/subjects/{id}", cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadFromJsonAsync<ApiResponse<SubjectDto>>(cancellationToken: cancellationToken);
                return error ?? ApiResponse<SubjectDto>.FailureResponse("Subject not found.");
            }

            var result = await response.Content.ReadFromJsonAsync<ApiResponse<SubjectDto>>(cancellationToken: cancellationToken);
            return result ?? ApiResponse<SubjectDto>.FailureResponse("Empty response.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching subject by ID {Id}", id);
            return ApiResponse<SubjectDto>.FailureResponse(ex.Message);
        }
    }

    public async Task<ApiResponse<SubjectDto>> CreateSubjectAsync(
        CreateSubjectRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/v1/subjects", request, cancellationToken);
            var result = await response.Content.ReadFromJsonAsync<ApiResponse<SubjectDto>>(cancellationToken: cancellationToken);
            return result ?? ApiResponse<SubjectDto>.FailureResponse("Failed to create subject.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating subject");
            return ApiResponse<SubjectDto>.FailureResponse(ex.Message);
        }
    }

    public async Task<ApiResponse<SubjectDto>> UpdateSubjectAsync(
        UpdateSubjectRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.PutAsJsonAsync($"api/v1/subjects/{request.Id}", request, cancellationToken);
            var result = await response.Content.ReadFromJsonAsync<ApiResponse<SubjectDto>>(cancellationToken: cancellationToken);
            return result ?? ApiResponse<SubjectDto>.FailureResponse("Failed to update subject.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating subject {Id}", request.Id);
            return ApiResponse<SubjectDto>.FailureResponse(ex.Message);
        }
    }

    public async Task<ApiResponse<bool>> DeleteSubjectAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.DeleteAsync($"api/v1/subjects/{id}", cancellationToken);
            var result = await response.Content.ReadFromJsonAsync<ApiResponse<bool>>(cancellationToken: cancellationToken);
            return result ?? ApiResponse<bool>.FailureResponse("Failed to delete subject.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting subject {Id}", id);
            return ApiResponse<bool>.FailureResponse(ex.Message);
        }
    }

    public async Task<ApiResponse<bool>> ToggleSubjectStatusAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.PatchAsync($"api/v1/subjects/{id}/toggle-status", null, cancellationToken);
            var result = await response.Content.ReadFromJsonAsync<ApiResponse<bool>>(cancellationToken: cancellationToken);
            return result ?? ApiResponse<bool>.FailureResponse("Failed to toggle subject status.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error toggling status for subject {Id}", id);
            return ApiResponse<bool>.FailureResponse(ex.Message);
        }
    }
}
