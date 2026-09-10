using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using Vargshala.Contracts.Attendances;
using Vargshala.Contracts.Common;

namespace Vargshala.Web.Services;

public class AttendanceService : IAttendanceService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<AttendanceService> _logger;

    public AttendanceService(
        IHttpClientFactory httpClientFactory,
        ILogger<AttendanceService> logger)
    {
        _httpClient = httpClientFactory.CreateClient("VargshalaApi");
        _logger = logger;
    }

    public async Task<ApiResponse<SessionAttendanceSheetDto>> GetSessionAttendanceSheetAsync(Guid sessionId)
    {
        try
        {
            var response = await _httpClient.GetAsync($"api/v1/orgadmin/attendances/session/{sessionId}");
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<ApiResponse<SessionAttendanceSheetDto>>();
                return result ?? ApiResponse<SessionAttendanceSheetDto>.FailureResponse("Failed to deserialize response.");
            }

            var err = await response.Content.ReadFromJsonAsync<ApiResponse<SessionAttendanceSheetDto>>();
            return err ?? ApiResponse<SessionAttendanceSheetDto>.FailureResponse($"Server returned status code {response.StatusCode}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting attendance sheet for session {SessionId}", sessionId);
            return ApiResponse<SessionAttendanceSheetDto>.FailureResponse(ex.Message);
        }
    }

    public async Task<ApiResponse<SessionAttendanceSheetDto>> SaveSessionAttendanceAsync(Guid sessionId, MarkSessionAttendanceRequest request)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync($"api/v1/orgadmin/attendances/session/{sessionId}", request);
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<ApiResponse<SessionAttendanceSheetDto>>();
                return result ?? ApiResponse<SessionAttendanceSheetDto>.FailureResponse("Failed to deserialize response.");
            }

            var err = await response.Content.ReadFromJsonAsync<ApiResponse<SessionAttendanceSheetDto>>();
            return err ?? ApiResponse<SessionAttendanceSheetDto>.FailureResponse($"Server returned status code {response.StatusCode}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving attendance for session {SessionId}", sessionId);
            return ApiResponse<SessionAttendanceSheetDto>.FailureResponse(ex.Message);
        }
    }

    public async Task<ApiResponse<BatchAttendanceOverviewDto>> GetBatchOverviewAsync(Guid batchId, DateOnly? date = null)
    {
        try
        {
            var url = $"api/v1/orgadmin/attendances/batch/{batchId}/overview";
            if (date.HasValue)
            {
                url += $"?date={date.Value:yyyy-MM-dd}";
            }

            var response = await _httpClient.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<ApiResponse<BatchAttendanceOverviewDto>>();
                return result ?? ApiResponse<BatchAttendanceOverviewDto>.FailureResponse("Failed to deserialize response.");
            }

            var err = await response.Content.ReadFromJsonAsync<ApiResponse<BatchAttendanceOverviewDto>>();
            return err ?? ApiResponse<BatchAttendanceOverviewDto>.FailureResponse($"Server returned status code {response.StatusCode}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting batch overview for {BatchId}", batchId);
            return ApiResponse<BatchAttendanceOverviewDto>.FailureResponse(ex.Message);
        }
    }

    public async Task<ApiResponse<List<DateAttendanceReportDto>>> GetDateWiseReportAsync(Guid batchId, DateOnly? fromDate = null, DateOnly? toDate = null)
    {
        try
        {
            var queryParams = new List<string>();
            if (fromDate.HasValue) queryParams.Add($"fromDate={fromDate.Value:yyyy-MM-dd}");
            if (toDate.HasValue) queryParams.Add($"toDate={toDate.Value:yyyy-MM-dd}");
            var qs = queryParams.Count > 0 ? "?" + string.Join("&", queryParams) : "";

            var url = $"api/v1/orgadmin/attendances/batch/{batchId}/date-report{qs}";
            var response = await _httpClient.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<ApiResponse<List<DateAttendanceReportDto>>>();
                return result ?? ApiResponse<List<DateAttendanceReportDto>>.FailureResponse("Failed to deserialize response.");
            }

            var err = await response.Content.ReadFromJsonAsync<ApiResponse<List<DateAttendanceReportDto>>>();
            return err ?? ApiResponse<List<DateAttendanceReportDto>>.FailureResponse($"Server returned status code {response.StatusCode}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting date wise report for {BatchId}", batchId);
            return ApiResponse<List<DateAttendanceReportDto>>.FailureResponse(ex.Message);
        }
    }

    public async Task<ApiResponse<List<StudentAttendanceReportDto>>> GetStudentWiseReportAsync(Guid batchId)
    {
        try
        {
            var url = $"api/v1/orgadmin/attendances/batch/{batchId}/student-report";
            var response = await _httpClient.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<ApiResponse<List<StudentAttendanceReportDto>>>();
                return result ?? ApiResponse<List<StudentAttendanceReportDto>>.FailureResponse("Failed to deserialize response.");
            }

            var err = await response.Content.ReadFromJsonAsync<ApiResponse<List<StudentAttendanceReportDto>>>();
            return err ?? ApiResponse<List<StudentAttendanceReportDto>>.FailureResponse($"Server returned status code {response.StatusCode}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting student wise report for {BatchId}", batchId);
            return ApiResponse<List<StudentAttendanceReportDto>>.FailureResponse(ex.Message);
        }
    }
}
