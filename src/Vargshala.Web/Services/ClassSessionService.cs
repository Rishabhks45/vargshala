using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using Vargshala.Contracts.ClassSessions;
using Vargshala.Contracts.Common;

namespace Vargshala.Web.Services;

public class ClassSessionService : IClassSessionService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ClassSessionService> _logger;

    public ClassSessionService(
        IHttpClientFactory httpClientFactory,
        ILogger<ClassSessionService> logger)
    {
        _httpClient = httpClientFactory.CreateClient("VargshalaApi");
        _logger = logger;
    }

    public async Task<ApiResponse<PagedResponse<ClassSessionDto>>> GetClassSessionsPagedAsync(
        PagedRequest? request = null,
        Guid? branchId = null,
        Guid? classId = null,
        Guid? batchId = null,
        Guid? teacherId = null,
        DateOnly? fromDate = null,
        DateOnly? toDate = null,
        string? status = null,
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
            if (batchId.HasValue && batchId.Value != Guid.Empty)
                queryParams += $"&batchId={batchId.Value}";
            if (teacherId.HasValue && teacherId.Value != Guid.Empty)
                queryParams += $"&teacherId={teacherId.Value}";
            if (fromDate.HasValue)
                queryParams += $"&fromDate={fromDate.Value:yyyy-MM-dd}";
            if (toDate.HasValue)
                queryParams += $"&toDate={toDate.Value:yyyy-MM-dd}";
            if (!string.IsNullOrWhiteSpace(status))
                queryParams += $"&status={Uri.EscapeDataString(status)}";

            var response = await _httpClient.GetAsync($"api/v1/orgadmin/class-sessions{queryParams}", cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                var err = await response.Content.ReadFromJsonAsync<ApiResponse<PagedResponse<ClassSessionDto>>>(cancellationToken: cancellationToken);
                return err ?? ApiResponse<PagedResponse<ClassSessionDto>>.FailureResponse("Failed to retrieve class sessions.");
            }

            var result = await response.Content.ReadFromJsonAsync<ApiResponse<PagedResponse<ClassSessionDto>>>(cancellationToken: cancellationToken);
            return result ?? ApiResponse<PagedResponse<ClassSessionDto>>.FailureResponse("Received empty response.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching paged class sessions");
            return ApiResponse<PagedResponse<ClassSessionDto>>.FailureResponse(ex.Message);
        }
    }

    public async Task<ApiResponse<ClassSessionDto>> GetClassSessionByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.GetAsync($"api/v1/orgadmin/class-sessions/{id}", cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                var err = await response.Content.ReadFromJsonAsync<ApiResponse<ClassSessionDto>>(cancellationToken: cancellationToken);
                return err ?? ApiResponse<ClassSessionDto>.FailureResponse("Class session not found.");
            }

            var result = await response.Content.ReadFromJsonAsync<ApiResponse<ClassSessionDto>>(cancellationToken: cancellationToken);
            return result ?? ApiResponse<ClassSessionDto>.FailureResponse("Received empty response.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching class session {SessionId}", id);
            return ApiResponse<ClassSessionDto>.FailureResponse(ex.Message);
        }
    }

    public async Task<ApiResponse<ClassSessionDto>> CreateClassSessionAsync(
        CreateClassSessionRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/v1/orgadmin/class-sessions", request, cancellationToken);
            var result = await response.Content.ReadFromJsonAsync<ApiResponse<ClassSessionDto>>(cancellationToken: cancellationToken);
            return result ?? ApiResponse<ClassSessionDto>.FailureResponse("Failed to create class session.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating class session");
            return ApiResponse<ClassSessionDto>.FailureResponse(ex.Message);
        }
    }

    public async Task<ApiResponse<ClassSessionDto>> UpdateClassSessionAsync(
        Guid id,
        UpdateClassSessionRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.PutAsJsonAsync($"api/v1/orgadmin/class-sessions/{id}", request, cancellationToken);
            var result = await response.Content.ReadFromJsonAsync<ApiResponse<ClassSessionDto>>(cancellationToken: cancellationToken);
            return result ?? ApiResponse<ClassSessionDto>.FailureResponse("Failed to update class session.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating class session {SessionId}", id);
            return ApiResponse<ClassSessionDto>.FailureResponse(ex.Message);
        }
    }

    public async Task<ApiResponse<bool>> DeleteClassSessionAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.DeleteAsync($"api/v1/orgadmin/class-sessions/{id}", cancellationToken);
            var result = await response.Content.ReadFromJsonAsync<ApiResponse<bool>>(cancellationToken: cancellationToken);
            return result ?? ApiResponse<bool>.FailureResponse("Failed to delete class session.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting class session {SessionId}", id);
            return ApiResponse<bool>.FailureResponse(ex.Message);
        }
    }

    public async Task<ApiResponse<bool>> CancelClassSessionAsync(
        Guid id,
        CancelClassSessionRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.PatchAsJsonAsync($"api/v1/orgadmin/class-sessions/{id}/cancel", request, cancellationToken);
            var result = await response.Content.ReadFromJsonAsync<ApiResponse<bool>>(cancellationToken: cancellationToken);
            return result ?? ApiResponse<bool>.FailureResponse("Failed to cancel class session.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cancelling class session {SessionId}", id);
            return ApiResponse<bool>.FailureResponse(ex.Message);
        }
    }

    public async Task<ApiResponse<bool>> CompleteClassSessionAsync(
        Guid id,
        CompleteClassSessionRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.PatchAsJsonAsync($"api/v1/orgadmin/class-sessions/{id}/complete", request, cancellationToken);
            var result = await response.Content.ReadFromJsonAsync<ApiResponse<bool>>(cancellationToken: cancellationToken);
            return result ?? ApiResponse<bool>.FailureResponse("Failed to complete class session.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error completing class session {SessionId}", id);
            return ApiResponse<bool>.FailureResponse(ex.Message);
        }
    }

    public async Task<ApiResponse<int>> GenerateSessionsFromScheduleAsync(
        GenerateSessionsFromScheduleRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/v1/orgadmin/class-sessions/generate", request, cancellationToken);
            var result = await response.Content.ReadFromJsonAsync<ApiResponse<int>>(cancellationToken: cancellationToken);
            return result ?? ApiResponse<int>.FailureResponse("Failed to generate class sessions.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating class sessions from schedule");
            return ApiResponse<int>.FailureResponse(ex.Message);
        }
    }
}
