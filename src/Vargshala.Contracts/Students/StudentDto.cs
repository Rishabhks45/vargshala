namespace Vargshala.Contracts.Students;

public class StudentDto
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string EnrollmentNumber { get; set; } = string.Empty;
    public string RollNumber { get; set; } = string.Empty;
    public string BatchName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string ParentName { get; set; } = string.Empty;
    public string Status { get; set; } = "Active"; // Active, Inactive
    public string FeeStatus { get; set; } = "Paid"; // Paid, Partial, Overdue
    public decimal TotalFee { get; set; } = 25000;
    public decimal PaidFee { get; set; } = 25000;
    public decimal DueFee => TotalFee - PaidFee;
    public string Initials => string.Join("", Name.Split(' ', StringSplitOptions.RemoveEmptyEntries).Select(s => s[0])).ToUpper();
}
