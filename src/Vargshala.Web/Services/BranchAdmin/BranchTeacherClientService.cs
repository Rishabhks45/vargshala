using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using Vargshala.Contracts.Common;
using Vargshala.Contracts.Teachers;

namespace Vargshala.Web.Services.BranchAdmin;

public interface IBranchTeacherClientService
{
    Task<ApiResponse<PagedResponse<TeacherDto>>> GetTeachersPagedAsync(PagedRequest? request = null, string? department = null, string? designation = null, bool? isActive = null, CancellationToken cancellationToken = default);
    Task<ApiResponse<string>> GenerateNextTeacherCodeAsync(CancellationToken cancellationToken = default);
    Task<ApiResponse<TeacherDto>> GetTeacherByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ApiResponse<TeacherDto>> CreateTeacherAsync(CreateTeacherRequest request, CancellationToken cancellationToken = default);
    Task<ApiResponse<TeacherDto>> UpdateTeacherAsync(Guid id, UpdateTeacherRequest request, CancellationToken cancellationToken = default);
    Task<ApiResponse<bool>> DeleteTeacherAsync(Guid id, CancellationToken cancellationToken = default);
}

public class BranchTeacherClientService : IBranchTeacherClientService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<BranchTeacherClientService> _logger;

    public BranchTeacherClientService(IHttpClientFactory httpClientFactory, ILogger<BranchTeacherClientService> logger)
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

            var response = await _httpClient.GetAsync($"api/v1/branchadmin/teachers{queryParams}", cancellationToken);
            var result = await response.Content.ReadFromJsonAsync<ApiResponse<PagedResponse<TeacherDto>>>(cancellationToken: cancellationToken);
            return result ?? ApiResponse<PagedResponse<TeacherDto>>.FailureResponse("Received empty response.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching branch teachers");
            return ApiResponse<PagedResponse<TeacherDto>>.FailureResponse(ex.Message);
        }
    }

    public async Task<ApiResponse<string>> GenerateNextTeacherCodeAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.GetAsync("api/v1/branchadmin/teachers/generate-code", cancellationToken);
            var result = await response.Content.ReadFromJsonAsync<ApiResponse<string>>(cancellationToken: cancellationToken);
            return result ?? ApiResponse<string>.FailureResponse("Received empty response.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating next teacher code");
            return ApiResponse<string>.FailureResponse(ex.Message);
        }
    }

    public async Task<ApiResponse<TeacherDto>> GetTeacherByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.GetAsync($"api/v1/branchadmin/teachers/{id}", cancellationToken);
            var result = await response.Content.ReadFromJsonAsync<ApiResponse<TeacherDto>>(cancellationToken: cancellationToken);
            return result ?? ApiResponse<TeacherDto>.FailureResponse("Failed to retrieve teacher.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching branch teacher by id");
            return ApiResponse<TeacherDto>.FailureResponse(ex.Message);
        }
    }

    public async Task<ApiResponse<TeacherDto>> CreateTeacherAsync(CreateTeacherRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/v1/branchadmin/teachers", request, cancellationToken);
            var result = await response.Content.ReadFromJsonAsync<ApiResponse<TeacherDto>>(cancellationToken: cancellationToken);
            return result ?? ApiResponse<TeacherDto>.FailureResponse("Failed to create teacher.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating teacher");
            return ApiResponse<TeacherDto>.FailureResponse(ex.Message);
        }
    }

    public async Task<ApiResponse<TeacherDto>> UpdateTeacherAsync(Guid id, UpdateTeacherRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.PutAsJsonAsync($"api/v1/branchadmin/teachers/{id}", request, cancellationToken);
            var result = await response.Content.ReadFromJsonAsync<ApiResponse<TeacherDto>>(cancellationToken: cancellationToken);
            return result ?? ApiResponse<TeacherDto>.FailureResponse("Failed to update teacher.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating teacher");
            return ApiResponse<TeacherDto>.FailureResponse(ex.Message);
        }
    }

    public async Task<ApiResponse<bool>> DeleteTeacherAsync(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.DeleteAsync($"api/v1/branchadmin/teachers/{id}", cancellationToken);
            var result = await response.Content.ReadFromJsonAsync<ApiResponse<bool>>(cancellationToken: cancellationToken);
            return result ?? ApiResponse<bool>.FailureResponse("Failed to delete teacher.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting teacher");
            return ApiResponse<bool>.FailureResponse(ex.Message);
        }
    }
}
