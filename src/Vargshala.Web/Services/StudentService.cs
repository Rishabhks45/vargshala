using System.Net.Http.Json;
using Vargshala.Contracts.Common;
using Vargshala.Contracts.Students;

namespace Vargshala.Web.Services;

public class StudentService : IStudentService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<StudentService> _logger;

    public StudentService(
        IHttpClientFactory httpClientFactory,
        ILogger<StudentService> logger)
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

            var response = await _httpClient.GetAsync($"api/v1/orgadmin/students{queryParams}", cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var errorResponse = await response.Content.ReadFromJsonAsync<ApiResponse<PagedResponse<StudentDto>>>(cancellationToken: cancellationToken);
                return errorResponse ?? ApiResponse<PagedResponse<StudentDto>>.FailureResponse("Failed to fetch students.");
            }

            var result = await response.Content.ReadFromJsonAsync<ApiResponse<PagedResponse<StudentDto>>>(cancellationToken: cancellationToken);
            return result ?? ApiResponse<PagedResponse<StudentDto>>.FailureResponse("Invalid response from server.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching students.");
            return ApiResponse<PagedResponse<StudentDto>>.FailureResponse($"Network or server error: {ex.Message}");
        }
    }

    public async Task<ApiResponse<StudentDto>> GetStudentByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.GetAsync($"api/v1/orgadmin/students/{id}", cancellationToken);
            var result = await response.Content.ReadFromJsonAsync<ApiResponse<StudentDto>>(cancellationToken: cancellationToken);
            return result ?? ApiResponse<StudentDto>.FailureResponse("Failed to fetch student details.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching student {Id}", id);
            return ApiResponse<StudentDto>.FailureResponse($"Network or server error: {ex.Message}");
        }
    }

    public async Task<ApiResponse<StudentDto>> CreateStudentAsync(
        CreateStudentRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/v1/orgadmin/students", request, cancellationToken);
            var result = await response.Content.ReadFromJsonAsync<ApiResponse<StudentDto>>(cancellationToken: cancellationToken);
            return result ?? ApiResponse<StudentDto>.FailureResponse("Failed to create student.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating student.");
            return ApiResponse<StudentDto>.FailureResponse($"Network or server error: {ex.Message}");
        }
    }

    public async Task<ApiResponse<StudentDto>> UpdateStudentAsync(
        UpdateStudentRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.PutAsJsonAsync($"api/v1/orgadmin/students/{request.Id}", request, cancellationToken);
            var result = await response.Content.ReadFromJsonAsync<ApiResponse<StudentDto>>(cancellationToken: cancellationToken);
            return result ?? ApiResponse<StudentDto>.FailureResponse("Failed to update student.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating student {Id}", request.Id);
            return ApiResponse<StudentDto>.FailureResponse($"Network or server error: {ex.Message}");
        }
    }

    public async Task<ApiResponse<bool>> DeleteStudentAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.DeleteAsync($"api/v1/orgadmin/students/{id}", cancellationToken);
            var result = await response.Content.ReadFromJsonAsync<ApiResponse<bool>>(cancellationToken: cancellationToken);
            return result ?? ApiResponse<bool>.FailureResponse("Failed to delete student.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting student {Id}", id);
            return ApiResponse<bool>.FailureResponse($"Network or server error: {ex.Message}");
        }
    }

    public async Task<ApiResponse<GeneratedStudentCodeDto>?> GenerateStudentCodeAsync(
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<ApiResponse<GeneratedStudentCodeDto>>(
                "api/v1/orgadmin/students/generate-code",
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating student code");
            return ApiResponse<GeneratedStudentCodeDto>.FailureResponse($"Network or server error: {ex.Message}");
        }
    }
}
