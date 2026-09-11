namespace Vargshala.Contracts.Subjects;

public class SubjectDto
{
    public Guid Id { get; set; }
    public Guid OrganizationId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    public Vargshala.Contracts.Common.EntityStatus Status => Vargshala.Contracts.Common.EntityStatusExtensions.FromBool(IsActive);
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
