namespace Vargshala.Domain.Entities;

public class BatchStudent
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid BatchId { get; set; }
    public Guid StudentId { get; set; }

    public bool IsPrimary { get; set; } = true;
    public string EnrollmentType { get; set; } = "Regular"; // Regular, Additional

    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LeftAt { get; set; }

    public bool IsActive { get; set; } = true;

    // Audit fields
    public Guid? CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public Guid? UpdatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }

    // Navigation
    public Batch Batch { get; set; } = null!;
    public Student Student { get; set; } = null!;
}
