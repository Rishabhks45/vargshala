namespace Vargshala.Contracts.Masters;

public class BranchDto
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string ContactPerson { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public int ActiveBatchesCount { get; set; }
    public int TotalStudentsCount { get; set; }
    public string Status { get; set; } = "Active"; // Active, Inactive
}

public class SubjectDto
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string GradeLevel { get; set; } = string.Empty; // Class 11 & 12, Class 9 & 10, etc.
    public string Stream { get; set; } = "Science"; // Science, Commerce, Arts, General
    public int TotalFacultyAssigned { get; set; }
    public int WeeklyHours { get; set; } = 6;
    public string Status { get; set; } = "Active";
}

public class FacultySubjectAssignmentDto
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string FacultyName { get; set; } = string.Empty;
    public string FacultyInitials => string.Join("", FacultyName.Split(' ', StringSplitOptions.RemoveEmptyEntries).Select(s => s[0])).ToUpper();
    public string BranchName { get; set; } = string.Empty;
    public string SubjectName { get; set; } = string.Empty;
    public string BatchName { get; set; } = string.Empty;
    public int HoursPerWeek { get; set; } = 4;
    public bool IsPrimary { get; set; } = true;
    public string Status { get; set; } = "Active"; // Active, On Leave
}

public class PeriodTimingDto
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public int PeriodNumber { get; set; } = 1;
    public string PeriodName { get; set; } = string.Empty; // e.g. "Period 1", "Recess / Break"
    public string Shift { get; set; } = "Morning Shift"; // Morning Shift, Evening Shift
    public string StartTime { get; set; } = "08:00 AM";
    public string EndTime { get; set; } = "08:50 AM";
    public int DurationMinutes { get; set; } = 50;
    public string BranchName { get; set; } = "All Branches";
    public bool IsBreak { get; set; } = false;
    public string Status { get; set; } = "Active";
}
