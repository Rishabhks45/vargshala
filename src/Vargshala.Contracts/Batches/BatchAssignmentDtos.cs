namespace Vargshala.Contracts.Batches;

public class BatchTeacherDto
{
    public Guid Id { get; set; }
    public Guid BatchId { get; set; }
    public Guid TeacherId { get; set; }
    public string TeacherName { get; set; } = string.Empty;
    public string TeacherCode { get; set; } = string.Empty;
    public string? TeacherEmail { get; set; }
    public string? TeacherMobile { get; set; }
    public DateTime AssignedAt { get; set; }
    public DateTime? RemovedAt { get; set; }
    public bool IsActive { get; set; }
}

public class AssignTeacherToBatchRequest
{
    public Guid TeacherId { get; set; }
}

public class BatchStudentDto
{
    public Guid Id { get; set; }
    public Guid BatchId { get; set; }
    public Guid StudentId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public string StudentCode { get; set; } = string.Empty;
    public string? StudentEmail { get; set; }
    public string? StudentMobile { get; set; }
    public bool IsPrimary { get; set; }
    public string EnrollmentType { get; set; } = "Regular"; // Regular, Additional
    public DateTime JoinedAt { get; set; }
    public DateTime? LeftAt { get; set; }
    public bool IsActive { get; set; }
}

public class EnrollStudentToBatchRequest
{
    public Guid StudentId { get; set; }
    public bool IsPrimary { get; set; } = true;
    public string EnrollmentType { get; set; } = "Regular";
    public DateTime? JoinedAt { get; set; }
}

public class StudentBatchEnrollmentDto
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
    public bool IsPrimary { get; set; }
    public string EnrollmentType { get; set; } = "Regular";
    public DateTime JoinedAt { get; set; }
    public DateTime? LeftAt { get; set; }
    public bool IsActive { get; set; }
}
