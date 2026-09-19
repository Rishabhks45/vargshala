using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Vargshala.Application.Abstractions.Storage;
using Vargshala.Contracts.Common;
using Vargshala.Contracts.Messages;
using Vargshala.Infrastructure.Settings;

namespace Vargshala.Infrastructure.Services;

public class SupabaseStorageService : IStorageService
{
    private readonly HttpClient _httpClient;
    private readonly SupabaseStorageOptions _options;
    private readonly ILogger<SupabaseStorageService> _logger;

    public SupabaseStorageService(
        HttpClient httpClient,
        IOptions<SupabaseStorageOptions> options,
        ILogger<SupabaseStorageService> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<ApiResponse<MessageAttachmentUploadResponse>> UploadFileAsync(
        Stream fileStream,
        string fileName,
        string contentType,
        string folderPath,
        CancellationToken cancellationToken = default)
    {
        var apiKey = GetEffectiveApiKey();
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            _logger.LogError("Supabase API Key is not configured in appsettings or environment variables.");
            return ApiResponse<MessageAttachmentUploadResponse>.FailureResponse(
                "Supabase Storage is not configured. Please configure the Supabase API Key.");
        }

        try
        {
            var sanitizedFileName = SanitizeFileName(fileName);
            var uniqueFileName = $"{Guid.NewGuid():N}_{sanitizedFileName}";
            var cleanFolder = folderPath.Trim('/').Replace('\\', '/');
            var baseFolder = _options.BaseFolder.Trim('/').Replace('\\', '/');
            
            var storagePath = string.IsNullOrWhiteSpace(cleanFolder)
                ? $"{baseFolder}/{uniqueFileName}"
                : $"{baseFolder}/{cleanFolder}/{uniqueFileName}";

            var bucket = _options.Bucket.Trim('/');
            var baseUrl = _options.Url.TrimEnd('/');
            var uploadUrl = $"{baseUrl}/storage/v1/object/{bucket}/{storagePath}";

            using var request = new HttpRequestMessage(HttpMethod.Post, uploadUrl);
            request.Headers.Add("apikey", apiKey);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
            request.Headers.Add("x-upsert", "true");

            using var content = new StreamContent(fileStream);
            content.Headers.ContentType = new MediaTypeHeaderValue(string.IsNullOrWhiteSpace(contentType) ? "application/octet-stream" : contentType);
            request.Content = content;

            var response = await _httpClient.SendAsync(request, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError("Supabase Storage upload failed with status {StatusCode}: {Error}", response.StatusCode, errorBody);
                return ApiResponse<MessageAttachmentUploadResponse>.FailureResponse(
                    $"Failed to upload file to storage: {response.ReasonPhrase} ({errorBody})");
            }

            var publicUrl = $"{baseUrl}/storage/v1/object/public/{bucket}/{storagePath}";
            long fileSize = fileStream.CanSeek ? fileStream.Length : 0;

            var result = new MessageAttachmentUploadResponse
            {
                FileName = fileName,
                ContentType = contentType,
                FileSize = fileSize,
                FileUrl = publicUrl,
                StorageKey = storagePath
            };

            _logger.LogInformation("File '{FileName}' uploaded successfully to Supabase Storage at '{StoragePath}'.", fileName, storagePath);
            return ApiResponse<MessageAttachmentUploadResponse>.SuccessResponse(result, "File uploaded successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error uploading file '{FileName}' to Supabase Storage", fileName);
            return ApiResponse<MessageAttachmentUploadResponse>.FailureResponse($"Storage upload error: {ex.Message}");
        }
    }

    public async Task<ApiResponse<bool>> DeleteFileAsync(
        string storageKey,
        CancellationToken cancellationToken = default)
    {
        var apiKey = GetEffectiveApiKey();
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            return ApiResponse<bool>.FailureResponse("Supabase Storage is not configured.");
        }

        try
        {
            var bucket = _options.Bucket.Trim('/');
            var baseUrl = _options.Url.TrimEnd('/');
            var cleanKey = storageKey.TrimStart('/');
            var deleteUrl = $"{baseUrl}/storage/v1/object/{bucket}/{cleanKey}";

            using var request = new HttpRequestMessage(HttpMethod.Delete, deleteUrl);
            request.Headers.Add("apikey", apiKey);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

            var response = await _httpClient.SendAsync(request, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogWarning("Supabase Storage delete failed for '{StorageKey}': {Error}", storageKey, errorBody);
                return ApiResponse<bool>.FailureResponse($"Failed to delete file from storage: {response.ReasonPhrase}");
            }

            return ApiResponse<bool>.SuccessResponse(true, "File deleted successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error deleting file '{StorageKey}' from Supabase Storage", storageKey);
            return ApiResponse<bool>.FailureResponse($"Storage deletion error: {ex.Message}");
        }
    }

    private string GetEffectiveApiKey()
    {
        if (!string.IsNullOrWhiteSpace(_options.ApiKey))
        {
            return _options.ApiKey;
        }

        var envKey = Environment.GetEnvironmentVariable("SUPABASE_API_KEY") 
            ?? Environment.GetEnvironmentVariable("SUPABASE_SERVICE_ROLE_KEY")
            ?? Environment.GetEnvironmentVariable("SUPABASE_KEY");

        return envKey ?? string.Empty;
    }

    private static string SanitizeFileName(string fileName)
    {
        var nameWithoutExt = Path.GetFileNameWithoutExtension(fileName);
        var ext = Path.GetExtension(fileName);

        var sanitized = new string(nameWithoutExt
            .Where(c => char.IsLetterOrDigit(c) || c == '-' || c == '_')
            .ToArray());

        if (string.IsNullOrWhiteSpace(sanitized))
        {
            sanitized = "file";
        }

        if (sanitized.Length > 50)
        {
            sanitized = sanitized[..50];
        }

        return $"{sanitized}{ext.ToLowerInvariant()}";
    }
}
