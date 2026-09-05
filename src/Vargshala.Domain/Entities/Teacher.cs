using Vargshala.Domain.Common;

namespace Vargshala.Domain.Entities;

public class Teacher : BaseEntity
{
    public Guid UserId { get; set; }

    // Professional Details
    public string? EmployeeCode { get; set; }
    public DateOnly? JoiningDate { get; set; }
    public string? Department { get; set; }
    public string? Designation { get; set; }

    // Qualification Details
    public string? HighestQualification { get; set; }
    public string? Specialization { get; set; }
    public decimal? TeachingExperienceYears { get; set; }

    // Address
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? PostalCode { get; set; }
    public string? Country { get; set; }

    // Additional Information
    public string? AadharNumber { get; set; }
    public string? PreviousInstitute { get; set; }
    public string? Bio { get; set; }

    // Navigation
    public User User { get; set; } = null!;
}
