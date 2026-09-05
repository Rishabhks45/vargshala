namespace Vargshala.Domain.Entities;

public class UserBranchAccess
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }
    public Guid BranchId { get; set; }

    public bool IsActive { get; set; } = true;

    // Audit
    public Guid? CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public Guid? UpdatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }

    // Navigation
    public User User { get; set; } = null!;
    public Branch Branch { get; set; } = null!;
}
