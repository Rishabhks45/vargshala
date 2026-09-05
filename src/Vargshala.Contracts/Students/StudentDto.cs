using FluentValidation;

namespace Vargshala.Contracts.Students;

#region Student DTO
public class StudentDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid? OrganizationId { get; set; }

    // User details
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Mobile { get; set; }
    public string? ProfilePictureUrl { get; set; }

    // Convenience & backwards-compatibility
    public string FullName => $"{FirstName} {LastName}".Trim();
    public string Name
    {
        get => !string.IsNullOrWhiteSpace(FullName) ? FullName : _name;
        set => _name = value;
    }
    private string _name = string.Empty;

    public string Phone
    {
        get => Mobile ?? string.Empty;
        set => Mobile = value;
    }

    public string Initials
    {
        get
        {
            var display = !string.IsNullOrWhiteSpace(FullName) ? FullName : Name;
            if (string.IsNullOrWhiteSpace(display)) return "ST";
            var parts = display.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            return parts.Length > 1
                ? $"{parts[0][0]}{parts[1][0]}".ToUpper()
                : $"{parts[0][0]}".ToUpper();
        }
    }

    // Personal Details
    public DateOnly? DateOfBirth { get; set; }
    public string? Gender { get; set; }
    public string? BloodGroup { get; set; }
    public string? Nationality { get; set; }

    // Academic Details
    public string? StudentCode { get; set; }
    public string EnrollmentNumber
    {
        get => StudentCode ?? string.Empty;
        set => StudentCode = value;
    }
    public DateOnly? AdmissionDate { get; set; }
    public string? ClassName { get; set; }
    public string? Section { get; set; }
    public string? RollNumber { get; set; }

    public string BatchName
    {
        get => !string.IsNullOrEmpty(ClassName)
            ? (!string.IsNullOrEmpty(Section) ? $"{ClassName} - {Section}" : ClassName)
            : _batchName;
        set => _batchName = value;
    }
    private string _batchName = string.Empty;

    // Parent Details
    public string? FatherName { get; set; }
    public string? FatherMobile { get; set; }
    public string? FatherAlternateMobile { get; set; }
    public string? MotherName { get; set; }

    public string ParentName
    {
        get => !string.IsNullOrEmpty(FatherName) ? FatherName : (!string.IsNullOrEmpty(MotherName) ? MotherName : _parentName);
        set => _parentName = value;
    }
    private string _parentName = string.Empty;

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

    // Status
    public bool IsActive { get; set; } = true;
    public string Status
    {
        get => IsActive ? "Active" : "Inactive";
        set => IsActive = value == "Active";
    }

    // Fee placeholders for UI compatibility
    public string FeeStatus { get; set; } = "Paid";
    public decimal TotalFee { get; set; } = 25000;
    public decimal PaidFee { get; set; } = 25000;
    public decimal DueFee => TotalFee - PaidFee;

    // Audit
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
#endregion

#region Create Student Request & Validator
public class CreateStudentRequest
{
    // User Profile
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Mobile { get; set; }
    public string? Password { get; set; }

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

    // Additional
    public string? AadharNumber { get; set; }
    public string? PreviousInstitute { get; set; }
    public string? MedicalNotes { get; set; }
    public bool IsActive { get; set; } = true;
}

public class CreateStudentRequestValidator : AbstractValidator<CreateStudentRequest>
{
    public CreateStudentRequestValidator()
    {
        ClassLevelCascadeMode = CascadeMode.Stop;
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("First name is required.")
            .MaximumLength(100).WithMessage("First name cannot exceed 100 characters.");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Last name is required.")
            .MaximumLength(100).WithMessage("Last name cannot exceed 100 characters.");

        RuleFor(x => x.Email)
            .MaximumLength(150).WithMessage("Email cannot exceed 150 characters.")
            .EmailAddress().When(x => !string.IsNullOrWhiteSpace(x.Email)).WithMessage("Enter a valid email address.");

        RuleFor(x => x.Mobile)
            .MaximumLength(20).WithMessage("Mobile cannot exceed 20 characters.");

        RuleFor(x => x.StudentCode)
            .MaximumLength(50).WithMessage("Student code cannot exceed 50 characters.");

        RuleFor(x => x.ClassName)
            .MaximumLength(100).WithMessage("Class name cannot exceed 100 characters.");

        RuleFor(x => x.Section)
            .MaximumLength(50).WithMessage("Section cannot exceed 50 characters.");

        RuleFor(x => x.RollNumber)
            .MaximumLength(50).WithMessage("Roll number cannot exceed 50 characters.");
    }
}
#endregion

#region Update Student Request & Validator
public class UpdateStudentRequest
{
    public Guid Id { get; set; }

    // User Profile
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Mobile { get; set; }

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

    // Additional
    public string? AadharNumber { get; set; }
    public string? PreviousInstitute { get; set; }
    public string? MedicalNotes { get; set; }
    public bool IsActive { get; set; } = true;
}

public class UpdateStudentRequestValidator : AbstractValidator<UpdateStudentRequest>
{
    public UpdateStudentRequestValidator()
    {
        ClassLevelCascadeMode = CascadeMode.Stop;
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Student ID is required.");

        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("First name is required.")
            .MaximumLength(100).WithMessage("First name cannot exceed 100 characters.");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Last name is required.")
            .MaximumLength(100).WithMessage("Last name cannot exceed 100 characters.");

        RuleFor(x => x.Email)
            .MaximumLength(150).WithMessage("Email cannot exceed 150 characters.")
            .EmailAddress().When(x => !string.IsNullOrWhiteSpace(x.Email)).WithMessage("Enter a valid email address.");

        RuleFor(x => x.Mobile)
            .MaximumLength(20).WithMessage("Mobile cannot exceed 20 characters.");

        RuleFor(x => x.StudentCode)
            .MaximumLength(50).WithMessage("Student code cannot exceed 50 characters.");

        RuleFor(x => x.ClassName)
            .MaximumLength(100).WithMessage("Class name cannot exceed 100 characters.");

        RuleFor(x => x.Section)
            .MaximumLength(50).WithMessage("Section cannot exceed 50 characters.");

        RuleFor(x => x.RollNumber)
            .MaximumLength(50).WithMessage("Roll number cannot exceed 50 characters.");
    }
}
#endregion

#region Generated Student Code & Roll Response DTO
public class GeneratedStudentCodeDto
{
    public string StudentCode { get; set; } = string.Empty;
    public string RollNumber { get; set; } = string.Empty;
}
#endregion

