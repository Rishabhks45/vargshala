using Vargshala.Domain.Common;

namespace Vargshala.Domain.Entities;

public class Batch : BaseEntity
{
    public Guid ClassId { get; set; }
    public Guid SubjectId { get; set; }

    // Batch Details
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public TimeOnly? StartTime { get; set; }
    public TimeOnly? EndTime { get; set; }

    // Navigation
    public Class Class { get; set; } = null!;
    public Subject Subject { get; set; } = null!;
    public ICollection<BatchTeacher> BatchTeachers { get; set; } = new List<BatchTeacher>();
    public ICollection<BatchStudent> BatchStudents { get; set; } = new List<BatchStudent>();
    public ICollection<BatchSchedule> BatchSchedules { get; set; } = new List<BatchSchedule>();
    public ICollection<ClassSession> ClassSessions { get; set; } = new List<ClassSession>();
}
