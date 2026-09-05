using Vargshala.Domain.Common;

namespace Vargshala.Domain.Entities;

public class Student : BaseEntity
{
    public Guid UserId { get; set; }

    // Personal Details
    public DateOnly? DateOfBirth { get; set; }
    public string? Gender { get; set; }
    public string? BloodGroup { get; set; }
    public string? Nationality { get; set; }

    // Academic Details
    public string? StudentCode { get; set; }
    public DateOnly? AdmissionDate { get; set; }
    public string? ClassName { get; set; }
    public string? Section { get; set; }
    public string? RollNumber { get; set; }

    // Parent Details
    public string? FatherName { get; set; }
    public string? FatherMobile { get; set; }
    public string? FatherAlternateMobile { get; set; }
    public string? MotherName { get; set; }

    // Address
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? PostalCode { get; set; }
    public string? Country { get; set; }

    // Emergency Contact
    public string? EmergencyContactName { get; set; }
    public string? EmergencyContactMobile { get; set; }
    public string? EmergencyContactRelation { get; set; }

    // Additional Information
    public string? AadharNumber { get; set; }
    public string? PreviousInstitute { get; set; }
    public string? MedicalNotes { get; set; }

    // Navigation
    public User User { get; set; } = null!;
}
