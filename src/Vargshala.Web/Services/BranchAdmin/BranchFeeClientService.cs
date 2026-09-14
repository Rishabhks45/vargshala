using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Vargshala.Contracts.Common;
using Vargshala.Contracts.Fees;

namespace Vargshala.Web.Services.BranchAdmin;

public interface IBranchFeeClientService
{
    Task<ApiResponse<PagedResponse<StudentFeeDto>>> GetStudentFeesPagedAsync(
        PagedRequest? request = null,
        Guid? classId = null,
        Guid? batchId = null,
        string? status = null,
        CancellationToken cancellationToken = default);

    Task<ApiResponse<FeeStatisticsDto>> GetFeeStatisticsAsync(
        CancellationToken cancellationToken = default);

    Task<ApiResponse<StudentFeeDetailDto>> GetStudentFeeDetailsAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<ApiResponse<List<StudentLookupForFeeDto>>> GetStudentsLookupAsync(
        Guid? classId = null,
        CancellationToken cancellationToken = default);

    Task<ApiResponse<StudentFeeDto>> AssignFeeAsync(
        AssignStudentFeeRequest request,
        CancellationToken cancellationToken = default);

    Task<ApiResponse<PaymentDto>> CollectFeeAsync(
        CollectPaymentRequest request,
        CancellationToken cancellationToken = default);

    Task<ApiResponse<PaymentDto>> GetPaymentReceiptAsync(
        Guid paymentId,
        CancellationToken cancellationToken = default);

    Task<byte[]?> GetPaymentReceiptPdfAsync(
        Guid paymentId,
        CancellationToken cancellationToken = default);

    Task<byte[]?> GetStudentFeeReceiptPdfAsync(
        Guid feeId,
        CancellationToken cancellationToken = default);
}

public class BranchFeeClientService : IBranchFeeClientService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<BranchFeeClientService> _logger;

    public BranchFeeClientService(
        IHttpClientFactory httpClientFactory,
        ILogger<BranchFeeClientService> logger)
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
                return ApiResponse<T>.FailureResponse(
                    !response.IsSuccessStatusCode
                        ? $"{defaultErrorMessage} (HTTP {(int)response.StatusCode})"
                        : "Received empty response from server.");
            }

            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var result = JsonSerializer.Deserialize<ApiResponse<T>>(contentStr, options);
            if (result != null)
            {
                return result;
            }

            return ApiResponse<T>.FailureResponse(
                response.IsSuccessStatusCode
                    ? "Failed to parse server response."
                    : $"{defaultErrorMessage} (HTTP {(int)response.StatusCode})");
        }
        catch (JsonException)
        {
            return ApiResponse<T>.FailureResponse(
                response.IsSuccessStatusCode
                    ? "Invalid data format received from server."
                    : $"{defaultErrorMessage} (HTTP {(int)response.StatusCode})");
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
            if (classId.HasValue && classId.Value != Guid.Empty)
                queryParams += $"&classId={classId.Value}";
            if (batchId.HasValue && batchId.Value != Guid.Empty)
                queryParams += $"&batchId={batchId.Value}";
            if (!string.IsNullOrWhiteSpace(status))
                queryParams += $"&status={Uri.EscapeDataString(status)}";

            var response = await _httpClient.GetAsync($"api/v1/branchadmin/fees{queryParams}", cancellationToken);
            return await ParseResponseAsync<PagedResponse<StudentFeeDto>>(response, "Failed to retrieve student fees.", cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching branch student fees");
            return ApiResponse<PagedResponse<StudentFeeDto>>.FailureResponse(ex.Message);
        }
    }

    public async Task<ApiResponse<FeeStatisticsDto>> GetFeeStatisticsAsync(
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.GetAsync("api/v1/branchadmin/fees/stats", cancellationToken);
            return await ParseResponseAsync<FeeStatisticsDto>(response, "Failed to retrieve fee statistics.", cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching branch fee statistics");
            return ApiResponse<FeeStatisticsDto>.FailureResponse(ex.Message);
        }
    }

    public async Task<ApiResponse<StudentFeeDetailDto>> GetStudentFeeDetailsAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.GetAsync($"api/v1/branchadmin/fees/{id}", cancellationToken);
            return await ParseResponseAsync<StudentFeeDetailDto>(response, "Fee record not found.", cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching branch student fee details for {Id}", id);
            return ApiResponse<StudentFeeDetailDto>.FailureResponse(ex.Message);
        }
    }

    public async Task<ApiResponse<List<StudentLookupForFeeDto>>> GetStudentsLookupAsync(
        Guid? classId = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var url = "api/v1/branchadmin/fees/students-lookup";
            if (classId.HasValue && classId.Value != Guid.Empty)
                url += $"?classId={classId.Value}";

            var response = await _httpClient.GetAsync(url, cancellationToken);
            return await ParseResponseAsync<List<StudentLookupForFeeDto>>(response, "Failed to retrieve students for fee assignment.", cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching branch students for fee lookup");
            return ApiResponse<List<StudentLookupForFeeDto>>.FailureResponse(ex.Message);
        }
    }

    public async Task<ApiResponse<StudentFeeDto>> AssignFeeAsync(
        AssignStudentFeeRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/v1/branchadmin/fees/assign", request, cancellationToken);
            return await ParseResponseAsync<StudentFeeDto>(response, "Failed to assign fee.", cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error assigning branch fee");
            return ApiResponse<StudentFeeDto>.FailureResponse(ex.Message);
        }
    }

    public async Task<ApiResponse<PaymentDto>> CollectFeeAsync(
        CollectPaymentRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/v1/branchadmin/fees/collect", request, cancellationToken);
            return await ParseResponseAsync<PaymentDto>(response, "Failed to collect payment.", cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error collecting branch payment");
            return ApiResponse<PaymentDto>.FailureResponse(ex.Message);
        }
    }

    public async Task<ApiResponse<PaymentDto>> GetPaymentReceiptAsync(
        Guid paymentId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.GetAsync($"api/v1/branchadmin/fees/payments/{paymentId}/receipt", cancellationToken);
            return await ParseResponseAsync<PaymentDto>(response, "Payment receipt not found.", cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching branch receipt for payment {PaymentId}", paymentId);
            return ApiResponse<PaymentDto>.FailureResponse(ex.Message);
        }
    }

    public async Task<byte[]?> GetPaymentReceiptPdfAsync(
        Guid paymentId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.GetAsync($"api/v1/branchadmin/fees/payments/{paymentId}/receipt/pdf", cancellationToken);
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadAsByteArrayAsync(cancellationToken);
            }
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching branch payment receipt PDF {Id}", paymentId);
            return null;
        }
    }

    public async Task<byte[]?> GetStudentFeeReceiptPdfAsync(
        Guid feeId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.GetAsync($"api/v1/branchadmin/fees/student-fees/{feeId}/receipt/pdf", cancellationToken);
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadAsByteArrayAsync(cancellationToken);
            }
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching branch student fee receipt PDF {FeeId}", feeId);
            return null;
        }
    }
}
