using Vargshala.Contracts.Common;
using Vargshala.Contracts.EmailTemplates;

namespace Vargshala.Web.Services;

public class EmailTemplateService : IEmailTemplateService
{
    private readonly List<EmailTemplateDto> _templates = new()
    {
        new EmailTemplateDto
        {
            Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
            Code = "WELCOME_ONBOARDING",
            Name = "Institute Onboarding Welcome",
            Category = "Onboarding",
            Subject = "Welcome to Vargshala, {{InstituteName}}! 🎉",
            Description = "Triggered immediately when a new coaching institute is provisioned or onboarded.",
            AvailablePlaceholders = new List<string> { "{{InstituteName}}", "{{OwnerName}}", "{{TenantCode}}", "{{LoginUrl}}", "{{SupportEmail}}" },
            IsActive = true,
            UpdatedAt = DateTime.UtcNow.AddDays(-2),
            UpdatedBy = "System",
            BodyHtml = @"<!DOCTYPE html>
<html>
<head>
    <meta charset=""utf-8"">
    <style>
        body { font-family: 'Inter', -apple-system, sans-serif; background-color: #f8fafc; color: #1e293b; margin: 0; padding: 24px; }
        .container { max-width: 580px; margin: 0 auto; background: #ffffff; border-radius: 16px; border: 1px solid #e2e8f0; overflow: hidden; box-shadow: 0 4px 12px rgba(0,0,0,0.05); }
        .header { background: linear-gradient(135deg, #004D40 0%, #00796B 100%); padding: 32px 28px; text-align: center; color: #ffffff; }
        .header h1 { margin: 0 0 8px 0; font-size: 22px; font-weight: 800; letter-spacing: -0.5px; }
        .header p { margin: 0; font-size: 13px; opacity: 0.9; }
        .body { padding: 32px 28px; }
        .welcome-badge { display: inline-block; padding: 4px 12px; background: #E0F2F1; color: #004D40; border-radius: 999px; font-size: 11px; font-weight: 700; margin-bottom: 16px; }
        .credentials-box { background: #f1f5f9; border-radius: 12px; padding: 16px 20px; margin: 20px 0; border: 1px solid #cbd5e1; }
        .cred-row { display: flex; justify-content: space-between; margin-bottom: 8px; font-size: 13px; }
        .cred-row:last-child { margin-bottom: 0; }
        .cred-label { color: #64748b; font-weight: 600; }
        .cred-val { font-weight: 700; color: #0f172a; font-family: monospace; }
        .btn { display: block; text-align: center; background: linear-gradient(135deg, #009488 0%, #00796B 100%); color: #ffffff; text-decoration: none; font-weight: 700; font-size: 14px; padding: 14px 28px; border-radius: 12px; margin: 28px 0 16px 0; }
        .footer { padding: 20px 28px; background: #f8fafc; text-align: center; font-size: 11px; color: #94a3b8; border-top: 1px solid #f1f5f9; }
    </style>
</head>
<body>
    <div class=""container"">
        <div class=""header"">
            <h1>Vargshala EdTech</h1>
            <p>Next-Gen Institute & Coaching Operating System</p>
        </div>
        <div class=""body"">
            <span class=""welcome-badge"">NEW TENANT PROVISIONED</span>
            <h2 style=""margin: 0 0 12px 0; font-size: 18px; color: #0f172a;"">Welcome aboard, {{OwnerName}}!</h2>
            <p style=""font-size: 14px; line-height: 1.6; color: #475569; margin: 0 0 16px 0;"">
                Your dedicated workspace for <strong>{{InstituteName}}</strong> is live and fully isolated in our cloud. Here are your organization details:
            </p>
            <div class=""credentials-box"">
                <div class=""cred-row""><span class=""cred-label"">Institute:</span><span class=""cred-val"">{{InstituteName}}</span></div>
                <div class=""cred-row""><span class=""cred-label"">Tenant Code:</span><span class=""cred-val"">{{TenantCode}}</span></div>
                <div class=""cred-row""><span class=""cred-label"">Login Portal:</span><span class=""cred-val"">{{LoginUrl}}</span></div>
            </div>
            <a href=""{{LoginUrl}}"" class=""btn"">Access Institute Dashboard →</a>
            <p style=""font-size: 12px; color: #64748b; line-height: 1.5; margin: 0;"">
                Need assistance setting up student batches or biometric attendance? Reply to this email or reach us at {{SupportEmail}}.
            </p>
        </div>
        <div class=""footer"">
            &copy; 2026 Vargshala Educational Technologies Pvt Ltd. All rights reserved.
        </div>
    </div>
</body>
</html>"
        },
        new EmailTemplateDto
        {
            Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
            Code = "RESET_PASSWORD",
            Name = "Password Reset Request",
            Category = "Auth & Security",
            Subject = "Reset your Vargshala password - Action Required",
            Description = "Dispatched when an administrator or staff member requests a secure password recovery link.",
            AvailablePlaceholders = new List<string> { "{{RecipientName}}", "{{ResetUrl}}", "{{ExpiryMinutes}}", "{{SupportEmail}}" },
            IsActive = true,
            UpdatedAt = DateTime.UtcNow.AddDays(-5),
            UpdatedBy = "SuperAdmin",
            BodyHtml = @"<!DOCTYPE html>
<html>
<head>
    <meta charset=""utf-8"">
    <style>
        body { font-family: 'Inter', -apple-system, sans-serif; background-color: #f8fafc; color: #1e293b; margin: 0; padding: 24px; }
        .container { max-width: 540px; margin: 0 auto; background: #ffffff; border-radius: 16px; border: 1px solid #e2e8f0; overflow: hidden; }
        .header { background: #004D40; padding: 24px; text-align: center; color: #ffffff; }
        .header h1 { margin: 0; font-size: 18px; font-weight: 800; }
        .body { padding: 32px 28px; }
        .btn { display: block; text-align: center; background: #009488; color: #ffffff; text-decoration: none; font-weight: 700; font-size: 14px; padding: 14px 28px; border-radius: 10px; margin: 24px 0; }
        .warning-box { background: #fffbeb; border-radius: 10px; padding: 12px 16px; font-size: 12px; color: #b45309; border: 1px solid #fef3c7; }
        .footer { padding: 16px; background: #f8fafc; text-align: center; font-size: 11px; color: #94a3b8; }
    </style>
</head>
<body>
    <div class=""container"">
        <div class=""header"">
            <h1>Security &amp; Account Protection</h1>
        </div>
        <div class=""body"">
            <h2 style=""margin: 0 0 12px 0; font-size: 17px;"">Hello, {{RecipientName}}</h2>
            <p style=""font-size: 13px; line-height: 1.6; color: #475569;"">
                We received a request to reset the password associated with your Vargshala account. Click the button below to choose a new password:
            </p>
            <a href=""{{ResetUrl}}"" class=""btn"">Reset My Password →</a>
            <div class=""warning-box"">
                ⚠️ This link will expire in <strong>{{ExpiryMinutes}} minutes</strong>. If you did not request this change, please ignore this email or notify security at {{SupportEmail}}.
            </div>
        </div>
        <div class=""footer"">
            Vargshala Multi-tenant SaaS &bull; Automated Security Dispatcher
        </div>
    </div>
</body>
</html>"
        },
        new EmailTemplateDto
        {
            Id = Guid.Parse("33333333-3333-3333-3333-333333333333"),
            Code = "INVOICE_RECEIPT",
            Name = "Subscription Payment Receipt",
            Category = "Billing",
            Subject = "Payment Receipt for {{InstituteName}} - Invoice #{{InvoiceNumber}}",
            Description = "Issued automatically after successful recurring billing or plan upgrade checkout.",
            AvailablePlaceholders = new List<string> { "{{InstituteName}}", "{{InvoiceNumber}}", "{{PlanName}}", "{{AmountPaid}}", "{{NextBillingDate}}", "{{InvoiceDownloadUrl}}" },
            IsActive = true,
            UpdatedAt = DateTime.UtcNow.AddDays(-1),
            UpdatedBy = "BillingBot",
            BodyHtml = @"<!DOCTYPE html>
<html>
<head>
    <meta charset=""utf-8"">
    <style>
        body { font-family: 'Inter', -apple-system, sans-serif; background-color: #f8fafc; margin: 0; padding: 24px; }
        .container { max-width: 560px; margin: 0 auto; background: #ffffff; border-radius: 16px; border: 1px solid #e2e8f0; overflow: hidden; }
        .header { background: #004D40; padding: 24px; color: #ffffff; display: flex; justify-content: space-between; }
        .body { padding: 32px 28px; }
        .table { width: 100%; border-collapse: collapse; margin: 20px 0; font-size: 13px; }
        .table th { text-align: left; padding: 10px; background: #f1f5f9; color: #475569; font-size: 11px; text-transform: uppercase; }
        .table td { padding: 12px 10px; border-bottom: 1px solid #e2e8f0; color: #0f172a; }
        .total-box { text-align: right; margin-top: 16px; font-size: 18px; font-weight: 800; color: #004D40; }
        .btn { display: inline-block; background: #009488; color: #ffffff; text-decoration: none; font-weight: 700; font-size: 13px; padding: 10px 20px; border-radius: 8px; margin-top: 16px; }
        .footer { padding: 16px; background: #f8fafc; text-align: center; font-size: 11px; color: #94a3b8; }
    </style>
</head>
<body>
    <div class=""container"">
        <div class=""header"">
            <div>
                <h1 style=""margin: 0; font-size: 18px;"">Payment Received</h1>
                <p style=""margin: 4px 0 0 0; font-size: 12px; opacity: 0.85;"">Invoice #{{InvoiceNumber}}</p>
            </div>
            <div style=""text-align: right; font-size: 12px;"">
                <span style=""padding: 4px 10px; background: #10b981; border-radius: 6px; font-weight: 700;"">PAID</span>
            </div>
        </div>
        <div class=""body"">
            <p style=""font-size: 14px; margin: 0 0 12px 0;"">Thank you for your payment, <strong>{{InstituteName}}</strong>!</p>
            <table class=""table"">
                <thead><tr><th>Description</th><th style=""text-align: right;"">Amount</th></tr></thead>
                <tbody>
                    <tr><td>Vargshala SaaS Tier: <strong>{{PlanName}}</strong></td><td style=""text-align: right;"">₹{{AmountPaid}}</td></tr>
                </tbody>
            </table>
            <div class=""total-box"">Total Paid: ₹{{AmountPaid}}</div>
            <p style=""font-size: 12px; color: #64748b; margin-top: 16px;"">Next automatic renewal: <strong>{{NextBillingDate}}</strong>.</p>
            <a href=""{{InvoiceDownloadUrl}}"" class=""btn"">Download PDF Invoice 📄</a>
        </div>
        <div class=""footer"">
            Vargshala Multi-tenant Billing System &bull; GSTIN: 10AAACV1234F1Z5
        </div>
    </div>
</body>
</html>"
        },
        new EmailTemplateDto
        {
            Id = Guid.Parse("44444444-4444-4444-4444-444444444444"),
            Code = "TRIAL_EXPIRING",
            Name = "Free Trial Ending Reminder",
            Category = "Billing",
            Subject = "Your Vargshala 14-day trial ends in {{DaysRemaining}} days",
            Description = "Alerts institute owners 3 days prior to expiration so students and batches remain active without interruption.",
            AvailablePlaceholders = new List<string> { "{{InstituteName}}", "{{DaysRemaining}}", "{{UpgradeUrl}}", "{{StudentCount}}" },
            IsActive = true,
            UpdatedAt = DateTime.UtcNow.AddDays(-10),
            UpdatedBy = "GrowthTeam",
            BodyHtml = @"<!DOCTYPE html>
<html>
<head>
    <meta charset=""utf-8"">
    <style>
        body { font-family: 'Inter', -apple-system, sans-serif; background-color: #f8fafc; margin: 0; padding: 24px; }
        .container { max-width: 540px; margin: 0 auto; background: #ffffff; border-radius: 16px; border: 1px solid #e2e8f0; padding: 32px 28px; }
        .btn { display: block; text-align: center; background: #009488; color: #ffffff; text-decoration: none; font-weight: 700; font-size: 14px; padding: 14px 28px; border-radius: 10px; margin: 24px 0 12px 0; }
    </style>
</head>
<body>
    <div class=""container"">
        <h2 style=""color: #004D40; margin-top: 0;"">Don't lose your coaching progress!</h2>
        <p style=""font-size: 13px; line-height: 1.6; color: #475569;"">
            Your 14-day trial for <strong>{{InstituteName}}</strong> will conclude in <strong>{{DaysRemaining}} days</strong>. You currently have <strong>{{StudentCount}} students enrolled</strong> and active batches running.
        </p>
        <p style=""font-size: 13px; line-height: 1.6; color: #475569;"">
            Upgrade today to lock in seamless uninterrupted student tests, attendance tracking, and fee collections.
        </p>
        <a href=""{{UpgradeUrl}}"" class=""btn"">Upgrade to Pro Institute →</a>
    </div>
</body>
</html>"
        }
    };

    public Task<List<EmailTemplateDto>> GetAllTemplatesAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_templates.OrderBy(t => t.Category).ThenBy(t => t.Name).ToList());
    }

    public Task<EmailTemplateDto?> GetTemplateByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_templates.FirstOrDefault(t => t.Id == id));
    }

    public Task<ApiResponse<EmailTemplateDto>> CreateTemplateAsync(CreateEmailTemplateRequest request, CancellationToken cancellationToken = default)
    {
        if (_templates.Any(t => t.Code.Equals(request.Code, StringComparison.OrdinalIgnoreCase)))
        {
            return Task.FromResult(ApiResponse<EmailTemplateDto>.FailureResponse($"Template code '{request.Code}' already exists."));
        }

        var newTemplate = new EmailTemplateDto
        {
            Id = Guid.NewGuid(),
            Code = request.Code.Trim().ToUpperInvariant(),
            Name = request.Name.Trim(),
            Category = request.Category,
            Subject = request.Subject.Trim(),
            BodyHtml = request.BodyHtml,
            Description = request.Description?.Trim(),
            AvailablePlaceholders = request.AvailablePlaceholders ?? new List<string>(),
            IsActive = request.IsActive,
            UpdatedAt = DateTime.UtcNow,
            UpdatedBy = "SuperAdmin"
        };

        _templates.Add(newTemplate);
        return Task.FromResult(ApiResponse<EmailTemplateDto>.SuccessResponse(newTemplate, "Email template created successfully."));
    }

    public Task<ApiResponse<EmailTemplateDto>> UpdateTemplateAsync(UpdateEmailTemplateRequest request, CancellationToken cancellationToken = default)
    {
        var existing = _templates.FirstOrDefault(t => t.Id == request.Id);
        if (existing == null)
        {
            return Task.FromResult(ApiResponse<EmailTemplateDto>.FailureResponse("Template not found."));
        }

        existing.Name = request.Name.Trim();
        existing.Category = request.Category;
        existing.Subject = request.Subject.Trim();
        existing.BodyHtml = request.BodyHtml;
        existing.Description = request.Description?.Trim();
        existing.AvailablePlaceholders = request.AvailablePlaceholders ?? existing.AvailablePlaceholders;
        existing.IsActive = request.IsActive;
        existing.UpdatedAt = DateTime.UtcNow;
        existing.UpdatedBy = "SuperAdmin";

        return Task.FromResult(ApiResponse<EmailTemplateDto>.SuccessResponse(existing, "Template updated successfully."));
    }

    public Task<ApiResponse<bool>> ToggleTemplateStatusAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var existing = _templates.FirstOrDefault(t => t.Id == id);
        if (existing == null)
        {
            return Task.FromResult(ApiResponse<bool>.FailureResponse("Template not found."));
        }

        existing.IsActive = !existing.IsActive;
        existing.UpdatedAt = DateTime.UtcNow;
        return Task.FromResult(ApiResponse<bool>.SuccessResponse(existing.IsActive, $"Template is now {(existing.IsActive ? "Active" : "Disabled")}."));
    }

    public Task<ApiResponse<bool>> SendTestEmailAsync(SendTestEmailRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.RecipientEmail) || !request.RecipientEmail.Contains('@'))
        {
            return Task.FromResult(ApiResponse<bool>.FailureResponse("Please enter a valid recipient email address."));
        }

        // Simulate successful test dispatch
        return Task.FromResult(ApiResponse<bool>.SuccessResponse(true, $"Test email successfully dispatched to {request.RecipientEmail}."));
    }
}
