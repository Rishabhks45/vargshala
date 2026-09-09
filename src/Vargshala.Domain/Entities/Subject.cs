using Vargshala.Domain.Common;

namespace Vargshala.Domain.Entities;

public class Subject : BaseEntity
{
    public Guid OrganizationId { get; set; }

    // Subject Details
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? Description { get; set; }

    // Navigation
    public Organization Organization { get; set; } = null!;
}
