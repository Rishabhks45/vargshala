using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.RegularExpressions;
using Vargshala.Application.Abstractions.CurrentUser;
using Vargshala.Contracts.Common;

namespace Vargshala.API.Controllers;

[ApiController]
[Route("api/v1/uploads")]
[Authorize]
public class UploadsController : ControllerBase
{
    private readonly IWebHostEnvironment _webHostEnvironment;
    private readonly ICurrentUser _currentUser;
    private readonly ILogger<UploadsController> _logger;

    private static readonly HashSet<string> AllowedFolders = new(StringComparer.OrdinalIgnoreCase)
    {
        "orglogo",
        "profile"
    };

    private const long MaxFileSize = 5 * 1024 * 1024; // 5 MB

    // Magic byte signatures for binary verification
    private static readonly byte[] JpegHeader = { 0xFF, 0xD8, 0xFF };
    private static readonly byte[] PngHeader = { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A };
    private static readonly byte[] RiffHeader = { 0x52, 0x49, 0x46, 0x46 }; // "RIFF"
    private static readonly byte[] WebpHeader = { 0x57, 0x45, 0x42, 0x50 }; // "WEBP"

    // Disallowed dangerous strings for SVG to prevent Stored XSS
    private static readonly Regex DangerousSvgRegex = new(
        @"(<\s*script|javascript\s*:|on\w+\s*=|data\s*:\s*text\/html|<\s*iframe|<\s*embed|<\s*object|<\s*foreignObject)",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    public UploadsController(
        IWebHostEnvironment webHostEnvironment,
        ICurrentUser currentUser,
        ILogger<UploadsController> logger)
    {
        _webHostEnvironment = webHostEnvironment;
        _currentUser = currentUser;
        _logger = logger;
    }

    [HttpPost("{folder}")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(MaxFileSize)]
    [RequestFormLimits(MultipartBodyLengthLimit = MaxFileSize)]
    public async Task<IActionResult> UploadFile(string folder, IFormFile? file)
    {
        // 1. Validate Target Folder Whitelist
        if (string.IsNullOrWhiteSpace(folder) || !AllowedFolders.Contains(folder))
        {
            return BadRequest(ApiResponse<FileUploadResponse>.FailureResponse(
                $"Invalid folder '{folder}'. Allowed folders: {string.Join(", ", AllowedFolders)}"));
        }

        // 2. Role-based Access Control
        if (string.Equals(folder, "orglogo", StringComparison.OrdinalIgnoreCase))
        {
            if (!_currentUser.IsSuperAdmin && _currentUser.UserRole != UserRole.OrganizationAdmin)
            {
                return StatusCode(StatusCodes.Status403Forbidden,
                    ApiResponse<FileUploadResponse>.FailureResponse("Only Organization Admins can upload organization logos."));
            }
        }
        else if (string.Equals(folder, "profile", StringComparison.OrdinalIgnoreCase))
        {
            if (!_currentUser.IsAuthenticated || _currentUser.UserId == Guid.Empty)
            {
                return Unauthorized(ApiResponse<FileUploadResponse>.FailureResponse("User is not authenticated."));
            }
        }

        // 3. Validate File Existence & Basic Metadata
        if (file == null || file.Length == 0)
        {
            return BadRequest(ApiResponse<FileUploadResponse>.FailureResponse("No file was uploaded."));
        }

        if (file.Length > MaxFileSize)
        {
            return BadRequest(ApiResponse<FileUploadResponse>.FailureResponse(
                $"File size exceeds maximum allowed limit of {MaxFileSize / (1024 * 1024)} MB."));
        }

        // 4. Sanitize and Validate Extension
        var rawExt = Path.GetExtension(file.FileName);
        if (string.IsNullOrWhiteSpace(rawExt) || rawExt.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0 || rawExt.Contains('\0'))
        {
            return BadRequest(ApiResponse<FileUploadResponse>.FailureResponse("Invalid or unsafe file name."));
        }

        var ext = rawExt.ToLowerInvariant();

        // 5. Read into memory buffer for Deep Inspection (Magic Bytes & Anti-Malware check)
        using var memoryStream = new MemoryStream();
        await file.CopyToAsync(memoryStream);
        var bytes = memoryStream.ToArray();

        if (bytes.Length == 0)
        {
            return BadRequest(ApiResponse<FileUploadResponse>.FailureResponse("Uploaded file contains no data."));
        }

        var (isValidFile, sanitizedExt, securityError) = ValidateFileSecurity(bytes, ext);
        if (!isValidFile)
        {
            _logger.LogWarning("Security check failed for file '{FileName}' by User '{UserId}': {Error}",
                file.FileName, _currentUser.UserId, securityError);
            return BadRequest(ApiResponse<FileUploadResponse>.FailureResponse(securityError));
        }

        // 6. Safe File Writing with Cryptographically Random GUID Name (Prevent Overwrite & Path Traversal)
        var webRoot = _webHostEnvironment.WebRootPath ?? Path.Combine(AppContext.BaseDirectory, "wwwroot");
        var uploadsDir = Path.GetFullPath(Path.Combine(webRoot, "uploads"));
        var targetFolder = Path.GetFullPath(Path.Combine(uploadsDir, folder.ToLowerInvariant()));

        // Double check target folder is strictly within uploadsDir
        if (!targetFolder.StartsWith(uploadsDir, StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest(ApiResponse<FileUploadResponse>.FailureResponse("Path traversal detected."));
        }

        if (!Directory.Exists(targetFolder))
        {
            Directory.CreateDirectory(targetFolder);
        }

        var safeFileName = $"{Guid.NewGuid():N}{sanitizedExt}";
        var finalFilePath = Path.Combine(targetFolder, safeFileName);

        await System.IO.File.WriteAllBytesAsync(finalFilePath, bytes);

        var relativeUrl = $"/uploads/{folder.ToLowerInvariant()}/{safeFileName}";
        var fullUrl = $"{Request.Scheme}://{Request.Host}{relativeUrl}";

        _logger.LogInformation("File '{SafeFileName}' securely uploaded to '{Folder}' by user '{UserId}'.",
            safeFileName, folder, _currentUser.UserId);

        return Ok(ApiResponse<FileUploadResponse>.SuccessResponse(new FileUploadResponse
        {
            RelativeUrl = relativeUrl,
            FileUrl = fullUrl,
            FileName = safeFileName
        }, "File uploaded successfully."));
    }

    [HttpDelete]
    public IActionResult DeleteFile([FromQuery] string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return BadRequest(ApiResponse<bool>.FailureResponse("Path is required."));
        }

        // Block directory traversal indicators
        if (path.Contains("..") || path.Contains(':') || path.Contains('\0'))
        {
            return BadRequest(ApiResponse<bool>.FailureResponse("Malicious path sequence detected."));
        }

        try
        {
            var webRoot = _webHostEnvironment.WebRootPath ?? Path.Combine(AppContext.BaseDirectory, "wwwroot");
            var uploadsDir = Path.GetFullPath(Path.Combine(webRoot, "uploads"));

            var cleanPath = path.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
            var fullPath = Path.GetFullPath(Path.Combine(webRoot, cleanPath));

            // Must strictly reside inside uploads directory
            if (!fullPath.StartsWith(uploadsDir, StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest(ApiResponse<bool>.FailureResponse("Access to specified file path is denied."));
            }

            // Role check: Only admin can delete org logos
            if (fullPath.Contains("orglogo", StringComparison.OrdinalIgnoreCase))
            {
                if (!_currentUser.IsSuperAdmin && _currentUser.UserRole != UserRole.OrganizationAdmin)
                {
                    return StatusCode(StatusCodes.Status403Forbidden,
                        ApiResponse<bool>.FailureResponse("Only Organization Admins can delete organization logos."));
                }
            }

            if (System.IO.File.Exists(fullPath))
            {
                System.IO.File.Delete(fullPath);
                return Ok(ApiResponse<bool>.SuccessResponse(true, "File deleted successfully."));
            }

            return Ok(ApiResponse<bool>.SuccessResponse(false, "File does not exist."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting file {Path}", path);
            return StatusCode(500, ApiResponse<bool>.FailureResponse($"Failed to delete file: {ex.Message}"));
        }
    }

    private static (bool IsValid, string SanitizedExt, string Error) ValidateFileSecurity(byte[] bytes, string ext)
    {
        switch (ext)
        {
            case ".jpg":
            case ".jpeg":
                if (bytes.Length < JpegHeader.Length || !bytes.Take(JpegHeader.Length).SequenceEqual(JpegHeader))
                {
                    return (false, string.Empty, "File signature does not match a valid JPEG image (magic bytes mismatch).");
                }
                return (true, ".jpg", string.Empty);

            case ".png":
                if (bytes.Length < PngHeader.Length || !bytes.Take(PngHeader.Length).SequenceEqual(PngHeader))
                {
                    return (false, string.Empty, "File signature does not match a valid PNG image (magic bytes mismatch).");
                }
                return (true, ".png", string.Empty);

            case ".webp":
                if (bytes.Length < 12 ||
                    !bytes.Take(4).SequenceEqual(RiffHeader) ||
                    !bytes.Skip(8).Take(4).SequenceEqual(WebpHeader))
                {
                    return (false, string.Empty, "File signature does not match a valid WEBP image (magic bytes mismatch).");
                }
                return (true, ".webp", string.Empty);

            case ".svg":
                var svgText = System.Text.Encoding.UTF8.GetString(bytes);
                if (!svgText.Contains("<svg", StringComparison.OrdinalIgnoreCase) ||
                    !svgText.Contains("</svg>", StringComparison.OrdinalIgnoreCase))
                {
                    return (false, string.Empty, "Invalid SVG structure. Must contain valid <svg> and </svg> tags.");
                }

                if (DangerousSvgRegex.IsMatch(svgText))
                {
                    return (false, string.Empty, "Security violation: SVG contains potentially dangerous script or embedded executable elements.");
                }
                return (true, ".svg", string.Empty);

            default:
                return (false, string.Empty, $"File extension '{ext}' is not allowed. Allowed: .jpg, .jpeg, .png, .webp, .svg");
        }
    }
}
