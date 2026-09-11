using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using Vargshala.Contracts.BranchAdmin;
using Vargshala.Contracts.Common;

namespace Vargshala.Web.Services.BranchAdmin;

public interface IBranchDashboardClientService
{
    Task<ApiResponse<BranchDashboardDto>> GetDashboardStatsAsync(CancellationToken cancellationToken = default);
}

public class BranchDashboardClientService : IBranchDashboardClientService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<BranchDashboardClientService> _logger;

    public BranchDashboardClientService(IHttpClientFactory httpClientFactory, ILogger<BranchDashboardClientService> logger)
    {
        _httpClient = httpClientFactory.CreateClient("VargshalaApi");
        _logger = logger;
    }

    public async Task<ApiResponse<BranchDashboardDto>> GetDashboardStatsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.GetAsync("api/v1/branchadmin/dashboard", cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadFromJsonAsync<ApiResponse<BranchDashboardDto>>(cancellationToken: cancellationToken);
                return error ?? ApiResponse<BranchDashboardDto>.FailureResponse("Failed to retrieve branch dashboard data.");
            }

            var result = await response.Content.ReadFromJsonAsync<ApiResponse<BranchDashboardDto>>(cancellationToken: cancellationToken);
            return result ?? ApiResponse<BranchDashboardDto>.FailureResponse("Received empty response from server.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching branch dashboard stats");
            return ApiResponse<BranchDashboardDto>.FailureResponse(ex.Message);
        }
    }
}
