namespace Vargshala.Contracts.Attendances;

public static class AttendanceStatus
{
    public const string Present = "Present";
    public const string Absent = "Absent";
    public const string Late = "Late";
    public const string Excused = "Excused";

    public static readonly string[] All = { Present, Absent, Late, Excused };

    public static bool IsValid(string? status)
    {
        return !string.IsNullOrWhiteSpace(status) &&
               (status.Equals(Present, StringComparison.OrdinalIgnoreCase) ||
                status.Equals(Absent, StringComparison.OrdinalIgnoreCase) ||
                status.Equals(Late, StringComparison.OrdinalIgnoreCase) ||
                status.Equals(Excused, StringComparison.OrdinalIgnoreCase));
    }

    public static string Normalize(string? status)
    {
        if (string.IsNullOrWhiteSpace(status)) return Present;
        return status.Trim().ToLowerInvariant() switch
        {
            "absent" => Absent,
            "late" => Late,
            "excused" => Excused,
            _ => Present
        };
    }
}
