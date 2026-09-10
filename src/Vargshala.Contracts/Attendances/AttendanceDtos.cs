namespace Vargshala.Contracts.Attendances;

public class AttendanceDto
{
    public Guid Id { get; set; }
    public Guid ClassSessionId { get; set; }
    public Guid StudentId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public string? StudentCode { get; set; }
    public string? RollNumber { get; set; }
    public string Status { get; set; } = AttendanceStatus.Present;
    public DateTime? MarkedAt { get; set; }
    public string? Remarks { get; set; }
}

public class StudentAttendanceItemDto
{
    public Guid StudentId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public string? StudentCode { get; set; }
    public string? RollNumber { get; set; }
    public Guid? AttendanceId { get; set; }
    public string Status { get; set; } = AttendanceStatus.Present; // Present, Absent, Late, Excused
    public string? Remarks { get; set; }
    public DateTime? MarkedAt { get; set; }
    public string CheckInTimeFormatted => MarkedAt.HasValue ? MarkedAt.Value.ToString("hh:mm tt") : "-";
}

public class SessionAttendanceSheetDto
{
    public Guid SessionId { get; set; }
    public DateOnly SessionDate { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public Guid BatchId { get; set; }
    public string BatchName { get; set; } = string.Empty;
    public string ClassName { get; set; } = string.Empty;
    public string SubjectName { get; set; } = string.Empty;
    public string? TeacherName { get; set; }
    public string? Topic { get; set; }
    public string SessionStatus { get; set; } = "Scheduled";

    // Summary counts
    public int TotalStudents => Students.Count;
    public int PresentCount => Students.Count(s => s.Status == AttendanceStatus.Present);
    public int AbsentCount => Students.Count(s => s.Status == AttendanceStatus.Absent);
    public int LateCount => Students.Count(s => s.Status == AttendanceStatus.Late);
    public int ExcusedCount => Students.Count(s => s.Status == AttendanceStatus.Excused);
    public double AttendanceRate => TotalStudents > 0
        ? Math.Round((double)(PresentCount + LateCount) / TotalStudents * 100, 1)
        : 0;

    public bool IsAttendanceMarked => Students.Any(s => s.AttendanceId.HasValue);

    public List<StudentAttendanceItemDto> Students { get; set; } = new();
}

public class StudentAttendanceEntry
{
    public Guid StudentId { get; set; }
    public string Status { get; set; } = AttendanceStatus.Present;
    public string? Remarks { get; set; }
}

public class MarkSessionAttendanceRequest
{
    public Guid ClassSessionId { get; set; }
    public List<StudentAttendanceEntry> Items { get; set; } = new();
}

public class DailyAttendanceTrendDto
{
    public DateOnly Date { get; set; }
    public string DayOfWeekShort { get; set; } = string.Empty; // e.g. "Mon"
    public string DateShortFormatted { get; set; } = string.Empty; // e.g. "21 Sep"
    public int TotalStudents { get; set; }
    public int PresentCount { get; set; }
    public int LateCount { get; set; }
    public int AbsentCount { get; set; }
    public int Percentage { get; set; }
}

public class RecentAttendanceActivityDto
{
    public Guid StudentId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public string Status { get; set; } = AttendanceStatus.Present;
    public string? MarkedAtFormatted { get; set; }
    public DateTime? MarkedAt { get; set; }
}

public class BatchAttendanceOverviewDto
{
    public Guid BatchId { get; set; }
    public string BatchName { get; set; } = string.Empty;
    public string ClassName { get; set; } = string.Empty;
    public string SubjectName { get; set; } = string.Empty;
    public string BranchName { get; set; } = string.Empty;
    public string ScheduleFormatted { get; set; } = string.Empty;
    public int TotalStudents { get; set; }

    // Today's stats
    public int TodayPresent { get; set; }
    public int TodayAbsent { get; set; }
    public int TodayLate { get; set; }
    public int TodayExcused { get; set; }
    public double TodayAttendancePercentage => TotalStudents > 0
        ? Math.Round((double)(TodayPresent + TodayLate) / TotalStudents * 100, 1)
        : 0;
    public double TodayAbsentPercentage => TotalStudents > 0
        ? Math.Round((double)TodayAbsent / TotalStudents * 100, 1)
        : 0;
    public double TodayLatePercentage => TotalStudents > 0
        ? Math.Round((double)TodayLate / TotalStudents * 100, 1)
        : 0;

    // 7-day trend
    public List<DailyAttendanceTrendDto> DailyTrends { get; set; } = new();

    // Recent marked activities
    public List<RecentAttendanceActivityDto> RecentActivities { get; set; } = new();
}

public class DateAttendanceReportDto
{
    public Guid SessionId { get; set; }
    public DateOnly SessionDate { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public string BatchName { get; set; } = string.Empty;
    public string TeacherName { get; set; } = string.Empty;
    public string? Topic { get; set; }
    public int TotalStudents { get; set; }
    public int PresentCount { get; set; }
    public int AbsentCount { get; set; }
    public int LateCount { get; set; }
    public int ExcusedCount { get; set; }
    public double AttendancePercentage => TotalStudents > 0
        ? Math.Round((double)(PresentCount + LateCount) / TotalStudents * 100, 1)
        : 0;
}

public class StudentAttendanceReportDto
{
    public Guid StudentId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string? RollNumber { get; set; }
    public string? StudentCode { get; set; }
    public int TotalClasses { get; set; }
    public int PresentCount { get; set; }
    public int AbsentCount { get; set; }
    public int LateCount { get; set; }
    public int ExcusedCount { get; set; }
    public double AttendancePercentage => TotalClasses > 0
        ? Math.Round((double)(PresentCount + LateCount) / TotalClasses * 100, 1)
        : 0;
}
