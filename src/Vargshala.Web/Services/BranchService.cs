using System.Net.Http.Json;
using Vargshala.Contracts.Branches;
using Vargshala.Contracts.Common;

namespace Vargshala.Web.Services;

public class BranchService : IBranchService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<BranchService> _logger;

    public BranchService(
        IHttpClientFactory httpClientFactory,
        ILogger<BranchService> logger)
    {
        _httpClient = httpClientFactory.CreateClient("VargshalaApi");
        _logger = logger;
    }

    public async Task<ApiResponse<PagedResponse<BranchDto>>> GetBranchesPagedAsync(
        PagedRequest? request = null,
        string? city = null,
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
            if (!string.IsNullOrWhiteSpace(city))
                queryParams += $"&city={Uri.EscapeDataString(city)}";
            if (isActive.HasValue)
                queryParams += $"&isActive={isActive.Value.ToString().ToLowerInvariant()}";

            var response = await _httpClient.GetAsync($"api/v1/orgadmin/branches{queryParams}", cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var errorResponse = await response.Content.ReadFromJsonAsync<ApiResponse<PagedResponse<BranchDto>>>(cancellationToken: cancellationToken);
                return errorResponse ?? ApiResponse<PagedResponse<BranchDto>>.FailureResponse("Failed to fetch branches.");
            }

            var result = await response.Content.ReadFromJsonAsync<ApiResponse<PagedResponse<BranchDto>>>(cancellationToken: cancellationToken);
            return result ?? ApiResponse<PagedResponse<BranchDto>>.FailureResponse("Invalid response from server.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching branches.");
            return ApiResponse<PagedResponse<BranchDto>>.FailureResponse($"Network or server error: {ex.Message}");
        }
    }

    public async Task<ApiResponse<List<BranchDto>>> GetAllActiveBranchesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.GetAsync("api/v1/orgadmin/branches/all-active", cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                var err = await response.Content.ReadFromJsonAsync<ApiResponse<List<BranchDto>>>(cancellationToken: cancellationToken);
                return err ?? ApiResponse<List<BranchDto>>.FailureResponse("Failed to load branches.");
            }

            var result = await response.Content.ReadFromJsonAsync<ApiResponse<List<BranchDto>>>(cancellationToken: cancellationToken);
            return result ?? ApiResponse<List<BranchDto>>.FailureResponse("Invalid response.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching active branches list.");
            return ApiResponse<List<BranchDto>>.FailureResponse($"Network or server error: {ex.Message}");
        }
    }

    public async Task<ApiResponse<BranchDto>> GetBranchByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.GetAsync($"api/v1/orgadmin/branches/{id}", cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadFromJsonAsync<ApiResponse<BranchDto>>(cancellationToken: cancellationToken);
                return error ?? ApiResponse<BranchDto>.FailureResponse("Failed to fetch branch details.");
            }

            var result = await response.Content.ReadFromJsonAsync<ApiResponse<BranchDto>>(cancellationToken: cancellationToken);
            return result ?? ApiResponse<BranchDto>.FailureResponse("Invalid response.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching branch by id {Id}", id);
            return ApiResponse<BranchDto>.FailureResponse($"Network or server error: {ex.Message}");
        }
    }

    public async Task<ApiResponse<BranchDto>> CreateBranchAsync(CreateBranchRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/v1/orgadmin/branches", request, cancellationToken);
            var result = await response.Content.ReadFromJsonAsync<ApiResponse<BranchDto>>(cancellationToken: cancellationToken);
            return result ?? ApiResponse<BranchDto>.FailureResponse("Failed to create branch.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating branch.");
            return ApiResponse<BranchDto>.FailureResponse($"Network or server error: {ex.Message}");
        }
    }

    public async Task<ApiResponse<BranchDto>> UpdateBranchAsync(UpdateBranchRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.PutAsJsonAsync($"api/v1/orgadmin/branches/{request.Id}", request, cancellationToken);
            var result = await response.Content.ReadFromJsonAsync<ApiResponse<BranchDto>>(cancellationToken: cancellationToken);
            return result ?? ApiResponse<BranchDto>.FailureResponse("Failed to update branch.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating branch.");
            return ApiResponse<BranchDto>.FailureResponse($"Network or server error: {ex.Message}");
        }
    }

    public async Task<ApiResponse<bool>> DeleteBranchAsync(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.DeleteAsync($"api/v1/orgadmin/branches/{id}", cancellationToken);
            var result = await response.Content.ReadFromJsonAsync<ApiResponse<bool>>(cancellationToken: cancellationToken);
            return result ?? ApiResponse<bool>.FailureResponse("Failed to delete branch.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting branch.");
            return ApiResponse<bool>.FailureResponse($"Network or server error: {ex.Message}");
        }
    }

    public async Task<ApiResponse<List<UserBranchAccessDto>>> GetUserBranchesAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.GetAsync($"api/v1/orgadmin/branches/user/{userId}", cancellationToken);
            var result = await response.Content.ReadFromJsonAsync<ApiResponse<List<UserBranchAccessDto>>>(cancellationToken: cancellationToken);
            return result ?? ApiResponse<List<UserBranchAccessDto>>.FailureResponse("Failed to get user branches.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user branches.");
            return ApiResponse<List<UserBranchAccessDto>>.FailureResponse($"Network or server error: {ex.Message}");
        }
    }

    public async Task<ApiResponse<bool>> AssignUserBranchesAsync(AssignUserBranchesRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/v1/orgadmin/branches/assign-user", request, cancellationToken);
            var result = await response.Content.ReadFromJsonAsync<ApiResponse<bool>>(cancellationToken: cancellationToken);
            return result ?? ApiResponse<bool>.FailureResponse("Failed to assign user branches.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error assigning user branches.");
            return ApiResponse<bool>.FailureResponse($"Network or server error: {ex.Message}");
        }
    }
}
