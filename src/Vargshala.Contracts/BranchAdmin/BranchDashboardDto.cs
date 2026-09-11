namespace Vargshala.Contracts.BranchAdmin;

public class BranchDashboardDto
{
    public Guid BranchId { get; set; }
    public string BranchName { get; set; } = string.Empty;
    public string BranchCode { get; set; } = string.Empty;
    public int TotalStudents { get; set; }
    public int TotalTeachers { get; set; }
    public int TotalClasses { get; set; }
    public int TotalBatches { get; set; }
    public int TodaySessionsCount { get; set; }
    public double TodayAttendancePercentage { get; set; }
    public List<BranchRecentSessionDto> RecentSessions { get; set; } = new();
}

public class BranchRecentSessionDto
{
    public Guid Id { get; set; }
    public string BatchName { get; set; } = string.Empty;
    public string ClassName { get; set; } = string.Empty;
    public string SubjectName { get; set; } = string.Empty;
    public string TeacherName { get; set; } = string.Empty;
    public DateOnly Date { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public string Status { get; set; } = string.Empty;
}
