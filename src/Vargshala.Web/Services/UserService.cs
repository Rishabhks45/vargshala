using System.Net.Http.Json;
using Vargshala.Contracts.Common;
using Vargshala.Contracts.Users;

namespace Vargshala.Web.Services;

public class UserService : IUserService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<UserService> _logger;

    public UserService(
        IHttpClientFactory httpClientFactory,
        ILogger<UserService> logger)
    {
        _httpClient = httpClientFactory.CreateClient("VargshalaApi");
        _logger = logger;
    }

    public async Task<ApiResponse<PagedResponse<UserDto>>> GetControlPanelUsersAsync(
        PagedRequest? request = null,
        UserRole? role = null,
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
            if (role.HasValue)
                queryParams += $"&role={role.Value}";
            if (isActive.HasValue)
                queryParams += $"&isActive={isActive.Value.ToString().ToLowerInvariant()}";

            var response = await _httpClient.GetAsync($"api/v1/users/controlpanel{queryParams}", cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogWarning("Failed to fetch control panel users. Status: {StatusCode}, Error: {Error}", response.StatusCode, errorContent);
                return ApiResponse<PagedResponse<UserDto>>.FailureResponse($"API returned {response.StatusCode}");
            }

            var result = await response.Content.ReadFromJsonAsync<ApiResponse<PagedResponse<UserDto>>>(cancellationToken: cancellationToken);
            return result ?? ApiResponse<PagedResponse<UserDto>>.FailureResponse("Received null response from server.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching control panel users");
            return ApiResponse<PagedResponse<UserDto>>.FailureResponse($"Error fetching users: {ex.Message}");
        }
    }

    public async Task<ApiResponse<UserDto>> CreateControlPanelUserAsync(
        CreateUserRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/v1/users/controlpanel", request, cancellationToken);
            var content = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                try
                {
                    var errObj = System.Text.Json.JsonSerializer.Deserialize<ApiResponse<UserDto>>(content, new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    if (errObj != null && !string.IsNullOrWhiteSpace(errObj.Message))
                    {
                        return errObj;
                    }
                }
                catch { }

                return ApiResponse<UserDto>.FailureResponse($"Failed to create user (Status: {response.StatusCode})");
            }

            var result = await response.Content.ReadFromJsonAsync<ApiResponse<UserDto>>(cancellationToken: cancellationToken);
            return result ?? ApiResponse<UserDto>.FailureResponse("User creation failed.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating control panel user");
            return ApiResponse<UserDto>.FailureResponse($"Error creating user: {ex.Message}");
        }
    }

    public async Task<ApiResponse<UserDto>> UpdateControlPanelUserAsync(
        UpdateUserRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.PutAsJsonAsync($"api/v1/users/controlpanel/{request.Id}", request, cancellationToken);
            var content = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                try
                {
                    var errObj = System.Text.Json.JsonSerializer.Deserialize<ApiResponse<UserDto>>(content, new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    if (errObj != null && !string.IsNullOrWhiteSpace(errObj.Message))
                    {
                        return errObj;
                    }
                }
                catch { }

                return ApiResponse<UserDto>.FailureResponse($"Failed to update user (Status: {response.StatusCode})");
            }

            var result = await response.Content.ReadFromJsonAsync<ApiResponse<UserDto>>(cancellationToken: cancellationToken);
            return result ?? ApiResponse<UserDto>.FailureResponse("User update failed.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating control panel user");
            return ApiResponse<UserDto>.FailureResponse($"Error updating user: {ex.Message}");
        }
    }

    public async Task<ApiResponse<bool>> ToggleUserStatusAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.PatchAsync($"api/v1/users/{userId}/toggle-status", null, cancellationToken);
            var content = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                try
                {
                    var errObj = System.Text.Json.JsonSerializer.Deserialize<ApiResponse<bool>>(content, new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    if (errObj != null && !string.IsNullOrWhiteSpace(errObj.Message))
                    {
                        return errObj;
                    }
                }
                catch { }

                return ApiResponse<bool>.FailureResponse($"Failed to update status (Status: {response.StatusCode})");
            }

            var result = await response.Content.ReadFromJsonAsync<ApiResponse<bool>>(cancellationToken: cancellationToken);
            return result ?? ApiResponse<bool>.FailureResponse("Status update failed.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error toggling user status for ID: {UserId}", userId);
            return ApiResponse<bool>.FailureResponse($"Error updating status: {ex.Message}");
        }
    }
}
