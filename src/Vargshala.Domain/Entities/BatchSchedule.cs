using DayOfWeek = Vargshala.Contracts.Common.DayOfWeek;
using Vargshala.Domain.Common;

namespace Vargshala.Domain.Entities;

public class BatchSchedule : BaseEntity
{
    public Guid BatchId { get; set; }
    public DayOfWeek DayOfWeek { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public string? RoomOrLocation { get; set; }

    // Navigation
    public Batch Batch { get; set; } = null!;
    public ICollection<ClassSession> ClassSessions { get; set; } = new List<ClassSession>();
}
