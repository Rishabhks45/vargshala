using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using Vargshala.Contracts.ClassSessions;
using Vargshala.Contracts.Common;

namespace Vargshala.Web.Services.BranchAdmin;

public interface IBranchClassSessionClientService
{
    Task<ApiResponse<PagedResponse<ClassSessionDto>>> GetClassSessionsPagedAsync(PagedRequest? request = null, Guid? classId = null, Guid? batchId = null, Guid? teacherId = null, DateOnly? fromDate = null, DateOnly? toDate = null, string? status = null, CancellationToken cancellationToken = default);
    Task<ApiResponse<ClassSessionDto>> GetClassSessionByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ApiResponse<ClassSessionDto>> CreateClassSessionAsync(CreateClassSessionRequest request, CancellationToken cancellationToken = default);
    Task<ApiResponse<ClassSessionDto>> UpdateClassSessionAsync(Guid id, UpdateClassSessionRequest request, CancellationToken cancellationToken = default);
    Task<ApiResponse<bool>> DeleteClassSessionAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ApiResponse<bool>> CancelClassSessionAsync(Guid id, CancelClassSessionRequest request, CancellationToken cancellationToken = default);
    Task<ApiResponse<bool>> CompleteClassSessionAsync(Guid id, CompleteClassSessionRequest request, CancellationToken cancellationToken = default);
    Task<ApiResponse<List<ClassSessionDto>>> GenerateSessionsFromScheduleAsync(GenerateSessionsFromScheduleRequest request, CancellationToken cancellationToken = default);
}

public class BranchClassSessionClientService : IBranchClassSessionClientService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<BranchClassSessionClientService> _logger;

    public BranchClassSessionClientService(IHttpClientFactory httpClientFactory, ILogger<BranchClassSessionClientService> logger)
    {
        _httpClient = httpClientFactory.CreateClient("VargshalaApi");
        _logger = logger;
    }

    public async Task<ApiResponse<PagedResponse<ClassSessionDto>>> GetClassSessionsPagedAsync(
        PagedRequest? request = null,
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
            if (classId.HasValue)
                queryParams += $"&classId={classId.Value}";
            if (batchId.HasValue)
                queryParams += $"&batchId={batchId.Value}";
            if (teacherId.HasValue)
                queryParams += $"&teacherId={teacherId.Value}";
            if (fromDate.HasValue)
                queryParams += $"&fromDate={fromDate.Value:yyyy-MM-dd}";
            if (toDate.HasValue)
                queryParams += $"&toDate={toDate.Value:yyyy-MM-dd}";
            if (!string.IsNullOrWhiteSpace(status))
                queryParams += $"&status={Uri.EscapeDataString(status)}";

            var response = await _httpClient.GetAsync($"api/v1/branchadmin/class-sessions{queryParams}", cancellationToken);
            var result = await response.Content.ReadFromJsonAsync<ApiResponse<PagedResponse<ClassSessionDto>>>(cancellationToken: cancellationToken);
            return result ?? ApiResponse<PagedResponse<ClassSessionDto>>.FailureResponse("Received empty response.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching branch class sessions");
            return ApiResponse<PagedResponse<ClassSessionDto>>.FailureResponse(ex.Message);
        }
    }

    public async Task<ApiResponse<ClassSessionDto>> GetClassSessionByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.GetAsync($"api/v1/branchadmin/class-sessions/{id}", cancellationToken);
            var result = await response.Content.ReadFromJsonAsync<ApiResponse<ClassSessionDto>>(cancellationToken: cancellationToken);
            return result ?? ApiResponse<ClassSessionDto>.FailureResponse("Failed to retrieve class session.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching class session by id");
            return ApiResponse<ClassSessionDto>.FailureResponse(ex.Message);
        }
    }

    public async Task<ApiResponse<ClassSessionDto>> CreateClassSessionAsync(CreateClassSessionRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/v1/branchadmin/class-sessions", request, cancellationToken);
            var result = await response.Content.ReadFromJsonAsync<ApiResponse<ClassSessionDto>>(cancellationToken: cancellationToken);
            return result ?? ApiResponse<ClassSessionDto>.FailureResponse("Failed to create class session.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating class session");
            return ApiResponse<ClassSessionDto>.FailureResponse(ex.Message);
        }
    }

    public async Task<ApiResponse<ClassSessionDto>> UpdateClassSessionAsync(Guid id, UpdateClassSessionRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.PutAsJsonAsync($"api/v1/branchadmin/class-sessions/{id}", request, cancellationToken);
            var result = await response.Content.ReadFromJsonAsync<ApiResponse<ClassSessionDto>>(cancellationToken: cancellationToken);
            return result ?? ApiResponse<ClassSessionDto>.FailureResponse("Failed to update class session.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating class session");
            return ApiResponse<ClassSessionDto>.FailureResponse(ex.Message);
        }
    }

    public async Task<ApiResponse<bool>> DeleteClassSessionAsync(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.DeleteAsync($"api/v1/branchadmin/class-sessions/{id}", cancellationToken);
            var result = await response.Content.ReadFromJsonAsync<ApiResponse<bool>>(cancellationToken: cancellationToken);
            return result ?? ApiResponse<bool>.FailureResponse("Failed to delete class session.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting class session");
            return ApiResponse<bool>.FailureResponse(ex.Message);
        }
    }

    public async Task<ApiResponse<bool>> CancelClassSessionAsync(Guid id, CancelClassSessionRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.PatchAsJsonAsync($"api/v1/branchadmin/class-sessions/{id}/cancel", request, cancellationToken);
            var result = await response.Content.ReadFromJsonAsync<ApiResponse<bool>>(cancellationToken: cancellationToken);
            return result ?? ApiResponse<bool>.FailureResponse("Failed to cancel class session.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cancelling class session");
            return ApiResponse<bool>.FailureResponse(ex.Message);
        }
    }

    public async Task<ApiResponse<bool>> CompleteClassSessionAsync(Guid id, CompleteClassSessionRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.PatchAsJsonAsync($"api/v1/branchadmin/class-sessions/{id}/complete", request, cancellationToken);
            var result = await response.Content.ReadFromJsonAsync<ApiResponse<bool>>(cancellationToken: cancellationToken);
            return result ?? ApiResponse<bool>.FailureResponse("Failed to complete class session.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error completing class session");
            return ApiResponse<bool>.FailureResponse(ex.Message);
        }
    }

    public async Task<ApiResponse<List<ClassSessionDto>>> GenerateSessionsFromScheduleAsync(GenerateSessionsFromScheduleRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/v1/branchadmin/class-sessions/generate-from-schedule", request, cancellationToken);
            var result = await response.Content.ReadFromJsonAsync<ApiResponse<List<ClassSessionDto>>>(cancellationToken: cancellationToken);
            return result ?? ApiResponse<List<ClassSessionDto>>.FailureResponse("Failed to generate sessions.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating sessions from schedule");
            return ApiResponse<List<ClassSessionDto>>.FailureResponse(ex.Message);
        }
    }
}
