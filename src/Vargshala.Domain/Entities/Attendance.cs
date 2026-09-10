namespace Vargshala.Domain.Entities;

public class Attendance
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid ClassSessionId { get; set; }
    public Guid StudentId { get; set; }

    public string Status { get; set; } = "Present"; // Present, Absent, Late, Excused

    public DateTime? MarkedAt { get; set; }
    public string? Remarks { get; set; }

    // Audit fields
    public Guid? CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public Guid? UpdatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }

    // Soft Delete
    public bool IsDeleted { get; set; } = false;
    public Guid? DeletedBy { get; set; }
    public DateTime? DeletedAt { get; set; }

    // Navigation
    public ClassSession ClassSession { get; set; } = null!;
    public Student Student { get; set; } = null!;
}
