namespace Vargshala.Contracts.Batches;

public class BatchDto
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty; // e.g. "Class 10 — Maths Morning"
    public string Grade { get; set; } = string.Empty; // e.g. "Class 10", "Class 12"
    public string AcademicSession { get; set; } = "2026-27";
    public string RoomOrTiming { get; set; } = string.Empty; // e.g. "Room A1, 8:00 AM - 10:00 AM"
    public string PrimaryTeacher { get; set; } = string.Empty;
    public int StudentCount { get; set; }
    public int MaxCapacity { get; set; } = 60;
    public string Status { get; set; } = "Active"; // Active, Archived
    public string Initials => string.Join("", Name.Split(' ', StringSplitOptions.RemoveEmptyEntries).Where(s => char.IsLetter(s[0])).Take(2).Select(s => s[0])).ToUpper();
    public double OccupancyPercent => MaxCapacity > 0 ? Math.Round((double)StudentCount / MaxCapacity * 100, 0) : 0;
}
