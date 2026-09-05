using System.Net.Http.Json;
using Vargshala.Contracts.Common;
using Vargshala.Contracts.Teachers;

namespace Vargshala.Web.Services;

public class TeacherService : ITeacherService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<TeacherService> _logger;

    public TeacherService(
        IHttpClientFactory httpClientFactory,
        ILogger<TeacherService> logger)
    {
        _httpClient = httpClientFactory.CreateClient("VargshalaApi");
        _logger = logger;
    }

    public async Task<ApiResponse<PagedResponse<TeacherDto>>> GetTeachersPagedAsync(
        PagedRequest? request = null,
        string? department = null,
        string? designation = null,
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
            if (!string.IsNullOrWhiteSpace(department))
                queryParams += $"&department={Uri.EscapeDataString(department)}";
            if (!string.IsNullOrWhiteSpace(designation))
                queryParams += $"&designation={Uri.EscapeDataString(designation)}";
            if (isActive.HasValue)
                queryParams += $"&isActive={isActive.Value.ToString().ToLowerInvariant()}";

            var response = await _httpClient.GetAsync($"api/v1/orgadmin/teachers{queryParams}", cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var errorResponse = await response.Content.ReadFromJsonAsync<ApiResponse<PagedResponse<TeacherDto>>>(cancellationToken: cancellationToken);
                return errorResponse ?? ApiResponse<PagedResponse<TeacherDto>>.FailureResponse("Failed to fetch teachers.");
            }

            var result = await response.Content.ReadFromJsonAsync<ApiResponse<PagedResponse<TeacherDto>>>(cancellationToken: cancellationToken);
            return result ?? ApiResponse<PagedResponse<TeacherDto>>.FailureResponse("Invalid response from server.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching teachers.");
            return ApiResponse<PagedResponse<TeacherDto>>.FailureResponse($"Network or server error: {ex.Message}");
        }
    }

    public async Task<ApiResponse<TeacherDto>> GetTeacherByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.GetAsync($"api/v1/orgadmin/teachers/{id}", cancellationToken);
            var result = await response.Content.ReadFromJsonAsync<ApiResponse<TeacherDto>>(cancellationToken: cancellationToken);
            return result ?? ApiResponse<TeacherDto>.FailureResponse("Failed to fetch teacher details.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching teacher {Id}", id);
            return ApiResponse<TeacherDto>.FailureResponse($"Network or server error: {ex.Message}");
        }
    }

    public async Task<ApiResponse<TeacherDto>> CreateTeacherAsync(
        CreateTeacherRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/v1/orgadmin/teachers", request, cancellationToken);
            var result = await response.Content.ReadFromJsonAsync<ApiResponse<TeacherDto>>(cancellationToken: cancellationToken);
            return result ?? ApiResponse<TeacherDto>.FailureResponse("Failed to create teacher.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating teacher.");
            return ApiResponse<TeacherDto>.FailureResponse($"Network or server error: {ex.Message}");
        }
    }

    public async Task<ApiResponse<TeacherDto>> UpdateTeacherAsync(
        UpdateTeacherRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.PutAsJsonAsync($"api/v1/orgadmin/teachers/{request.Id}", request, cancellationToken);
            var result = await response.Content.ReadFromJsonAsync<ApiResponse<TeacherDto>>(cancellationToken: cancellationToken);
            return result ?? ApiResponse<TeacherDto>.FailureResponse("Failed to update teacher.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating teacher {Id}", request.Id);
            return ApiResponse<TeacherDto>.FailureResponse($"Network or server error: {ex.Message}");
        }
    }

    public async Task<ApiResponse<bool>> DeleteTeacherAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.DeleteAsync($"api/v1/orgadmin/teachers/{id}", cancellationToken);
            var result = await response.Content.ReadFromJsonAsync<ApiResponse<bool>>(cancellationToken: cancellationToken);
            return result ?? ApiResponse<bool>.FailureResponse("Failed to delete teacher.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting teacher {Id}", id);
            return ApiResponse<bool>.FailureResponse($"Network or server error: {ex.Message}");
        }
    }

    public async Task<ApiResponse<GeneratedTeacherCodeDto>?> GenerateTeacherCodeAsync(
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<ApiResponse<GeneratedTeacherCodeDto>>(
                "api/v1/orgadmin/teachers/generate-code",
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating teacher employee code");
            return ApiResponse<GeneratedTeacherCodeDto>.FailureResponse($"Network or server error: {ex.Message}");
        }
    }
}
