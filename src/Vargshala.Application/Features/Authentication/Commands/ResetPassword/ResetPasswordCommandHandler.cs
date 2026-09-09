using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Vargshala.Application.Abstractions.Authentication;
using Vargshala.Application.Features.Authentication.Infrastructure;
using Vargshala.Application.Settings;
using Vargshala.Contracts.Authentication;
using Vargshala.Contracts.Common;

namespace Vargshala.Application.Features.Authentication.Commands.ResetPassword;

public class ResetPasswordCommandHandler : IRequestHandler<ResetPasswordCommand, ApiResponse<ResetPasswordResponse>>
{
    private readonly IAuthRepository _authRepository;
    private readonly IEncryptionService _encryptionService;
    private readonly EncryptionSettings _encryptionSettings;
    private readonly ILogger<ResetPasswordCommandHandler> _logger;

    public ResetPasswordCommandHandler(
        IAuthRepository authRepository,
        IEncryptionService encryptionService,
        IOptions<EncryptionSettings> encryptionOptions,
        ILogger<ResetPasswordCommandHandler> logger)
    {
        _authRepository = authRepository;
        _encryptionService = encryptionService;
        _encryptionSettings = encryptionOptions.Value;
        _logger = logger;
    }

    public async Task<ApiResponse<ResetPasswordResponse>> Handle(
        ResetPasswordCommand command,
        CancellationToken cancellationToken)
    {
        var request = command.Request;

        if (string.IsNullOrWhiteSpace(request.Token))
        {
            return ApiResponse<ResetPasswordResponse>.FailureResponse("Reset token is required.");
        }

        if (string.IsNullOrWhiteSpace(request.NewPassword))
        {
            return ApiResponse<ResetPasswordResponse>.FailureResponse("New password is required.");
        }

        if (request.NewPassword != request.ConfirmPassword)
        {
            return ApiResponse<ResetPasswordResponse>.FailureResponse("Passwords do not match.");
        }

        // 1. Fetch user by reset token
        var user = await _authRepository.GetUserByResetTokenAsync(request.Token.Trim(), cancellationToken);

        // Fallback: If not found by token, or if token was tampered with, attempt lookup by email to give a safe, accurate message
        if (user is null)
        {
            _logger.LogWarning("Invalid reset token attempted.");
            return ApiResponse<ResetPasswordResponse>.FailureResponse(
                "This password reset link is invalid or has already been used. Please request a new password reset link.");
        }

        // Verify email match if email was supplied
        if (!string.IsNullOrWhiteSpace(request.Email) && 
            !string.Equals(user.Email, request.Email.Trim(), StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogWarning("Password reset token email mismatch for user {Email}", user.Email);
            return ApiResponse<ResetPasswordResponse>.FailureResponse(
                "This password reset link does not match the provided email address.");
        }

        // 2. Validate token expiry
        if (!user.PasswordResetTokenExpiresAt.HasValue || user.PasswordResetTokenExpiresAt.Value <= DateTime.UtcNow)
        {
            _logger.LogWarning("Expired password reset token attempted for user {Email}", user.Email);
            
            // Clear expired token to keep DB tidy
            user.PasswordResetToken = null;
            user.PasswordResetTokenExpiresAt = null;
            await _authRepository.SaveChangesAsync(cancellationToken);

            return ApiResponse<ResetPasswordResponse>.FailureResponse(
                "This password reset link has expired. Please request a new password reset link.");
        }

        // 3. Update password hash
        user.PasswordHash = _encryptionService.Encrypt(request.NewPassword, _encryptionSettings.MasterKey);

        // 4. CRITICAL SINGLE-USE REQUIREMENT: Token expires immediately after one use
        user.PasswordResetToken = null;
        user.PasswordResetTokenExpiresAt = null;
        user.UpdatedAt = DateTime.UtcNow;

        await _authRepository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Password successfully reset for user {Email}. Reset token invalidated.", user.Email);

        return ApiResponse<ResetPasswordResponse>.SuccessResponse(
            new ResetPasswordResponse
            {
                Success = true,
                Message = "Your password has been reset successfully. You can now log in with your new password."
            },
            "Password reset successful.");
    }
}
