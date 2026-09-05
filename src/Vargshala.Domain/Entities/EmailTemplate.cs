using Vargshala.Contracts.Common;
using Vargshala.Domain.Common;

namespace Vargshala.Domain.Entities;

public class EmailTemplate : BaseEntity
{
    public Guid? OrganizationId { get; set; }

    public EmailTemplateCategory Category { get; set; } = EmailTemplateCategory.Onboarding;

    public UserRole? TargetRole { get; set; }

    public string Code { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string Subject { get; set; } = string.Empty;

    public string? AvailablePlaceholders { get; set; }

    public string BodyHtml { get; set; } = string.Empty;

    public string? Description { get; set; }

    // Navigation
    public Organization? Organization { get; set; }
}
