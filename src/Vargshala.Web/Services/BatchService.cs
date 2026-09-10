using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using Vargshala.Contracts.Batches;
using Vargshala.Contracts.Common;

namespace Vargshala.Web.Services;

public class BatchService : IBatchService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<BatchService> _logger;

    public BatchService(
        IHttpClientFactory httpClientFactory,
        ILogger<BatchService> logger)
    {
        _httpClient = httpClientFactory.CreateClient("VargshalaApi");
        _logger = logger;
    }

    public async Task<ApiResponse<PagedResponse<BatchDto>>> GetBatchesPagedAsync(
        PagedRequest? request = null,
        Guid? branchId = null,
        Guid? classId = null,
        Guid? subjectId = null,
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
            if (classId.HasValue)
                queryParams += $"&classId={classId.Value}";
            if (subjectId.HasValue)
                queryParams += $"&subjectId={subjectId.Value}";
            if (isActive.HasValue)
                queryParams += $"&isActive={isActive.Value.ToString().ToLowerInvariant()}";

            var response = await _httpClient.GetAsync($"api/v1/orgadmin/batches{queryParams}", cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                var errorResponse = await response.Content.ReadFromJsonAsync<ApiResponse<PagedResponse<BatchDto>>>(cancellationToken: cancellationToken);
                return errorResponse ?? ApiResponse<PagedResponse<BatchDto>>.FailureResponse("Failed to retrieve batches.");
            }

            var result = await response.Content.ReadFromJsonAsync<ApiResponse<PagedResponse<BatchDto>>>(cancellationToken: cancellationToken);
            return result ?? ApiResponse<PagedResponse<BatchDto>>.FailureResponse("Received empty response.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching paged batches");
            return ApiResponse<PagedResponse<BatchDto>>.FailureResponse(ex.Message);
        }
    }

    public async Task<ApiResponse<List<BatchDto>>> GetAllActiveBatchesAsync(
        Guid? classId = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var url = "api/v1/orgadmin/batches/all-active";
            if (classId.HasValue)
                url += $"?classId={classId.Value}";

            var response = await _httpClient.GetAsync(url, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return ApiResponse<List<BatchDto>>.FailureResponse("Failed to retrieve active batches.");
            }

            var result = await response.Content.ReadFromJsonAsync<ApiResponse<List<BatchDto>>>(cancellationToken: cancellationToken);
            return result ?? ApiResponse<List<BatchDto>>.FailureResponse("Received empty response.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching active batches");
            return ApiResponse<List<BatchDto>>.FailureResponse(ex.Message);
        }
    }

    public async Task<ApiResponse<BatchDetailDto>> GetBatchByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.GetAsync($"api/v1/orgadmin/batches/{id}", cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadFromJsonAsync<ApiResponse<BatchDetailDto>>(cancellationToken: cancellationToken);
                return error ?? ApiResponse<BatchDetailDto>.FailureResponse("Batch not found.");
            }

            var result = await response.Content.ReadFromJsonAsync<ApiResponse<BatchDetailDto>>(cancellationToken: cancellationToken);
            return result ?? ApiResponse<BatchDetailDto>.FailureResponse("Received empty response.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching batch by id {BatchId}", id);
            return ApiResponse<BatchDetailDto>.FailureResponse(ex.Message);
        }
    }

    public async Task<ApiResponse<BatchDto>> CreateBatchAsync(
        CreateBatchRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/v1/orgadmin/batches", request, cancellationToken);
            var result = await response.Content.ReadFromJsonAsync<ApiResponse<BatchDto>>(cancellationToken: cancellationToken);
            return result ?? ApiResponse<BatchDto>.FailureResponse("Failed to create batch.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating batch");
            return ApiResponse<BatchDto>.FailureResponse(ex.Message);
        }
    }

    public async Task<ApiResponse<BatchDto>> UpdateBatchAsync(
        Guid id,
        UpdateBatchRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.PutAsJsonAsync($"api/v1/orgadmin/batches/{id}", request, cancellationToken);
            var result = await response.Content.ReadFromJsonAsync<ApiResponse<BatchDto>>(cancellationToken: cancellationToken);
            return result ?? ApiResponse<BatchDto>.FailureResponse("Failed to update batch.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating batch {BatchId}", id);
            return ApiResponse<BatchDto>.FailureResponse(ex.Message);
        }
    }

    public async Task<ApiResponse<bool>> DeleteBatchAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.DeleteAsync($"api/v1/orgadmin/batches/{id}", cancellationToken);
            var result = await response.Content.ReadFromJsonAsync<ApiResponse<bool>>(cancellationToken: cancellationToken);
            return result ?? ApiResponse<bool>.FailureResponse("Failed to delete batch.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting batch {BatchId}", id);
            return ApiResponse<bool>.FailureResponse(ex.Message);
        }
    }

    public async Task<ApiResponse<bool>> ToggleBatchStatusAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.PatchAsync($"api/v1/orgadmin/batches/{id}/toggle-status", null, cancellationToken);
            var result = await response.Content.ReadFromJsonAsync<ApiResponse<bool>>(cancellationToken: cancellationToken);
            return result ?? ApiResponse<bool>.FailureResponse("Failed to toggle batch status.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error toggling status for batch {BatchId}", id);
            return ApiResponse<bool>.FailureResponse(ex.Message);
        }
    }

    public async Task<ApiResponse<List<BatchTeacherDto>>> GetBatchTeachersAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.GetAsync($"api/v1/orgadmin/batches/{id}/teachers", cancellationToken);
            var result = await response.Content.ReadFromJsonAsync<ApiResponse<List<BatchTeacherDto>>>(cancellationToken: cancellationToken);
            return result ?? ApiResponse<List<BatchTeacherDto>>.FailureResponse("Received empty response.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching teachers for batch {BatchId}", id);
            return ApiResponse<List<BatchTeacherDto>>.FailureResponse(ex.Message);
        }
    }

    public async Task<ApiResponse<bool>> AssignTeacherToBatchAsync(
        Guid id,
        AssignTeacherToBatchRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync($"api/v1/orgadmin/batches/{id}/teachers", request, cancellationToken);
            var result = await response.Content.ReadFromJsonAsync<ApiResponse<bool>>(cancellationToken: cancellationToken);
            return result ?? ApiResponse<bool>.FailureResponse("Failed to assign teacher.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error assigning teacher to batch {BatchId}", id);
            return ApiResponse<bool>.FailureResponse(ex.Message);
        }
    }

    public async Task<ApiResponse<bool>> RemoveTeacherFromBatchAsync(
        Guid id,
        Guid teacherId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.DeleteAsync($"api/v1/orgadmin/batches/{id}/teachers/{teacherId}", cancellationToken);
            var result = await response.Content.ReadFromJsonAsync<ApiResponse<bool>>(cancellationToken: cancellationToken);
            return result ?? ApiResponse<bool>.FailureResponse("Failed to remove teacher.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing teacher {TeacherId} from batch {BatchId}", teacherId, id);
            return ApiResponse<bool>.FailureResponse(ex.Message);
        }
    }

    public async Task<ApiResponse<List<BatchStudentDto>>> GetBatchStudentsAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.GetAsync($"api/v1/orgadmin/batches/{id}/students", cancellationToken);
            var result = await response.Content.ReadFromJsonAsync<ApiResponse<List<BatchStudentDto>>>(cancellationToken: cancellationToken);
            return result ?? ApiResponse<List<BatchStudentDto>>.FailureResponse("Received empty response.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching students for batch {BatchId}", id);
            return ApiResponse<List<BatchStudentDto>>.FailureResponse(ex.Message);
        }
    }

    public async Task<ApiResponse<BatchStudentDto>> EnrollStudentToBatchAsync(
        Guid id,
        EnrollStudentToBatchRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync($"api/v1/orgadmin/batches/{id}/students", request, cancellationToken);
            var result = await response.Content.ReadFromJsonAsync<ApiResponse<BatchStudentDto>>(cancellationToken: cancellationToken);
            return result ?? ApiResponse<BatchStudentDto>.FailureResponse("Failed to enroll student.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error enrolling student to batch {BatchId}", id);
            return ApiResponse<BatchStudentDto>.FailureResponse(ex.Message);
        }
    }

    public async Task<ApiResponse<bool>> RemoveStudentFromBatchAsync(
        Guid id,
        Guid studentId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.DeleteAsync($"api/v1/orgadmin/batches/{id}/students/{studentId}", cancellationToken);
            var result = await response.Content.ReadFromJsonAsync<ApiResponse<bool>>(cancellationToken: cancellationToken);
            return result ?? ApiResponse<bool>.FailureResponse("Failed to remove student.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing student {StudentId} from batch {BatchId}", studentId, id);
            return ApiResponse<bool>.FailureResponse(ex.Message);
        }
    }
}
