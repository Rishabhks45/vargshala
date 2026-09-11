using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using Vargshala.Contracts.Attendances;
using Vargshala.Contracts.Common;

namespace Vargshala.Web.Services.BranchAdmin;

public interface IBranchAttendanceClientService
{
    Task<ApiResponse<SessionAttendanceSheetDto>> GetSessionAttendanceSheetAsync(Guid sessionId, CancellationToken cancellationToken = default);
    Task<ApiResponse<SessionAttendanceSheetDto>> SaveSessionAttendanceAsync(Guid sessionId, MarkSessionAttendanceRequest request, CancellationToken cancellationToken = default);
    Task<ApiResponse<BatchAttendanceOverviewDto>> GetBatchOverviewAsync(Guid batchId, DateOnly? date = null, CancellationToken cancellationToken = default);
    Task<ApiResponse<List<DateAttendanceReportDto>>> GetDateWiseReportAsync(Guid batchId, DateOnly? fromDate = null, DateOnly? toDate = null, CancellationToken cancellationToken = default);
    Task<ApiResponse<List<StudentAttendanceReportDto>>> GetStudentWiseReportAsync(Guid batchId, CancellationToken cancellationToken = default);
}

public class BranchAttendanceClientService : IBranchAttendanceClientService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<BranchAttendanceClientService> _logger;

    public BranchAttendanceClientService(IHttpClientFactory httpClientFactory, ILogger<BranchAttendanceClientService> logger)
    {
        _httpClient = httpClientFactory.CreateClient("VargshalaApi");
        _logger = logger;
    }

    public async Task<ApiResponse<SessionAttendanceSheetDto>> GetSessionAttendanceSheetAsync(Guid sessionId, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.GetAsync($"api/v1/branchadmin/attendances/session/{sessionId}", cancellationToken);
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<ApiResponse<SessionAttendanceSheetDto>>(cancellationToken: cancellationToken);
                return result ?? ApiResponse<SessionAttendanceSheetDto>.FailureResponse("Failed to deserialize response.");
            }

            var err = await response.Content.ReadFromJsonAsync<ApiResponse<SessionAttendanceSheetDto>>(cancellationToken: cancellationToken);
            return err ?? ApiResponse<SessionAttendanceSheetDto>.FailureResponse($"Server returned status code {response.StatusCode}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting branch attendance sheet for session {SessionId}", sessionId);
            return ApiResponse<SessionAttendanceSheetDto>.FailureResponse(ex.Message);
        }
    }

    public async Task<ApiResponse<SessionAttendanceSheetDto>> SaveSessionAttendanceAsync(Guid sessionId, MarkSessionAttendanceRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync($"api/v1/branchadmin/attendances/session/{sessionId}", request, cancellationToken);
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<ApiResponse<SessionAttendanceSheetDto>>(cancellationToken: cancellationToken);
                return result ?? ApiResponse<SessionAttendanceSheetDto>.FailureResponse("Failed to deserialize response.");
            }

            var err = await response.Content.ReadFromJsonAsync<ApiResponse<SessionAttendanceSheetDto>>(cancellationToken: cancellationToken);
            return err ?? ApiResponse<SessionAttendanceSheetDto>.FailureResponse($"Server returned status code {response.StatusCode}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving branch attendance for session {SessionId}", sessionId);
            return ApiResponse<SessionAttendanceSheetDto>.FailureResponse(ex.Message);
        }
    }

    public async Task<ApiResponse<BatchAttendanceOverviewDto>> GetBatchOverviewAsync(Guid batchId, DateOnly? date = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var url = $"api/v1/branchadmin/attendances/batch/{batchId}/overview";
            if (date.HasValue)
            {
                url += $"?date={date.Value:yyyy-MM-dd}";
            }

            var response = await _httpClient.GetAsync(url, cancellationToken);
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<ApiResponse<BatchAttendanceOverviewDto>>(cancellationToken: cancellationToken);
                return result ?? ApiResponse<BatchAttendanceOverviewDto>.FailureResponse("Failed to deserialize response.");
            }

            var err = await response.Content.ReadFromJsonAsync<ApiResponse<BatchAttendanceOverviewDto>>(cancellationToken: cancellationToken);
            return err ?? ApiResponse<BatchAttendanceOverviewDto>.FailureResponse($"Server returned status code {response.StatusCode}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting branch batch overview for {BatchId}", batchId);
            return ApiResponse<BatchAttendanceOverviewDto>.FailureResponse(ex.Message);
        }
    }

    public async Task<ApiResponse<List<DateAttendanceReportDto>>> GetDateWiseReportAsync(Guid batchId, DateOnly? fromDate = null, DateOnly? toDate = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var queryParams = new List<string>();
            if (fromDate.HasValue) queryParams.Add($"fromDate={fromDate.Value:yyyy-MM-dd}");
            if (toDate.HasValue) queryParams.Add($"toDate={toDate.Value:yyyy-MM-dd}");
            var qs = queryParams.Count > 0 ? "?" + string.Join("&", queryParams) : "";

            var url = $"api/v1/branchadmin/attendances/batch/{batchId}/date-report{qs}";
            var response = await _httpClient.GetAsync(url, cancellationToken);
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<ApiResponse<List<DateAttendanceReportDto>>>(cancellationToken: cancellationToken);
                return result ?? ApiResponse<List<DateAttendanceReportDto>>.FailureResponse("Failed to deserialize response.");
            }

            var err = await response.Content.ReadFromJsonAsync<ApiResponse<List<DateAttendanceReportDto>>>(cancellationToken: cancellationToken);
            return err ?? ApiResponse<List<DateAttendanceReportDto>>.FailureResponse($"Server returned status code {response.StatusCode}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting branch date wise report for {BatchId}", batchId);
            return ApiResponse<List<DateAttendanceReportDto>>.FailureResponse(ex.Message);
        }
    }

    public async Task<ApiResponse<List<StudentAttendanceReportDto>>> GetStudentWiseReportAsync(Guid batchId, CancellationToken cancellationToken = default)
    {
        try
        {
            var url = $"api/v1/branchadmin/attendances/batch/{batchId}/student-report";
            var response = await _httpClient.GetAsync(url, cancellationToken);
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<ApiResponse<List<StudentAttendanceReportDto>>>(cancellationToken: cancellationToken);
                return result ?? ApiResponse<List<StudentAttendanceReportDto>>.FailureResponse("Failed to deserialize response.");
            }

            var err = await response.Content.ReadFromJsonAsync<ApiResponse<List<StudentAttendanceReportDto>>>(cancellationToken: cancellationToken);
            return err ?? ApiResponse<List<StudentAttendanceReportDto>>.FailureResponse($"Server returned status code {response.StatusCode}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting branch student wise report for {BatchId}", batchId);
            return ApiResponse<List<StudentAttendanceReportDto>>.FailureResponse(ex.Message);
        }
    }
}
