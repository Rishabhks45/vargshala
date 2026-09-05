using System.Net.Http.Json;
using Vargshala.Contracts.Common;
using Vargshala.Contracts.Organizations;

namespace Vargshala.Web.Services;

public class InstituteService : IInstituteService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<InstituteService> _logger;

    public InstituteService(
        IHttpClientFactory httpClientFactory,
        ILogger<InstituteService> logger)
    {
        _httpClient = httpClientFactory.CreateClient("VargshalaApi");
        _logger = logger;
    }

    public async Task<ApiResponse<PagedResponse<InstituteSummaryDto>>> GetAllInstitutesAsync(PagedRequest? request = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var req = request ?? new PagedRequest();
            var queryParams = $"?pageNumber={req.PageNumber}&pageSize={req.PageSize}&sortDirection={req.SortDirection}";
            if (!string.IsNullOrWhiteSpace(req.Search))
                queryParams += $"&search={Uri.EscapeDataString(req.Search)}";
            if (!string.IsNullOrWhiteSpace(req.SortBy))
                queryParams += $"&sortBy={Uri.EscapeDataString(req.SortBy)}";

            var response = await _httpClient.GetAsync($"api/v1/organizations{queryParams}", cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<ApiResponse<PagedResponse<InstituteSummaryDto>>>(cancellationToken: cancellationToken);
                if (result != null && result.Success)
                {
                    return result;
                }
            }

            _logger.LogWarning("API returned non-success code {StatusCode} for GetAllInstitutes", response.StatusCode);
            return ApiResponse<PagedResponse<InstituteSummaryDto>>.FailureResponse($"API returned status code {(int)response.StatusCode}");
        }
        catch (HttpRequestException ex)
        {
            _logger.LogWarning(ex, "Could not reach API server for GetAllInstitutes.");
            return ApiResponse<PagedResponse<InstituteSummaryDto>>.FailureResponse("Unable to reach backend API server.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error in GetAllInstitutesAsync");
            return ApiResponse<PagedResponse<InstituteSummaryDto>>.FailureResponse(ex.Message);
        }
    }

    public async Task<ApiResponse<bool>> ToggleInstituteStatusAsync(Guid instituteId, CancellationToken cancellationToken = default)
    {
        try
        {
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

    public async Task<ApiResponse<OrganizationDto>> GetMyOrganizationAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.GetAsync("api/v1/organizations/me", cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<ApiResponse<OrganizationDto>>(cancellationToken: cancellationToken);
                if (result != null && result.Success)
                {
                    return result;
                }
            }

            var errResponse = await response.Content.ReadFromJsonAsync<ApiResponse<OrganizationDto>>(cancellationToken: cancellationToken);
            return errResponse ?? ApiResponse<OrganizationDto>.FailureResponse($"Failed to fetch organization (status code {(int)response.StatusCode}).");
        }
        catch (HttpRequestException ex)
        {
            _logger.LogWarning(ex, "Could not reach API server for GetMyOrganization.");
            return ApiResponse<OrganizationDto>.FailureResponse("Unable to reach backend API server.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error in GetMyOrganizationAsync");
            return ApiResponse<OrganizationDto>.FailureResponse(ex.Message);
        }
    }
}
