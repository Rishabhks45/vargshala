namespace Vargshala.Contracts.Subscriptions;

public class QuotaUsageDto
{
    public OrganizationSubscriptionDto? CurrentSubscription { get; set; }

    // Students Quota
    public int CurrentStudentsCount { get; set; }
    public int? MaxStudents { get; set; }
    public int StudentsUsagePercentage => MaxStudents.HasValue && MaxStudents.Value > 0
        ? (int)Math.Min(100, Math.Round((double)CurrentStudentsCount / MaxStudents.Value * 100))
        : 0;
    public bool IsStudentsQuotaExceeded => MaxStudents.HasValue && CurrentStudentsCount >= MaxStudents.Value;

    // Teachers Quota
    public int CurrentTeachersCount { get; set; }
    public int? MaxTeachers { get; set; }
    public int TeachersUsagePercentage => MaxTeachers.HasValue && MaxTeachers.Value > 0
        ? (int)Math.Min(100, Math.Round((double)CurrentTeachersCount / MaxTeachers.Value * 100))
        : 0;
    public bool IsTeachersQuotaExceeded => MaxTeachers.HasValue && CurrentTeachersCount >= MaxTeachers.Value;

    // Branches Quota
    public int CurrentBranchesCount { get; set; }
    public int? MaxBranches { get; set; }
    public int BranchesUsagePercentage => MaxBranches.HasValue && MaxBranches.Value > 0
        ? (int)Math.Min(100, Math.Round((double)CurrentBranchesCount / MaxBranches.Value * 100))
        : 0;
    public bool IsBranchesQuotaExceeded => MaxBranches.HasValue && CurrentBranchesCount >= MaxBranches.Value;
}
