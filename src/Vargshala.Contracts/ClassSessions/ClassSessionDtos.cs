using System.ComponentModel.DataAnnotations;

namespace Vargshala.Contracts.ClassSessions;

public class ClassSessionDto
{
    public Guid Id { get; set; }
    public Guid BatchId { get; set; }
    public string BatchName { get; set; } = string.Empty;
    public string BatchCode { get; set; } = string.Empty;

    public Guid ClassId { get; set; }
    public string ClassName { get; set; } = string.Empty;

    public Guid SubjectId { get; set; }
    public string SubjectName { get; set; } = string.Empty;

    public Guid BranchId { get; set; }
    public string BranchName { get; set; } = string.Empty;

    public Guid? BatchScheduleId { get; set; }
    public Guid? TeacherId { get; set; }
    public string? TeacherName { get; set; }
    public string? TeacherEmail { get; set; }

    public DateOnly SessionDate { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }

    public string? Topic { get; set; }
    public string? Notes { get; set; }
    public string Status { get; set; } = ClassSessionStatusNames.Scheduled;
    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class CreateClassSessionRequest
{
    [Required(ErrorMessage = "Batch is required.")]
    public Guid BatchId { get; set; }

    public Guid? BatchScheduleId { get; set; }
    public Guid? TeacherId { get; set; }

    [Required(ErrorMessage = "Session Date is required.")]
    public DateOnly SessionDate { get; set; }

    [Required(ErrorMessage = "Start Time is required.")]
    public TimeOnly StartTime { get; set; }

    [Required(ErrorMessage = "End Time is required.")]
    public TimeOnly EndTime { get; set; }

    [MaxLength(250, ErrorMessage = "Topic cannot exceed 250 characters.")]
    public string? Topic { get; set; }

    public string? Notes { get; set; }
}

public class UpdateClassSessionRequest
{
    [Required(ErrorMessage = "Session ID is required.")]
    public Guid Id { get; set; }

    public Guid? TeacherId { get; set; }

    [Required(ErrorMessage = "Session Date is required.")]
    public DateOnly SessionDate { get; set; }

    [Required(ErrorMessage = "Start Time is required.")]
    public TimeOnly StartTime { get; set; }

    [Required(ErrorMessage = "End Time is required.")]
    public TimeOnly EndTime { get; set; }

    [MaxLength(250, ErrorMessage = "Topic cannot exceed 250 characters.")]
    public string? Topic { get; set; }

    public string? Notes { get; set; }

    [Required(ErrorMessage = "Status is required.")]
    public string Status { get; set; } = ClassSessionStatusNames.Scheduled;
}

public class GenerateSessionsFromScheduleRequest
{
    [Required(ErrorMessage = "Batch is required.")]
    public Guid BatchId { get; set; }

    [Required(ErrorMessage = "Start Date is required.")]
    public DateOnly StartDate { get; set; }

    [Required(ErrorMessage = "End Date is required.")]
    public DateOnly EndDate { get; set; }

    public Guid? TeacherId { get; set; }
}

public class CancelClassSessionRequest
{
    public string? Reason { get; set; }
}

public class CompleteClassSessionRequest
{
    [MaxLength(250, ErrorMessage = "Topic cannot exceed 250 characters.")]
    public string? Topic { get; set; }
    public string? Notes { get; set; }
}
