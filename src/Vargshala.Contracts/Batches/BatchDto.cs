namespace Vargshala.Contracts.Batches;

public class BatchDto
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ClassId { get; set; }
    public string ClassName { get; set; } = string.Empty;
    public Guid BranchId { get; set; }
    public string BranchName { get; set; } = string.Empty;
    public Guid SubjectId { get; set; }
    public string SubjectName { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public TimeOnly? StartTime { get; set; }
    public TimeOnly? EndTime { get; set; }
    public bool IsActive { get; set; } = true;
    public int AssignedTeachersCount { get; set; }
    public int EnrolledStudentsCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    // UI compatibility properties for Batches.razor
    public string Grade { get; set; } = string.Empty;
    public string AcademicSession { get; set; } = "2026-27";
    public string RoomOrTiming { get; set; } = string.Empty;
    public string PrimaryTeacher { get; set; } = string.Empty;
    public int StudentCount { get; set; }
    public int MaxCapacity { get; set; } = 60;
    public string Status
    {
        get => IsActive ? Vargshala.Contracts.Common.EntityStatusNames.Active : Vargshala.Contracts.Common.EntityStatusNames.Inactive;
        set => IsActive = Vargshala.Contracts.Common.EntityStatusExtensions.FromString(value) == Vargshala.Contracts.Common.EntityStatus.Active;
    }
    public Vargshala.Contracts.Common.EntityStatus EntityStatus => Vargshala.Contracts.Common.EntityStatusExtensions.FromBool(IsActive);

    public string Initials
    {
        get
        {
            if (string.IsNullOrWhiteSpace(Name)) return "B";
            var parts = Name.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            return string.Join("", parts.Where(s => char.IsLetter(s[0])).Take(2).Select(s => s[0])).ToUpper();
        }
    }
    public double OccupancyPercent => MaxCapacity > 0 ? Math.Round((double)StudentCount / MaxCapacity * 100, 0) : 0;
}

public class CreateBatchRequest
{
    public Guid ClassId { get; set; }
    public Guid SubjectId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public TimeOnly? StartTime { get; set; }
    public TimeOnly? EndTime { get; set; }
}

public class UpdateBatchRequest
{
    public Guid Id { get; set; }
    public Guid ClassId { get; set; }
    public Guid SubjectId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public TimeOnly? StartTime { get; set; }
    public TimeOnly? EndTime { get; set; }
    public bool IsActive { get; set; } = true;
}

public class BatchDetailDto
{
    public BatchDto Batch { get; set; } = new();
    public List<BatchTeacherDto> Teachers { get; set; } = new();
    public List<BatchStudentDto> Students { get; set; } = new();
}
