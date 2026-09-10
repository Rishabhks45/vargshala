using Vargshala.Domain.Common;

namespace Vargshala.Domain.Entities;

public class ClassSession : BaseEntity
{
    public Guid BatchId { get; set; }
    public Guid? BatchScheduleId { get; set; }
    public Guid? TeacherId { get; set; }

    public DateOnly SessionDate { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }

    public string? Topic { get; set; }
    public string? Notes { get; set; }
    public string Status { get; set; } = "Scheduled";

    // Navigation
    public Batch Batch { get; set; } = null!;
    public BatchSchedule? BatchSchedule { get; set; }
    public Teacher? Teacher { get; set; }

    public ICollection<Attendance> Attendances { get; set; } = new List<Attendance>();
}
