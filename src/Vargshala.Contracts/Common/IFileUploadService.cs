using Microsoft.AspNetCore.Components.Forms;

namespace Vargshala.Contracts.Common;

public class FileValidationResult
{
    public bool IsValid { get; set; }
    public string ErrorMessage { get; set; } = string.Empty;
    public string Base64Data { get; set; } = string.Empty;
}

public class FileUploadResponse
{
    public string RelativeUrl { get; set; } = string.Empty;
    public string FileUrl { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
}

public interface IFileUploadService
{
    Task<string> HandleFileUploadAsync(IBrowserFile file, string subFolder = "");
    Task<string> HandleFileUploadInByteAsync(byte[] file, string subFolder = "profile", string extension = ".jpg");
    Task<FileValidationResult> ValidateFileAsync(IBrowserFile file);
    Task<bool> DeleteFileAsync(string relativePath);
    bool FileExists(string relativePath);
}
