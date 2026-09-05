using FluentValidation;
using Vargshala.Contracts.Common;

namespace Vargshala.Contracts.EmailTemplates;

public class EmailTemplateDto
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Code { get; set; } = string.Empty;
    public EmailTemplateName? TemplateType { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = "Onboarding";
    public UserRole? TargetRole { get; set; }
    public string? TargetRoleName
    {
        get => _targetRoleName ?? TargetRole?.GetDisplayName();
        set => _targetRoleName = value;
    }
    private string? _targetRoleName;
    public string Subject { get; set; } = string.Empty;
    public string BodyHtml { get; set; } = string.Empty;
    public string? Description { get; set; }
    public List<string> AvailablePlaceholders { get; set; } = new();
    public bool IsActive { get; set; } = true;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public string UpdatedBy { get; set; } = "SuperAdmin";
}

public class CreateEmailTemplateRequest
{
    public string Code { get; set; } = string.Empty;
    public EmailTemplateName? TemplateType { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = "Onboarding";
    public UserRole? TargetRole { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string BodyHtml { get; set; } = string.Empty;
    public string? Description { get; set; }
    public List<string> AvailablePlaceholders { get; set; } = new();
    public bool IsActive { get; set; } = true;
}

public class CreateEmailTemplateRequestValidator : AbstractValidator<CreateEmailTemplateRequest>
{
    public CreateEmailTemplateRequestValidator()
    {
        ClassLevelCascadeMode = CascadeMode.Stop;
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Template code is required.")
            .MaximumLength(50).WithMessage("Template code cannot exceed 50 characters.")
            .Matches(@"^[A-Z0-9_\-]+$").WithMessage("Code must be uppercase alphanumeric (e.g. WELCOME_ONBOARD).");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Template name is required.")
            .MaximumLength(150).WithMessage("Template name cannot exceed 150 characters.");

        RuleFor(x => x.Category)
            .NotEmpty().WithMessage("Category is required.");

        RuleFor(x => x.Subject)
            .NotEmpty().WithMessage("Email subject line is required.")
            .MaximumLength(200).WithMessage("Subject cannot exceed 200 characters.");

        RuleFor(x => x.BodyHtml)
            .NotEmpty().WithMessage("HTML body content is required.");
    }
}

public class UpdateEmailTemplateRequest
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public EmailTemplateName? TemplateType { get; set; }
    public string Category { get; set; } = "Onboarding";
    public UserRole? TargetRole { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string BodyHtml { get; set; } = string.Empty;
    public string? Description { get; set; }
    public List<string> AvailablePlaceholders { get; set; } = new();
    public bool IsActive { get; set; } = true;
}

public class UpdateEmailTemplateRequestValidator : AbstractValidator<UpdateEmailTemplateRequest>
{
    public UpdateEmailTemplateRequestValidator()
    {
        ClassLevelCascadeMode = CascadeMode.Stop;
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Template name is required.")
            .MaximumLength(150).WithMessage("Template name cannot exceed 150 characters.");

        RuleFor(x => x.Category)
            .NotEmpty().WithMessage("Category is required.");

        RuleFor(x => x.Subject)
            .NotEmpty().WithMessage("Email subject line is required.")
            .MaximumLength(200).WithMessage("Subject cannot exceed 200 characters.");

        RuleFor(x => x.BodyHtml)
            .NotEmpty().WithMessage("HTML body content is required.");
    }
}

public class SendTestEmailRequest
{
    public Guid TemplateId { get; set; }
    public string RecipientEmail { get; set; } = string.Empty;
    public Dictionary<string, string> SampleValues { get; set; } = new();
}

public class SendCustomEmailRequest
{
    public string To { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string HtmlBody { get; set; } = string.Empty;
    public string? From { get; set; }
}

public class SendCustomEmailRequestValidator : AbstractValidator<SendCustomEmailRequest>
{
    public SendCustomEmailRequestValidator()
    {
        ClassLevelCascadeMode = CascadeMode.Stop;
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(x => x.To)
            .NotEmpty().WithMessage("Recipient email address is required.")
            .EmailAddress().WithMessage("A valid email address is required.");

        RuleFor(x => x.Subject)
            .NotEmpty().WithMessage("Email subject is required.")
            .MaximumLength(200).WithMessage("Subject cannot exceed 200 characters.");

        RuleFor(x => x.HtmlBody)
            .NotEmpty().WithMessage("Email content is required.");
    }
}

