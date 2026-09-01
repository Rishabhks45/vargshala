namespace Vargshala.Domain.Common;

public abstract class BaseEntity
{
    public Guid Id { get; set; }

    // Status
    public bool IsActive { get; set; } = true;

    // Created Audit
    public Guid? CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }

    // Updated Audit
    public Guid? UpdatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }

    // Soft Delete
    public bool IsDeleted { get; set; } = false;
    public Guid? DeletedBy { get; set; }
    public DateTime? DeletedAt { get; set; }
}
