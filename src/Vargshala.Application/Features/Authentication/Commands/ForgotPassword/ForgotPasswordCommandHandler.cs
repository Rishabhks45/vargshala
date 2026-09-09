using System.Security.Cryptography;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Vargshala.Application.Abstractions.Email;
using Vargshala.Application.Features.Authentication.Infrastructure;
using Vargshala.Application.Features.Emails.Infrastructure;
using Vargshala.Contracts.Authentication;
using Vargshala.Contracts.Common;

namespace Vargshala.Application.Features.Authentication.Commands.ForgotPassword;

public class ForgotPasswordCommandHandler : IRequestHandler<ForgotPasswordCommand, ApiResponse<ForgotPasswordResponse>>
{
    private readonly IAuthRepository _authRepository;
    private readonly IEmailTemplateRepository _emailTemplateRepository;
    private readonly IEmailService _emailService;
    private readonly IConfiguration _configuration;
    private readonly ILogger<ForgotPasswordCommandHandler> _logger;

    public ForgotPasswordCommandHandler(
        IAuthRepository authRepository,
        IEmailTemplateRepository emailTemplateRepository,
        IEmailService emailService,
        IConfiguration configuration,
        ILogger<ForgotPasswordCommandHandler> logger)
    {
        _authRepository = authRepository;
        _emailTemplateRepository = emailTemplateRepository;
        _emailService = emailService;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<ApiResponse<ForgotPasswordResponse>> Handle(
        ForgotPasswordCommand command, 
        CancellationToken cancellationToken)
    {
        var email = command.Email.Trim().ToLowerInvariant();
        var user = await _authRepository.GetUserByEmailWithOrgAsync(email, cancellationToken);

        // Security best practice: Always return a generic success message to prevent account enumeration
        var genericResponse = new ForgotPasswordResponse
        {
            Success = true,
            Message = "If an account exists with this email address, password reset instructions have been sent."
        };

        if (user is null || !user.IsActive)
        {
            _logger.LogInformation("Password reset requested for non-existent or inactive user: {Email}", email);
            return ApiResponse<ForgotPasswordResponse>.SuccessResponse(genericResponse, genericResponse.Message);
        }

        // 1. Generate secure single-use URL-safe token (64 hex characters)
        var token = Convert.ToHexString(RandomNumberGenerator.GetBytes(32));

        // 2. Set token and expiry (30 minutes; previous token automatically invalidated)
        user.PasswordResetToken = token;
        user.PasswordResetTokenExpiresAt = DateTime.UtcNow.AddMinutes(30);

        await _authRepository.SaveChangesAsync(cancellationToken);

        // 3. Resolve client application base URL
        var clientBaseUrl = _configuration["ClientAppUrl"]
                         ?? _configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()?.FirstOrDefault()
                         ?? "http://localhost:5053";

        var resetUrl = $"{clientBaseUrl.TrimEnd('/')}/reset-password?email={Uri.EscapeDataString(user.Email ?? email)}&token={Uri.EscapeDataString(token)}";

        // 4. Retrieve email template from database (EmailTemplates table)
        var template = await _emailTemplateRepository.GetByCodeAsync("PASSWORD_RESET", cancellationToken)
                    ?? await _emailTemplateRepository.GetByCodeAsync("FORGOT_PASSWORD", cancellationToken);

        if (template is null || string.IsNullOrWhiteSpace(template.BodyHtml))
        {
            _logger.LogError("Email template 'PASSWORD_RESET' not found in database.");
            return ApiResponse<ForgotPasswordResponse>.SuccessResponse(genericResponse, genericResponse.Message);
        }

        var instituteName = user.Organization?.Name ?? "Vargshala";
        var recipientName = !string.IsNullOrWhiteSpace(user.FirstName) 
            ? $"{user.FirstName} {user.LastName}".Trim() 
            : "User";

        var emailSubject = template.Subject;
        var emailBody = template.BodyHtml;

        var placeholders = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["{{InstituteName}}"] = instituteName,
            ["{{RecipientName}}"] = recipientName,
            ["{{FirstName}}"] = user.FirstName ?? recipientName,
            ["{{LastName}}"] = user.LastName ?? string.Empty,
            ["{{Email}}"] = user.Email ?? email,
            ["{{ResetUrl}}"] = resetUrl,
            ["{{ResetLink}}"] = resetUrl,
            ["{{OtpCode}}"] = token.Length >= 6 ? token[..6].ToUpperInvariant() : token,
            ["{{ExpiryMinutes}}"] = "30",
            ["{{CurrentYear}}"] = DateTime.UtcNow.Year.ToString(),
            ["{{Year}}"] = DateTime.UtcNow.Year.ToString(),
            ["{{SupportEmail}}"] = _configuration["SupportEmail"] ?? "support@vargshala.com"
        };

        foreach (var (placeholder, value) in placeholders)
        {
            emailSubject = emailSubject.Replace(placeholder, value, StringComparison.OrdinalIgnoreCase);
            emailBody = emailBody.Replace(placeholder, value, StringComparison.OrdinalIgnoreCase);
        }

        try
        {
            await _emailService.SendEmailAsync(user.Email ?? email, emailSubject, emailBody, cancellationToken: cancellationToken);
            _logger.LogInformation("Password reset email sent to {Email} using template {TemplateCode} with token expiry {Expiry}", 
                email, template.Code, user.PasswordResetTokenExpiresAt);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to dispatch password reset email to {Email}", email);
        }

        return ApiResponse<ForgotPasswordResponse>.SuccessResponse(genericResponse, genericResponse.Message);
    }
}
