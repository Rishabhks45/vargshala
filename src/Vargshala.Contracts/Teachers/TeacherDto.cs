namespace Vargshala.Contracts.Teachers;

public class TeacherDto
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string EmployeeId { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Designation { get; set; } = string.Empty; // Senior Teacher, HOD, Assistant Teacher, etc.
    public string Qualification { get; set; } = string.Empty; // M.Sc., B.Ed., Ph.D., etc.
    public string Subject { get; set; } = string.Empty; // Primary subject
    public List<string> Batches { get; set; } = new(); // Assigned batch names
    public string Status { get; set; } = "Active"; // Active, Inactive
    public DateTime JoiningDate { get; set; } = DateTime.Today;
    public string Initials => string.Join("", Name.Split(' ', StringSplitOptions.RemoveEmptyEntries).Select(s => s[0])).ToUpper();
    public string BatchesDisplay => Batches.Count > 0 ? string.Join(", ", Batches) : "—";
    public int BatchCount => Batches.Count;
}
