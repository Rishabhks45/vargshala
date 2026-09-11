using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Vargshala.Contracts.Common;
using Vargshala.Contracts.Fees;

namespace Vargshala.Web.Services;

public class FeeService : IFeeService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<FeeService> _logger;

    public FeeService(
        IHttpClientFactory httpClientFactory,
        ILogger<FeeService> logger)
    {
        _httpClient = httpClientFactory.CreateClient("VargshalaApi");
        _logger = logger;
    }

    private async Task<ApiResponse<T>> ParseResponseAsync<T>(
        HttpResponseMessage response,
        string defaultErrorMessage,
        CancellationToken ct)
    {
        try
        {
            var contentStr = await response.Content.ReadAsStringAsync(ct);
            if (string.IsNullOrWhiteSpace(contentStr))
            {
                if (!response.IsSuccessStatusCode)
                {
                    return ApiResponse<T>.FailureResponse($"{defaultErrorMessage} (HTTP {(int)response.StatusCode})");
                }
                return ApiResponse<T>.FailureResponse("Received empty response from server.");
            }

            // Attempt JSON deserialization
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var result = JsonSerializer.Deserialize<ApiResponse<T>>(contentStr, options);
            if (result != null)
            {
                return result;
            }

            return response.IsSuccessStatusCode
                ? ApiResponse<T>.FailureResponse("Failed to parse server response.")
                : ApiResponse<T>.FailureResponse($"{defaultErrorMessage} (HTTP {(int)response.StatusCode})");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to parse API response: {DefaultError}", defaultErrorMessage);
            return ApiResponse<T>.FailureResponse(
                response.IsSuccessStatusCode
                    ? "Failed to process server response."
                    : $"{defaultErrorMessage} (HTTP {(int)response.StatusCode})");
        }
    }

    public async Task<ApiResponse<PagedResponse<StudentFeeDto>>> GetStudentFeesPagedAsync(
        PagedRequest? request = null,
        Guid? branchId = null,
        Guid? classId = null,
        Guid? batchId = null,
        string? status = null,
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
            if (branchId.HasValue && branchId.Value != Guid.Empty)
                queryParams += $"&branchId={branchId.Value}";
            if (classId.HasValue && classId.Value != Guid.Empty)
                queryParams += $"&classId={classId.Value}";
            if (batchId.HasValue && batchId.Value != Guid.Empty)
                queryParams += $"&batchId={batchId.Value}";
            if (!string.IsNullOrWhiteSpace(status))
                queryParams += $"&status={Uri.EscapeDataString(status)}";

            var response = await _httpClient.GetAsync($"api/v1/orgadmin/fees{queryParams}", cancellationToken);
            return await ParseResponseAsync<PagedResponse<StudentFeeDto>>(response, "Failed to retrieve student fees", cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching paged student fees");
            return ApiResponse<PagedResponse<StudentFeeDto>>.FailureResponse(ex.Message);
        }
    }

    public async Task<ApiResponse<FeeStatisticsDto>> GetFeeStatisticsAsync(
        Guid? branchId = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var url = "api/v1/orgadmin/fees/stats";
            if (branchId.HasValue && branchId.Value != Guid.Empty)
            {
                url += $"?branchId={branchId.Value}";
            }

            var response = await _httpClient.GetAsync(url, cancellationToken);
            return await ParseResponseAsync<FeeStatisticsDto>(response, "Failed to retrieve fee statistics", cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching fee statistics");
            return ApiResponse<FeeStatisticsDto>.FailureResponse(ex.Message);
        }
    }

    public async Task<ApiResponse<StudentFeeDetailDto>> GetStudentFeeDetailsAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.GetAsync($"api/v1/orgadmin/fees/{id}", cancellationToken);
            return await ParseResponseAsync<StudentFeeDetailDto>(response, "Student fee record not found", cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching student fee details for {Id}", id);
            return ApiResponse<StudentFeeDetailDto>.FailureResponse(ex.Message);
        }
    }

    public async Task<ApiResponse<List<StudentLookupForFeeDto>>> GetStudentsLookupAsync(
        Guid? branchId = null,
        Guid? classId = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var url = "api/v1/orgadmin/fees/students-lookup";
            var query = new List<string>();
            if (branchId.HasValue && branchId.Value != Guid.Empty) query.Add($"branchId={branchId.Value}");
            if (classId.HasValue && classId.Value != Guid.Empty) query.Add($"classId={classId.Value}");
            if (query.Any()) url += "?" + string.Join("&", query);

            var response = await _httpClient.GetAsync(url, cancellationToken);
            return await ParseResponseAsync<List<StudentLookupForFeeDto>>(response, "Failed to retrieve student lookup list", cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching students for fee lookup");
            return ApiResponse<List<StudentLookupForFeeDto>>.FailureResponse(ex.Message);
        }
    }

    public async Task<ApiResponse<StudentFeeDto>> AssignFeeAsync(
        AssignStudentFeeRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/v1/orgadmin/fees/assign", request, cancellationToken);
            return await ParseResponseAsync<StudentFeeDto>(response, "Failed to assign fee", cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error assigning fee to student");
            return ApiResponse<StudentFeeDto>.FailureResponse(ex.Message);
        }
    }

    public async Task<ApiResponse<PaymentDto>> CollectFeeAsync(
        CollectPaymentRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/v1/orgadmin/fees/collect", request, cancellationToken);
            return await ParseResponseAsync<PaymentDto>(response, "Failed to record payment", cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error collecting fee");
            return ApiResponse<PaymentDto>.FailureResponse(ex.Message);
        }
    }

    public async Task<ApiResponse<PaymentDto>> GetPaymentReceiptAsync(
        Guid paymentId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.GetAsync($"api/v1/orgadmin/fees/payments/{paymentId}/receipt", cancellationToken);
            return await ParseResponseAsync<PaymentDto>(response, "Payment receipt not found", cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching payment receipt {Id}", paymentId);
            return ApiResponse<PaymentDto>.FailureResponse(ex.Message);
        }
    }
}
