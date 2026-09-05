using FluentValidation;

namespace Vargshala.Contracts.Branches;

public class BranchDto
{
    public Guid Id { get; set; }
    public Guid OrganizationId { get; set; }

    // Branch Details
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? LogoUrl { get; set; }
    public string? Email { get; set; }
    public string? Mobile { get; set; }
    public string? AlternateMobile { get; set; }

    // Address
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? Pincode { get; set; }
    public string? Country { get; set; }

    // Branch Settings
    public bool IsMainBranch { get; set; }
    public bool UseBranchName { get; set; } = true;

    // Status & Audit
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    // Associated Counts (optional for UI)
    public int TotalStudentsCount { get; set; }
    public int ActiveBatchesCount { get; set; }

    // Branch Administrator (Head of Branch)
    public Guid? BranchAdminId { get; set; }
    public string? BranchAdminName { get; set; }
    public string? BranchAdminEmail { get; set; }
    public string? BranchAdminMobile { get; set; }
}

public class CreateBranchRequest
{
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? LogoUrl { get; set; }
    public string? Email { get; set; }
    public string? Mobile { get; set; }
    public string? AlternateMobile { get; set; }

    public string? Address { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? Pincode { get; set; }
    public string? Country { get; set; }

    public bool IsMainBranch { get; set; } = false;
    public bool UseBranchName { get; set; } = true;
    public bool IsActive { get; set; } = true;

    // Branch Administrator User Details (Assigned BranchAdmin role)
    public string? AdminFirstName { get; set; }
    public string? AdminLastName { get; set; }
    public string? AdminEmail { get; set; }
    public string? AdminMobile { get; set; }
    public string? AdminPassword { get; set; }
}

public class CreateBranchRequestValidator : AbstractValidator<CreateBranchRequest>
{
    public CreateBranchRequestValidator()
    {
        ClassLevelCascadeMode = CascadeMode.Stop;
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Branch Name is required.")
            .MaximumLength(200).WithMessage("Branch Name cannot exceed 200 characters.");

        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Branch Code is required.")
            .MaximumLength(50).WithMessage("Branch Code cannot exceed 50 characters.");

        RuleFor(x => x.Email)
            .EmailAddress().WithMessage("Please enter a valid email address.")
            .MaximumLength(150).WithMessage("Email cannot exceed 150 characters.")
            .When(x => !string.IsNullOrWhiteSpace(x.Email));

        RuleFor(x => x.Mobile)
            .MaximumLength(20).WithMessage("Mobile number cannot exceed 20 characters.");

        RuleFor(x => x.AlternateMobile)
            .MaximumLength(20).WithMessage("Alternate mobile number cannot exceed 20 characters.");

        RuleFor(x => x.Address)
            .MaximumLength(500).WithMessage("Address cannot exceed 500 characters.");

        RuleFor(x => x.City)
            .MaximumLength(100).WithMessage("City cannot exceed 100 characters.");

        RuleFor(x => x.State)
            .MaximumLength(100).WithMessage("State cannot exceed 100 characters.");

        RuleFor(x => x.Pincode)
            .MaximumLength(10).WithMessage("Pincode cannot exceed 10 characters.");

        RuleFor(x => x.Country)
            .MaximumLength(100).WithMessage("Country cannot exceed 100 characters.");

        When(x => !string.IsNullOrWhiteSpace(x.AdminEmail) || !string.IsNullOrWhiteSpace(x.AdminFirstName), () =>
        {
            RuleFor(x => x.AdminFirstName)
                .NotEmpty().WithMessage("Branch Admin First Name is required.")
                .MaximumLength(100).WithMessage("First name cannot exceed 100 characters.");

            RuleFor(x => x.AdminLastName)
                .MaximumLength(100).WithMessage("Last name cannot exceed 100 characters.");

            RuleFor(x => x.AdminEmail)
                .NotEmpty().WithMessage("Branch Admin Email is required.")
                .EmailAddress().WithMessage("Please enter a valid email address for Branch Admin.")
                .MaximumLength(150).WithMessage("Email cannot exceed 150 characters.");

            RuleFor(x => x.AdminPassword)
                .NotEmpty().WithMessage("Branch Admin Password is required.")
                .MinimumLength(6).WithMessage("Password must be at least 6 characters.");

            RuleFor(x => x.AdminMobile)
                .MaximumLength(20).WithMessage("Mobile number cannot exceed 20 characters.");
        });
    }
}

public class UpdateBranchRequest
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? LogoUrl { get; set; }
    public string? Email { get; set; }
    public string? Mobile { get; set; }
    public string? AlternateMobile { get; set; }

    public string? Address { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? Pincode { get; set; }
    public string? Country { get; set; }

    public bool IsMainBranch { get; set; }
    public bool UseBranchName { get; set; } = true;
    public bool IsActive { get; set; } = true;
}

public class UpdateBranchRequestValidator : AbstractValidator<UpdateBranchRequest>
{
    public UpdateBranchRequestValidator()
    {
        ClassLevelCascadeMode = CascadeMode.Stop;
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Branch ID is required.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Branch Name is required.")
            .MaximumLength(200).WithMessage("Branch Name cannot exceed 200 characters.");

        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Branch Code is required.")
            .MaximumLength(50).WithMessage("Branch Code cannot exceed 50 characters.");

        RuleFor(x => x.Email)
            .EmailAddress().WithMessage("Please enter a valid email address.")
            .MaximumLength(150).WithMessage("Email cannot exceed 150 characters.")
            .When(x => !string.IsNullOrWhiteSpace(x.Email));

        RuleFor(x => x.Mobile)
            .MaximumLength(20).WithMessage("Mobile number cannot exceed 20 characters.");

        RuleFor(x => x.AlternateMobile)
            .MaximumLength(20).WithMessage("Alternate mobile number cannot exceed 20 characters.");

        RuleFor(x => x.Address)
            .MaximumLength(500).WithMessage("Address cannot exceed 500 characters.");

        RuleFor(x => x.City)
            .MaximumLength(100).WithMessage("City cannot exceed 100 characters.");

        RuleFor(x => x.State)
            .MaximumLength(100).WithMessage("State cannot exceed 100 characters.");

        RuleFor(x => x.Pincode)
            .MaximumLength(10).WithMessage("Pincode cannot exceed 10 characters.");

        RuleFor(x => x.Country)
            .MaximumLength(100).WithMessage("Country cannot exceed 100 characters.");
    }
}

public class UserBranchAccessDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid BranchId { get; set; }
    public string BranchName { get; set; } = string.Empty;
    public string BranchCode { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
}

public class AssignUserBranchesRequest
{
    public Guid UserId { get; set; }
    public List<Guid> BranchIds { get; set; } = new();
}

public class AssignUserBranchesRequestValidator : AbstractValidator<AssignUserBranchesRequest>
{
    public AssignUserBranchesRequestValidator()
    {
        ClassLevelCascadeMode = CascadeMode.Stop;
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required.");
    }
}
