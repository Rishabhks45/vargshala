using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vargshala.Application.Abstractions.Email;
using Vargshala.Application.Features.Emails.Commands.ToggleStatus;
using Vargshala.Application.Features.Emails.Commands.UpdateEmailTemplate;
using Vargshala.Application.Features.Emails.Queries.GetEmailTemplateByCode;
using Vargshala.Application.Features.Emails.Queries.GetEmailTemplates;
using Vargshala.Contracts.Common;
using Vargshala.Contracts.EmailTemplates;

namespace Vargshala.API.Controllers;

[ApiController]
[Route("api/v1/emails")]
[Authorize(Roles = "SuperAdmin,BackOffice,1001,1002")]
public class EmailsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IEmailService _emailService;
    private readonly ILogger<EmailsController> _logger;

    public EmailsController(
        IMediator mediator,
        IEmailService emailService,
        ILogger<EmailsController> logger)
    {
        _mediator = mediator;
        _emailService = emailService;
        _logger = logger;
    }

    [HttpGet("templates")]
    public async Task<IActionResult> GetTemplates(
        [FromQuery] PagedRequest request,
        [FromQuery] EmailTemplateCategory? category = null,
        [FromQuery] bool? isActive = null,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(new GetEmailTemplatesQuery(request, category, isActive), cancellationToken);
        return Ok(result);
    }

    [HttpGet("templates/{code}")]
    public async Task<IActionResult> GetTemplateByCode(string code, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetEmailTemplateByCodeQuery(code), cancellationToken);
        if (!result.Success)
        {
            return NotFound(result);
        }
        return Ok(result);
    }

    [HttpPut("templates/{id:guid}")]
    public async Task<IActionResult> UpdateTemplate(Guid id, [FromBody] UpdateEmailTemplateRequest request, CancellationToken cancellationToken)
    {
        if (id != request.Id)
        {
            return BadRequest(ApiResponse<bool>.FailureResponse("Mismatched template ID in route and body."));
        }

        var result = await _mediator.Send(new UpdateEmailTemplateCommand(request), cancellationToken);
        if (!result.Success)
        {
            return BadRequest(result);
        }
        return Ok(result);
    }

    [HttpPatch("templates/{id:guid}/toggle-status")]
    public async Task<IActionResult> ToggleTemplateStatus(Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new ToggleEmailTemplateStatusCommand(id), cancellationToken);
        if (!result.Success)
        {
            return BadRequest(result);
        }
        return Ok(result);
    }

    [HttpPost("send")]
    public async Task<IActionResult> SendEmail([FromBody] SendCustomEmailRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.To) || string.IsNullOrWhiteSpace(request.Subject) || string.IsNullOrWhiteSpace(request.HtmlBody))
        {
            return BadRequest(ApiResponse<bool>.FailureResponse("Recipient email, subject, and HTML content are required."));
        }

        var success = await _emailService.SendEmailAsync(
            request.To,
            request.Subject,
            request.HtmlBody,
            request.From,
            cancellationToken);

        if (!success)
        {
            return BadRequest(ApiResponse<bool>.FailureResponse("Failed to dispatch email via Resend. Check server logs and API key configuration."));
        }

        return Ok(ApiResponse<bool>.SuccessResponse(true, $"Email dispatched successfully via Resend to {request.To}."));
    }

    [HttpPost("test")]
    public async Task<IActionResult> SendTestEmail([FromBody] SendTestEmailRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.RecipientEmail))
        {
            return BadRequest(ApiResponse<bool>.FailureResponse("Please provide a valid recipient email address."));
        }

        var testHtml = $@"
<!DOCTYPE html>
<html>
<head><meta charset='utf-8'></head>
<body style='font-family: sans-serif; background-color: #f8fafc; padding: 24px;'>
    <div style='max-width: 500px; margin: 0 auto; background: #ffffff; border-radius: 12px; padding: 24px; border: 1px solid #e2e8f0;'>
        <div style='background: #004D40; color: #ffffff; padding: 12px 16px; border-radius: 8px; font-weight: bold;'>
            ✉️ Vargshala &bull; Resend Live Test
        </div>
        <div style='padding: 16px 0;'>
            <p style='font-size: 14px; color: #334155;'>Hello,</p>
            <p style='font-size: 14px; color: #334155;'>This is a verification email dispatched via <strong>Resend</strong> from your Vargshala Educational Institute Management SaaS platform.</p>
            <div style='background: #f0fdfa; border: 1px solid #ccfbf1; padding: 12px; border-radius: 8px; color: #0f766e; font-size: 12px;'>
                ✅ <strong>Resend Configuration Status:</strong> Active &amp; Verified.<br/>
                ⏰ <strong>Dispatched At:</strong> {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC
            </div>
        </div>
        <div style='border-top: 1px solid #f1f5f9; padding-top: 12px; font-size: 11px; color: #94a3b8; text-align: center;'>
            Sent by Vargshala Cloud Infrastructure via Resend API
        </div>
    </div>
</body>
</html>";

        var success = await _emailService.SendEmailAsync(
            request.RecipientEmail,
            "Vargshala Test Email — Resend Verified",
            testHtml,
            null,
            cancellationToken);

        if (!success)
        {
            return BadRequest(ApiResponse<bool>.FailureResponse("Could not send test email via Resend. Check that RESEND_API_KEY is configured and recipient is permitted."));
        }

        return Ok(ApiResponse<bool>.SuccessResponse(true, $"Test email successfully dispatched to {request.RecipientEmail} via Resend."));
    }
}
