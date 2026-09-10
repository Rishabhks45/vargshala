using System.ComponentModel.DataAnnotations;

namespace Vargshala.Contracts.ClassSessions;

public enum ClassSessionStatus
{
    [Display(Name = "Scheduled")]
    Scheduled = 1,

    [Display(Name = "Completed")]
    Completed = 2,

    [Display(Name = "Cancelled")]
    Cancelled = 3,

    [Display(Name = "Postponed")]
    Postponed = 4
}

public static class ClassSessionStatusNames
{
    public const string Scheduled = "Scheduled";
    public const string Completed = "Completed";
    public const string Cancelled = "Cancelled";
    public const string Postponed = "Postponed";
}

public static class ClassSessionStatusExtensions
{
    public static string GetDisplayName(this ClassSessionStatus status) => status switch
    {
        ClassSessionStatus.Scheduled => ClassSessionStatusNames.Scheduled,
        ClassSessionStatus.Completed => ClassSessionStatusNames.Completed,
        ClassSessionStatus.Cancelled => ClassSessionStatusNames.Cancelled,
        ClassSessionStatus.Postponed => ClassSessionStatusNames.Postponed,
        _ => status.ToString()
    };
}
