using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using Vargshala.Contracts.Common;
using Vargshala.Contracts.Organizations;

namespace Vargshala.Web.Services;

public class InstituteService : IInstituteService
{
    private readonly HttpClient _httpClient;
    private readonly AuthenticationStateProvider _authStateProvider;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<InstituteService> _logger;

    public InstituteService(
        IHttpClientFactory httpClientFactory,
        AuthenticationStateProvider authStateProvider,
        IHttpContextAccessor httpContextAccessor,
        ILogger<InstituteService> logger)
    {
        _httpClient = httpClientFactory.CreateClient("VargshalaApi");
        _authStateProvider = authStateProvider;
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    private async Task AttachBearerTokenAsync()
    {
        string? token = null;

        if (_httpContextAccessor.HttpContext?.User != null)
        {
            token = _httpContextAccessor.HttpContext.User.FindFirst("access_token")?.Value;
        }

        if (string.IsNullOrWhiteSpace(token))
        {
            var authState = await _authStateProvider.GetAuthenticationStateAsync();
            token = authState.User.FindFirst("access_token")?.Value;
        }

        if (!string.IsNullOrWhiteSpace(token))
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }
    }

    public async Task<ApiResponse<List<InstituteSummaryDto>>> GetAllInstitutesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await AttachBearerTokenAsync();
            var response = await _httpClient.GetAsync("api/v1/organizations", cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<ApiResponse<List<InstituteSummaryDto>>>(cancellationToken: cancellationToken);
                if (result != null && result.Success)
                {
                    return result;
                }
            }

            _logger.LogWarning("API returned non-success code {StatusCode} for GetAllInstitutes", response.StatusCode);
            return ApiResponse<List<InstituteSummaryDto>>.FailureResponse($"API returned status code {(int)response.StatusCode}");
        }
        catch (HttpRequestException ex)
        {
            _logger.LogWarning(ex, "Could not reach API server for GetAllInstitutes.");
            return ApiResponse<List<InstituteSummaryDto>>.FailureResponse("Unable to reach backend API server.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error in GetAllInstitutesAsync");
            return ApiResponse<List<InstituteSummaryDto>>.FailureResponse(ex.Message);
        }
    }

    public async Task<ApiResponse<bool>> ToggleInstituteStatusAsync(Guid instituteId, CancellationToken cancellationToken = default)
    {
        try
        {
            await AttachBearerTokenAsync();
            var response = await _httpClient.PatchAsync($"api/v1/organizations/{instituteId}/toggle-status", null, cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<ApiResponse<bool>>(cancellationToken: cancellationToken);
                if (result != null)
                {
                    return result;
                }
            }

            return ApiResponse<bool>.FailureResponse("Failed to toggle status on API.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error toggling status for organization {Id}", instituteId);
            return ApiResponse<bool>.FailureResponse(ex.Message);
        }
    }

    public async Task<ApiResponse<OrganizationDto>> UpdateInstituteAsync(UpdateOrganizationRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            await AttachBearerTokenAsync();
            var response = await _httpClient.PutAsJsonAsync($"api/v1/organizations/{request.Id}", request, cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<ApiResponse<OrganizationDto>>(cancellationToken: cancellationToken);
                if (result != null)
                {
                    return result;
                }
            }

            var errResponse = await response.Content.ReadFromJsonAsync<ApiResponse<OrganizationDto>>(cancellationToken: cancellationToken);
            return errResponse ?? ApiResponse<OrganizationDto>.FailureResponse($"Failed to update institute (status code {(int)response.StatusCode}).");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating institute {Id}", request.Id);
            return ApiResponse<OrganizationDto>.FailureResponse(ex.Message);
        }
    }
}
