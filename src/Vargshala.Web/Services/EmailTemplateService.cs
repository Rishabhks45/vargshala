using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using Vargshala.Contracts.Common;
using Vargshala.Contracts.EmailTemplates;

namespace Vargshala.Web.Services;

public class EmailTemplateService : IEmailTemplateService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<EmailTemplateService> _logger;

    public EmailTemplateService(
        IHttpClientFactory httpClientFactory,
        ILogger<EmailTemplateService> logger)
    {
        _httpClient = httpClientFactory.CreateClient("VargshalaApi");
        _logger = logger;
    }

    public async Task<ApiResponse<PagedResponse<EmailTemplateDto>>> GetTemplatesPagedAsync(
        PagedRequest? request = null,
        EmailTemplateCategory? category = null,
        bool? isActive = null,
        CancellationToken cancellationToken = default)
    {
        var req = request ?? new PagedRequest();
        try
        {
            var queryParams = new List<string>
            {
                $"PageNumber={req.PageNumber}",
                $"PageSize={req.PageSize}"
            };

            if (!string.IsNullOrWhiteSpace(req.Search))
                queryParams.Add($"Search={Uri.EscapeDataString(req.Search)}");

            if (!string.IsNullOrWhiteSpace(req.SortBy))
                queryParams.Add($"SortBy={Uri.EscapeDataString(req.SortBy)}");

            if (!string.IsNullOrWhiteSpace(req.SortDirection))
                queryParams.Add($"SortDirection={Uri.EscapeDataString(req.SortDirection)}");

            if (category.HasValue)
                queryParams.Add($"category={(int)category.Value}");

            if (isActive.HasValue)
                queryParams.Add($"isActive={isActive.Value}");

            var url = "api/v1/emails/templates?" + string.Join("&", queryParams);

            var response = await _httpClient.GetFromJsonAsync<ApiResponse<PagedResponse<EmailTemplateDto>>>(url, cancellationToken);
            if (response != null)
            {
                return response;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load paged email templates from API.");
            return ApiResponse<PagedResponse<EmailTemplateDto>>.FailureResponse($"Failed to load email templates: {ex.Message}");
        }

        return ApiResponse<PagedResponse<EmailTemplateDto>>.SuccessResponse(new PagedResponse<EmailTemplateDto> { PageNumber = req.PageNumber, PageSize = req.PageSize });
    }

    public async Task<List<EmailTemplateDto>> GetAllTemplatesAsync(CancellationToken cancellationToken = default)
    {
        var paged = await GetTemplatesPagedAsync(new PagedRequest { PageSize = 100 }, cancellationToken: cancellationToken);
        return paged.Data?.Items ?? new List<EmailTemplateDto>();
    }

    public async Task<EmailTemplateDto?> GetTemplateByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var all = await GetAllTemplatesAsync(cancellationToken);
        return all.FirstOrDefault(t => t.Id == id);
    }

    public async Task<ApiResponse<EmailTemplateDto>> CreateTemplateAsync(CreateEmailTemplateRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/v1/emails/templates", request, cancellationToken);
            var result = await response.Content.ReadFromJsonAsync<ApiResponse<EmailTemplateDto>>(cancellationToken);
            if (result != null) return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create email template via API.");
            return ApiResponse<EmailTemplateDto>.FailureResponse($"API error: {ex.Message}");
        }

        return ApiResponse<EmailTemplateDto>.FailureResponse("Failed to create email template.");
    }

    public async Task<ApiResponse<EmailTemplateDto>> UpdateTemplateAsync(UpdateEmailTemplateRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.PutAsJsonAsync($"api/v1/emails/templates/{request.Id}", request, cancellationToken);
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<ApiResponse<bool>>(cancellationToken);
                if (result != null && result.Success)
                {
                    return ApiResponse<EmailTemplateDto>.SuccessResponse(
                        new EmailTemplateDto { Id = request.Id, Name = request.Name }, 
                        result.Message ?? "Email template updated successfully.");
                }
                return ApiResponse<EmailTemplateDto>.FailureResponse(result?.Message ?? "Failed to update email template.");
            }
            
            var err = await response.Content.ReadFromJsonAsync<ApiResponse<bool>>(cancellationToken);
            return ApiResponse<EmailTemplateDto>.FailureResponse(err?.Message ?? "Failed to update email template.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "API update failed for template {Id}", request.Id);
            return ApiResponse<EmailTemplateDto>.FailureResponse($"API update error: {ex.Message}");
        }
    }

    public async Task<ApiResponse<bool>> ToggleTemplateStatusAsync(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.PatchAsync($"api/v1/emails/templates/{id}/toggle-status", null, cancellationToken);
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<ApiResponse<bool>>(cancellationToken);
                if (result != null) return result;
            }
            var err = await response.Content.ReadFromJsonAsync<ApiResponse<bool>>(cancellationToken);
            return err ?? ApiResponse<bool>.FailureResponse("Failed to toggle template status.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "API toggle failed for template {Id}", id);
            return ApiResponse<bool>.FailureResponse($"API toggle error: {ex.Message}");
        }
    }

    public async Task<ApiResponse<bool>> SendTestEmailAsync(SendTestEmailRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.RecipientEmail) || !request.RecipientEmail.Contains('@'))
        {
            return ApiResponse<bool>.FailureResponse("Please enter a valid recipient email address.");
        }

        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/v1/emails/test", request, cancellationToken);
            var result = await response.Content.ReadFromJsonAsync<ApiResponse<bool>>(cancellationToken);
            if (result != null) return result;

            return ApiResponse<bool>.FailureResponse($"API returned status code {(int)response.StatusCode}.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "API test dispatch call failed.");
            return ApiResponse<bool>.FailureResponse($"Could not contact email API: {ex.Message}");
        }
    }
}
