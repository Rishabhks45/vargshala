using Microsoft.AspNetCore.Components.Forms;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace Vargshala.Contracts.Common;

public class FileUploadService : IFileUploadService
{
    private readonly HttpClient _httpClient;
    private const long MaxFileSize = 5 * 1024 * 1024; // 5 MB
    private static readonly string[] AllowedTypes = { ".jpg", ".jpeg", ".png", ".svg", ".webp" };

    public FileUploadService(IHttpClientFactory httpClientFactory)
    {
        _httpClient = httpClientFactory.CreateClient("VargshalaApi");
    }

    public async Task<FileValidationResult> ValidateFileAsync(IBrowserFile file)
    {
        var result = new FileValidationResult();

        try
        {
            if (file == null)
            {
                result.IsValid = false;
                result.ErrorMessage = "No file selected.";
                return result;
            }

            var extension = Path.GetExtension(file.Name).ToLowerInvariant();
            if (!AllowedTypes.Contains(extension))
            {
                result.IsValid = false;
                result.ErrorMessage = $"File type {extension} not allowed. Allowed: {string.Join(", ", AllowedTypes)}";
                return result;
            }

            if (file.Size > MaxFileSize)
            {
                result.IsValid = false;
                result.ErrorMessage = $"File size exceeds {MaxFileSize / (1024 * 1024)} MB";
                return result;
            }

            if (extension == ".svg")
            {
                using Stream svgStream = file.OpenReadStream(MaxFileSize);
                using var svgMemoryStream = new MemoryStream();
                await svgStream.CopyToAsync(svgMemoryStream);
                svgMemoryStream.Position = 0;

                var svgContent = System.Text.Encoding.UTF8.GetString(svgMemoryStream.ToArray());
                if (!svgContent.Contains("<svg") || !svgContent.Contains("</svg>"))
                {
                    result.IsValid = false;
                    result.ErrorMessage = "Invalid SVG file format.";
                    return result;
                }
            }

            result.IsValid = true;
        }
        catch (Exception ex)
        {
            result.IsValid = false;
            result.ErrorMessage = $"File validation failed: {ex.Message}";
        }

        return result;
    }

    public async Task<string> HandleFileUploadAsync(IBrowserFile file, string subFolder = "orglogo")
    {
        if (file == null) return string.Empty;

        var validation = await ValidateFileAsync(file);
        if (!validation.IsValid)
        {
            throw new InvalidOperationException(validation.ErrorMessage);
        }

        using var content = new MultipartFormDataContent();
        using var stream = file.OpenReadStream(MaxFileSize);
        var fileContent = new StreamContent(stream);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue(string.IsNullOrWhiteSpace(file.ContentType) ? "application/octet-stream" : file.ContentType);

        content.Add(fileContent, "file", file.Name);

        var folder = string.IsNullOrWhiteSpace(subFolder) ? "orglogo" : subFolder.ToLowerInvariant();
        var response = await _httpClient.PostAsync($"api/v1/uploads/{folder}", content);

        if (!response.IsSuccessStatusCode)
        {
            var err = await response.Content.ReadAsStringAsync();
            throw new Exception($"Failed to upload file to API (Status {response.StatusCode}): {err}");
        }

        var result = await response.Content.ReadFromJsonAsync<ApiResponse<FileUploadResponse>>();
        if (result?.Success == true && result.Data != null)
        {
            return result.Data.RelativeUrl;
        }

        throw new Exception(result?.Message ?? "Failed to upload file to API.");
    }

    public async Task<string> HandleFileUploadInByteAsync(byte[] file, string subFolder = "profile", string extension = ".jpg")
    {
        if (file == null || file.Length == 0) return string.Empty;
        if (file.Length > MaxFileSize)
        {
            throw new InvalidOperationException($"File size exceeds maximum allowed size of {MaxFileSize / (1024 * 1024)} MB.");
        }

        var ext = extension.StartsWith(".") ? extension.ToLowerInvariant() : $".{extension.ToLowerInvariant()}";
        var mimeType = ext switch
        {
            ".png" => "image/png",
            ".webp" => "image/webp",
            _ => "image/jpeg"
        };

        using var content = new MultipartFormDataContent();
        var fileContent = new ByteArrayContent(file);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue(mimeType);

        var fileName = $"{Guid.NewGuid():N}{ext}";
        content.Add(fileContent, "file", fileName);

        var folder = string.IsNullOrWhiteSpace(subFolder) ? "profile" : subFolder.ToLowerInvariant();
        var response = await _httpClient.PostAsync($"api/v1/uploads/{folder}", content);

        if (!response.IsSuccessStatusCode)
        {
            var err = await response.Content.ReadAsStringAsync();
            throw new Exception($"Failed to upload file to API (Status {response.StatusCode}): {err}");
        }

        var result = await response.Content.ReadFromJsonAsync<ApiResponse<FileUploadResponse>>();
        if (result?.Success == true && result.Data != null)
        {
            return result.Data.RelativeUrl;
        }

        throw new Exception(result?.Message ?? "Failed to upload file to API.");
    }

    public async Task<bool> DeleteFileAsync(string relativePath)
    {
        if (string.IsNullOrWhiteSpace(relativePath)) return false;

        try
        {
            var encodedPath = Uri.EscapeDataString(relativePath);
            var response = await _httpClient.DeleteAsync($"api/v1/uploads?path={encodedPath}");
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    public bool FileExists(string relativePath)
    {
        return !string.IsNullOrWhiteSpace(relativePath);
    }
}
