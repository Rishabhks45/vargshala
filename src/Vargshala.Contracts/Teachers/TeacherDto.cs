using FluentValidation;

namespace Vargshala.Contracts.Teachers;

#region Teacher DTO
public class TeacherDto
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
            if (string.IsNullOrWhiteSpace(display)) return "TR";
            var parts = display.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            return parts.Length > 1
                ? $"{parts[0][0]}{parts[1][0]}".ToUpper()
                : $"{parts[0][0]}".ToUpper();
        }
    }

    // Professional Details
    public string? EmployeeCode { get; set; }
    public string EmployeeId
    {
        get => EmployeeCode ?? string.Empty;
        set => EmployeeCode = value;
    }
    public DateOnly? JoiningDate { get; set; }
    public string? Department { get; set; }

    // Designation (Enum + Display)
    public Designation? Designation { get; set; }
    public string DesignationDisplay => Designation?.GetDisplayName() ?? (!string.IsNullOrWhiteSpace(_designationString) ? _designationString : string.Empty);
    public string DesignationName
    {
        get => DesignationDisplay;
        set
        {
            _designationString = value;
            var parsed = DesignationExtensions.ParseDesignation(value);
            if (parsed.HasValue) Designation = parsed.Value;
        }
    }
    private string? _designationString;

    // Qualification Details (Enum + Display)
    public HighestQualification? HighestQualification { get; set; }
    public string HighestQualificationDisplay => HighestQualification?.GetDisplayName() ?? (!string.IsNullOrWhiteSpace(_qualificationString) ? _qualificationString : string.Empty);
    public string Qualification
    {
        get => HighestQualificationDisplay;
        set
        {
            _qualificationString = value;
            var parsed = HighestQualificationExtensions.ParseQualification(value);
            if (parsed.HasValue) HighestQualification = parsed.Value;
        }
    }
    private string? _qualificationString;

    public string? Specialization { get; set; }
    public string Subject
    {
        get => Specialization ?? string.Empty;
        set => Specialization = value;
    }

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

    // Status
    public bool IsActive { get; set; } = true;
    public string Status
    {
        get => IsActive ? "Active" : "Inactive";
        set => IsActive = value == "Active";
    }

    // Backwards-compatibility with UI
    public List<string> Batches { get; set; } = new();
    public string BatchesDisplay => Batches.Count > 0 ? string.Join(", ", Batches) : "—";
    public int BatchCount => Batches.Count;

    // Audit
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
#endregion

#region Create Teacher Request & Validator
public class CreateTeacherRequest
{
    // User Profile
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Mobile { get; set; }
    public string? Password { get; set; }

    // Professional Details
    public string? EmployeeCode { get; set; }
    public DateOnly? JoiningDate { get; set; }
    public string? Department { get; set; }

    public Designation? Designation { get; set; }
    public string? DesignationName
    {
        get => Designation?.GetDisplayName();
        set => Designation = DesignationExtensions.ParseDesignation(value);
    }

    // Qualification Details
    public HighestQualification? HighestQualification { get; set; }
    public string? Qualification
    {
        get => HighestQualification?.GetDisplayName();
        set => HighestQualification = HighestQualificationExtensions.ParseQualification(value);
    }

    public string? Specialization { get; set; }
    public string? Subject
    {
        get => Specialization;
        set => Specialization = value;
    }

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
    public bool IsActive { get; set; } = true;
}

public class CreateTeacherRequestValidator : AbstractValidator<CreateTeacherRequest>
{
    public CreateTeacherRequestValidator()
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

        RuleFor(x => x.EmployeeCode)
            .MaximumLength(50).WithMessage("Employee code cannot exceed 50 characters.");

        RuleFor(x => x.Department)
            .MaximumLength(100).WithMessage("Department cannot exceed 100 characters.");

        RuleFor(x => x.Designation)
            .IsInEnum().When(x => x.Designation.HasValue).WithMessage("Select a valid designation.");

        RuleFor(x => x.HighestQualification)
            .IsInEnum().When(x => x.HighestQualification.HasValue).WithMessage("Select a valid qualification.");
    }
}
#endregion

#region Update Teacher Request & Validator
public class UpdateTeacherRequest
{
    public Guid Id { get; set; }

    // User Profile
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Mobile { get; set; }

    // Professional Details
    public string? EmployeeCode { get; set; }
    public DateOnly? JoiningDate { get; set; }
    public string? Department { get; set; }

    public Designation? Designation { get; set; }
    public string? DesignationName
    {
        get => Designation?.GetDisplayName();
        set => Designation = DesignationExtensions.ParseDesignation(value);
    }

    // Qualification Details
    public HighestQualification? HighestQualification { get; set; }
    public string? Qualification
    {
        get => HighestQualification?.GetDisplayName();
        set => HighestQualification = HighestQualificationExtensions.ParseQualification(value);
    }

    public string? Specialization { get; set; }
    public string? Subject
    {
        get => Specialization;
        set => Specialization = value;
    }

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
    public bool IsActive { get; set; } = true;
}

public class UpdateTeacherRequestValidator : AbstractValidator<UpdateTeacherRequest>
{
    public UpdateTeacherRequestValidator()
    {
        ClassLevelCascadeMode = CascadeMode.Stop;
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Teacher ID is required.");

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

        RuleFor(x => x.EmployeeCode)
            .MaximumLength(50).WithMessage("Employee code cannot exceed 50 characters.");

        RuleFor(x => x.Department)
            .MaximumLength(100).WithMessage("Department cannot exceed 100 characters.");

        RuleFor(x => x.Designation)
            .IsInEnum().When(x => x.Designation.HasValue).WithMessage("Select a valid designation.");

        RuleFor(x => x.HighestQualification)
            .IsInEnum().When(x => x.HighestQualification.HasValue).WithMessage("Select a valid qualification.");
    }
}
#endregion

#region Generated Teacher Code Response DTO
public class GeneratedTeacherCodeDto
{
    public string EmployeeCode { get; set; } = string.Empty;
}
#endregion
