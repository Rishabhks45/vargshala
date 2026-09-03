using FluentValidation;

namespace Vargshala.Contracts.Authentication;

#region Request
public class RegisterOrganizationRequest
{
    // Organization Details (matches 01_Organizations.sql)
    public string OrganizationName { get; set; } = string.Empty;
    public string OrganizationCode { get; set; } = string.Empty;
    public string Name { get => OrganizationName; set => OrganizationName = value; }
    public string Code { get => OrganizationCode; set => OrganizationCode = value; }

    public string InstituteType { get; set; } = "Coaching Institute";
    public string? LogoUrl { get; set; }
    public string? Email { get; set; }
    public string? Mobile { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? Pincode { get; set; }
    public string? AcademicSession { get; set; }

    // Super Admin Account Details (matches 02_Users.sql)
    public string AdminFirstName { get; set; } = string.Empty;
    public string AdminLastName { get; set; } = string.Empty;
    public string AdminEmail { get; set; } = string.Empty;
    public string? AdminMobile { get; set; }
    public string Password { get; set; } = string.Empty;
    public string ConfirmPassword { get; set; } = string.Empty;

    public bool AcceptTerms { get; set; } = false;
}

public class RegisterOrganizationRequestValidator : AbstractValidator<RegisterOrganizationRequest>
{
    public RegisterOrganizationRequestValidator()
    {
        ClassLevelCascadeMode = CascadeMode.Stop;
        RuleLevelCascadeMode = CascadeMode.Stop;

        // Organization Rules (matching 01_Organizations.sql: VARCHAR lengths)
        RuleFor(x => x.OrganizationName)
            .NotEmpty().WithMessage("Organization name is required.")
            .MinimumLength(3).WithMessage("Organization name must be at least 3 characters.")
            .MaximumLength(200).WithMessage("Organization name cannot exceed 200 characters.");

        RuleFor(x => x.OrganizationCode)
            .NotEmpty().WithMessage("Organization code is required.")
            .MinimumLength(2).WithMessage("Organization code must be at least 2 characters.")
            .MaximumLength(50).WithMessage("Organization code cannot exceed 50 characters.")
            .Matches(@"^[A-Za-z0-9\-_]+$").WithMessage("Code can only contain alphanumeric characters, hyphens or underscores.");

        RuleFor(x => x.Email)
            .MaximumLength(150).WithMessage("Organization email cannot exceed 150 characters.")
            .EmailAddress().When(x => !string.IsNullOrWhiteSpace(x.Email)).WithMessage("Enter a valid organization email address.");

        RuleFor(x => x.Mobile)
            .MaximumLength(20).WithMessage("Organization mobile number cannot exceed 20 characters.");

        RuleFor(x => x.Address)
            .MaximumLength(500).WithMessage("Address cannot exceed 500 characters.");

        RuleFor(x => x.City)
            .MaximumLength(100).WithMessage("City cannot exceed 100 characters.");

        RuleFor(x => x.State)
            .MaximumLength(100).WithMessage("State cannot exceed 100 characters.");

        RuleFor(x => x.Pincode)
            .MaximumLength(10).WithMessage("Pincode cannot exceed 10 characters.");

        RuleFor(x => x.AcademicSession)
            .MaximumLength(20).WithMessage("Academic session cannot exceed 20 characters.");

        // Super Admin Rules (matching 02_Users.sql: VARCHAR lengths)
        RuleFor(x => x.AdminFirstName)
            .NotEmpty().WithMessage("Admin first name is required.")
            .MaximumLength(100).WithMessage("First name cannot exceed 100 characters.");

        RuleFor(x => x.AdminLastName)
            .NotEmpty().WithMessage("Admin last name is required.")
            .MaximumLength(100).WithMessage("Last name cannot exceed 100 characters.");

        RuleFor(x => x.AdminEmail)
            .NotEmpty().WithMessage("Admin email is required.")
            .MaximumLength(150).WithMessage("Admin email cannot exceed 150 characters.")
            .EmailAddress().WithMessage("Enter a valid admin email address.");

        RuleFor(x => x.AdminMobile)
            .MaximumLength(20).WithMessage("Admin mobile number cannot exceed 20 characters.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.")
            .MinimumLength(6).WithMessage("Password must be at least 6 characters long.")
            .DependentRules(() =>
            {
                RuleFor(x => x.ConfirmPassword)
                    .NotEmpty().WithMessage("Please confirm your password.")
                    .Equal(x => x.Password).WithMessage("Passwords do not match.");
            });

        RuleFor(x => x.AcceptTerms)
            .Equal(true).WithMessage("You must accept the terms of service to register.");
    }
}
#endregion

#region Response
public class RegisterOrganizationResponse
{
    public Guid OrganizationId { get; set; }
    public string OrganizationName { get; set; } = string.Empty;
    public string OrganizationCode { get; set; } = string.Empty;
    public Guid AdminUserId { get; set; }
    public string AdminEmail { get; set; } = string.Empty;
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public string Message { get; set; } = "Organization registered successfully.";
}
#endregion
