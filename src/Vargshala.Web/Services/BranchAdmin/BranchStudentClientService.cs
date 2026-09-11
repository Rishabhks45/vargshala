using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using Vargshala.Contracts.Batches;
using Vargshala.Contracts.Common;
using Vargshala.Contracts.Students;

namespace Vargshala.Web.Services.BranchAdmin;

public interface IBranchStudentClientService
{
    Task<ApiResponse<PagedResponse<StudentDto>>> GetStudentsPagedAsync(PagedRequest? request = null, string? className = null, string? section = null, bool? isActive = null, CancellationToken cancellationToken = default);
    Task<ApiResponse<GeneratedStudentCodeDto>> GenerateStudentCodeAsync(CancellationToken cancellationToken = default);
    Task<ApiResponse<string>> GenerateNextStudentCodeAsync(CancellationToken cancellationToken = default);
    Task<ApiResponse<StudentDto>> GetStudentByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ApiResponse<List<StudentBatchEnrollmentDto>>> GetStudentBatchesAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ApiResponse<StudentDto>> CreateStudentAsync(CreateStudentRequest request, CancellationToken cancellationToken = default);
    Task<ApiResponse<StudentDto>> UpdateStudentAsync(Guid id, UpdateStudentRequest request, CancellationToken cancellationToken = default);
    Task<ApiResponse<bool>> DeleteStudentAsync(Guid id, CancellationToken cancellationToken = default);
}

public class BranchStudentClientService : IBranchStudentClientService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<BranchStudentClientService> _logger;

    public BranchStudentClientService(IHttpClientFactory httpClientFactory, ILogger<BranchStudentClientService> logger)
    {
        _httpClient = httpClientFactory.CreateClient("VargshalaApi");
        _logger = logger;
    }

    public async Task<ApiResponse<PagedResponse<StudentDto>>> GetStudentsPagedAsync(
        PagedRequest? request = null,
        string? className = null,
        string? section = null,
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
            if (!string.IsNullOrWhiteSpace(className))
                queryParams += $"&className={Uri.EscapeDataString(className)}";
            if (!string.IsNullOrWhiteSpace(section))
                queryParams += $"&section={Uri.EscapeDataString(section)}";
            if (isActive.HasValue)
                queryParams += $"&isActive={isActive.Value.ToString().ToLowerInvariant()}";

            var response = await _httpClient.GetAsync($"api/v1/branchadmin/students{queryParams}", cancellationToken);
            var result = await response.Content.ReadFromJsonAsync<ApiResponse<PagedResponse<StudentDto>>>(cancellationToken: cancellationToken);
            return result ?? ApiResponse<PagedResponse<StudentDto>>.FailureResponse("Received empty response.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching branch students");
            return ApiResponse<PagedResponse<StudentDto>>.FailureResponse(ex.Message);
        }
    }

    public async Task<ApiResponse<GeneratedStudentCodeDto>> GenerateStudentCodeAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.GetAsync("api/v1/branchadmin/students/generate-code", cancellationToken);
            var result = await response.Content.ReadFromJsonAsync<ApiResponse<GeneratedStudentCodeDto>>(cancellationToken: cancellationToken);
            return result ?? ApiResponse<GeneratedStudentCodeDto>.FailureResponse("Received empty response.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating next student code");
            return ApiResponse<GeneratedStudentCodeDto>.FailureResponse(ex.Message);
        }
    }

    public async Task<ApiResponse<string>> GenerateNextStudentCodeAsync(CancellationToken cancellationToken = default)
    {
        var res = await GenerateStudentCodeAsync(cancellationToken);
        if (res.Success && res.Data != null)
        {
            return ApiResponse<string>.SuccessResponse(res.Data.StudentCode);
        }
        return ApiResponse<string>.FailureResponse(res.Message ?? "Failed to generate student code.");
    }

    public async Task<ApiResponse<StudentDto>> GetStudentByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.GetAsync($"api/v1/branchadmin/students/{id}", cancellationToken);
            var result = await response.Content.ReadFromJsonAsync<ApiResponse<StudentDto>>(cancellationToken: cancellationToken);
            return result ?? ApiResponse<StudentDto>.FailureResponse("Failed to retrieve student.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching branch student by id");
            return ApiResponse<StudentDto>.FailureResponse(ex.Message);
        }
    }

    public async Task<ApiResponse<List<StudentBatchEnrollmentDto>>> GetStudentBatchesAsync(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.GetAsync($"api/v1/branchadmin/students/{id}/batches", cancellationToken);
            var result = await response.Content.ReadFromJsonAsync<ApiResponse<List<StudentBatchEnrollmentDto>>>(cancellationToken: cancellationToken);
            return result ?? ApiResponse<List<StudentBatchEnrollmentDto>>.FailureResponse("Received empty response.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching student batches");
            return ApiResponse<List<StudentBatchEnrollmentDto>>.FailureResponse(ex.Message);
        }
    }

    public async Task<ApiResponse<StudentDto>> CreateStudentAsync(CreateStudentRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/v1/branchadmin/students", request, cancellationToken);
            var result = await response.Content.ReadFromJsonAsync<ApiResponse<StudentDto>>(cancellationToken: cancellationToken);
            return result ?? ApiResponse<StudentDto>.FailureResponse("Failed to create student.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating student");
            return ApiResponse<StudentDto>.FailureResponse(ex.Message);
        }
    }

    public async Task<ApiResponse<StudentDto>> UpdateStudentAsync(Guid id, UpdateStudentRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.PutAsJsonAsync($"api/v1/branchadmin/students/{id}", request, cancellationToken);
            var result = await response.Content.ReadFromJsonAsync<ApiResponse<StudentDto>>(cancellationToken: cancellationToken);
            return result ?? ApiResponse<StudentDto>.FailureResponse("Failed to update student.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating student");
            return ApiResponse<StudentDto>.FailureResponse(ex.Message);
        }
    }

    public async Task<ApiResponse<bool>> DeleteStudentAsync(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.DeleteAsync($"api/v1/branchadmin/students/{id}", cancellationToken);
            var result = await response.Content.ReadFromJsonAsync<ApiResponse<bool>>(cancellationToken: cancellationToken);
            return result ?? ApiResponse<bool>.FailureResponse("Failed to delete student.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting student");
            return ApiResponse<bool>.FailureResponse(ex.Message);
        }
    }
}
