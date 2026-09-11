using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using Vargshala.Contracts.Batches;
using Vargshala.Contracts.Common;

namespace Vargshala.Web.Services.BranchAdmin;

public interface IBranchBatchClientService
{
    Task<ApiResponse<PagedResponse<BatchDto>>> GetBatchesPagedAsync(PagedRequest? request = null, Guid? classId = null, Guid? subjectId = null, bool? isActive = null, CancellationToken cancellationToken = default);
    Task<ApiResponse<List<BatchDto>>> GetAllActiveBatchesAsync(Guid? classId = null, CancellationToken cancellationToken = default);
    Task<ApiResponse<BatchDto>> GetBatchByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ApiResponse<BatchDto>> CreateBatchAsync(CreateBatchRequest request, CancellationToken cancellationToken = default);
    Task<ApiResponse<BatchDto>> UpdateBatchAsync(Guid id, UpdateBatchRequest request, CancellationToken cancellationToken = default);
    Task<ApiResponse<bool>> DeleteBatchAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ApiResponse<bool>> ToggleBatchStatusAsync(Guid id, CancellationToken cancellationToken = default);

    // Faculty
    Task<ApiResponse<List<BatchTeacherDto>>> GetBatchTeachersAsync(Guid batchId, CancellationToken cancellationToken = default);
    Task<ApiResponse<BatchTeacherDto>> AssignTeacherAsync(Guid batchId, AssignTeacherToBatchRequest request, CancellationToken cancellationToken = default);
    Task<ApiResponse<bool>> RemoveTeacherAsync(Guid batchId, Guid teacherId, Guid? subjectId = null, CancellationToken cancellationToken = default);

    // Students
    Task<ApiResponse<List<BatchStudentDto>>> GetBatchStudentsAsync(Guid batchId, CancellationToken cancellationToken = default);
    Task<ApiResponse<BatchStudentDto>> EnrollStudentAsync(Guid batchId, EnrollStudentToBatchRequest request, CancellationToken cancellationToken = default);
    Task<ApiResponse<bool>> RemoveStudentAsync(Guid batchId, Guid studentId, CancellationToken cancellationToken = default);
}

public class BranchBatchClientService : IBranchBatchClientService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<BranchBatchClientService> _logger;

    public BranchBatchClientService(IHttpClientFactory httpClientFactory, ILogger<BranchBatchClientService> logger)
    {
        _httpClient = httpClientFactory.CreateClient("VargshalaApi");
        _logger = logger;
    }

    public async Task<ApiResponse<PagedResponse<BatchDto>>> GetBatchesPagedAsync(
        PagedRequest? request = null,
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
            if (classId.HasValue)
                queryParams += $"&classId={classId.Value}";
            if (subjectId.HasValue)
                queryParams += $"&subjectId={subjectId.Value}";
            if (isActive.HasValue)
                queryParams += $"&isActive={isActive.Value.ToString().ToLowerInvariant()}";

            var response = await _httpClient.GetAsync($"api/v1/branchadmin/batches{queryParams}", cancellationToken);
            var result = await response.Content.ReadFromJsonAsync<ApiResponse<PagedResponse<BatchDto>>>(cancellationToken: cancellationToken);
            return result ?? ApiResponse<PagedResponse<BatchDto>>.FailureResponse("Received empty response.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching branch batches");
            return ApiResponse<PagedResponse<BatchDto>>.FailureResponse(ex.Message);
        }
    }

    public async Task<ApiResponse<List<BatchDto>>> GetAllActiveBatchesAsync(Guid? classId = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var url = "api/v1/branchadmin/batches/all-active";
            if (classId.HasValue)
                url += $"?classId={classId.Value}";

            var response = await _httpClient.GetAsync(url, cancellationToken);
            var result = await response.Content.ReadFromJsonAsync<ApiResponse<List<BatchDto>>>(cancellationToken: cancellationToken);
            return result ?? ApiResponse<List<BatchDto>>.FailureResponse("Received empty response.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching all active branch batches");
            return ApiResponse<List<BatchDto>>.FailureResponse(ex.Message);
        }
    }

    public async Task<ApiResponse<BatchDto>> GetBatchByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.GetAsync($"api/v1/branchadmin/batches/{id}", cancellationToken);
            var result = await response.Content.ReadFromJsonAsync<ApiResponse<BatchDto>>(cancellationToken: cancellationToken);
            return result ?? ApiResponse<BatchDto>.FailureResponse("Failed to retrieve batch.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching branch batch by id");
            return ApiResponse<BatchDto>.FailureResponse(ex.Message);
        }
    }

    public async Task<ApiResponse<BatchDto>> CreateBatchAsync(CreateBatchRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/v1/branchadmin/batches", request, cancellationToken);
            var result = await response.Content.ReadFromJsonAsync<ApiResponse<BatchDto>>(cancellationToken: cancellationToken);
            return result ?? ApiResponse<BatchDto>.FailureResponse("Failed to create batch.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating branch batch");
            return ApiResponse<BatchDto>.FailureResponse(ex.Message);
        }
    }

    public async Task<ApiResponse<BatchDto>> UpdateBatchAsync(Guid id, UpdateBatchRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.PutAsJsonAsync($"api/v1/branchadmin/batches/{id}", request, cancellationToken);
            var result = await response.Content.ReadFromJsonAsync<ApiResponse<BatchDto>>(cancellationToken: cancellationToken);
            return result ?? ApiResponse<BatchDto>.FailureResponse("Failed to update batch.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating branch batch");
            return ApiResponse<BatchDto>.FailureResponse(ex.Message);
        }
    }

    public async Task<ApiResponse<bool>> DeleteBatchAsync(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.DeleteAsync($"api/v1/branchadmin/batches/{id}", cancellationToken);
            var result = await response.Content.ReadFromJsonAsync<ApiResponse<bool>>(cancellationToken: cancellationToken);
            return result ?? ApiResponse<bool>.FailureResponse("Failed to delete batch.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting branch batch");
            return ApiResponse<bool>.FailureResponse(ex.Message);
        }
    }

    public async Task<ApiResponse<bool>> ToggleBatchStatusAsync(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.PatchAsync($"api/v1/branchadmin/batches/{id}/toggle-status", null, cancellationToken);
            var result = await response.Content.ReadFromJsonAsync<ApiResponse<bool>>(cancellationToken: cancellationToken);
            return result ?? ApiResponse<bool>.FailureResponse("Failed to toggle batch status.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error toggling branch batch status");
            return ApiResponse<bool>.FailureResponse(ex.Message);
        }
    }

    #region Teacher Assignments
    public async Task<ApiResponse<List<BatchTeacherDto>>> GetBatchTeachersAsync(Guid batchId, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.GetAsync($"api/v1/branchadmin/batches/{batchId}/teachers", cancellationToken);
            var result = await response.Content.ReadFromJsonAsync<ApiResponse<List<BatchTeacherDto>>>(cancellationToken: cancellationToken);
            return result ?? ApiResponse<List<BatchTeacherDto>>.FailureResponse("Received empty response.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching batch teachers");
            return ApiResponse<List<BatchTeacherDto>>.FailureResponse(ex.Message);
        }
    }

    public async Task<ApiResponse<BatchTeacherDto>> AssignTeacherAsync(Guid batchId, AssignTeacherToBatchRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync($"api/v1/branchadmin/batches/{batchId}/teachers", request, cancellationToken);
            var result = await response.Content.ReadFromJsonAsync<ApiResponse<BatchTeacherDto>>(cancellationToken: cancellationToken);
            return result ?? ApiResponse<BatchTeacherDto>.FailureResponse("Failed to assign teacher.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error assigning teacher to batch");
            return ApiResponse<BatchTeacherDto>.FailureResponse(ex.Message);
        }
    }

    public async Task<ApiResponse<bool>> RemoveTeacherAsync(Guid batchId, Guid teacherId, Guid? subjectId = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var url = $"api/v1/branchadmin/batches/{batchId}/teachers/{teacherId}";
            if (subjectId.HasValue)
                url += $"?subjectId={subjectId.Value}";

            var response = await _httpClient.DeleteAsync(url, cancellationToken);
            var result = await response.Content.ReadFromJsonAsync<ApiResponse<bool>>(cancellationToken: cancellationToken);
            return result ?? ApiResponse<bool>.FailureResponse("Failed to remove teacher.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing teacher from batch");
            return ApiResponse<bool>.FailureResponse(ex.Message);
        }
    }
    #endregion

    #region Student Enrollments
    public async Task<ApiResponse<List<BatchStudentDto>>> GetBatchStudentsAsync(Guid batchId, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.GetAsync($"api/v1/branchadmin/batches/{batchId}/students", cancellationToken);
            var result = await response.Content.ReadFromJsonAsync<ApiResponse<List<BatchStudentDto>>>(cancellationToken: cancellationToken);
            return result ?? ApiResponse<List<BatchStudentDto>>.FailureResponse("Received empty response.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching batch students");
            return ApiResponse<List<BatchStudentDto>>.FailureResponse(ex.Message);
        }
    }

    public async Task<ApiResponse<BatchStudentDto>> EnrollStudentAsync(Guid batchId, EnrollStudentToBatchRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync($"api/v1/branchadmin/batches/{batchId}/students", request, cancellationToken);
            var result = await response.Content.ReadFromJsonAsync<ApiResponse<BatchStudentDto>>(cancellationToken: cancellationToken);
            return result ?? ApiResponse<BatchStudentDto>.FailureResponse("Failed to enroll student.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error enrolling student to batch");
            return ApiResponse<BatchStudentDto>.FailureResponse(ex.Message);
        }
    }

    public async Task<ApiResponse<bool>> RemoveStudentAsync(Guid batchId, Guid studentId, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.DeleteAsync($"api/v1/branchadmin/batches/{batchId}/students/{studentId}", cancellationToken);
            var result = await response.Content.ReadFromJsonAsync<ApiResponse<bool>>(cancellationToken: cancellationToken);
            return result ?? ApiResponse<bool>.FailureResponse("Failed to remove student.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing student from batch");
            return ApiResponse<bool>.FailureResponse(ex.Message);
        }
    }
    #endregion
}
