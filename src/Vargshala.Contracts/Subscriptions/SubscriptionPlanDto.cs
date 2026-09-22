using FluentValidation;
using Vargshala.Contracts.Common;
using Vargshala.SharedKernel.Enums;

namespace Vargshala.Contracts.Subscriptions;

public class SubscriptionPlanDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public BillingCycle BillingCycle { get; set; } = BillingCycle.Monthly;
    public string BillingCycleName => BillingCycle.GetDisplayName();

    /// <summary>
    /// Student capacity limit. NULL indicates unlimited.
    /// </summary>
    public int? MaxStudents { get; set; }
    public string MaxStudentsDisplay => MaxStudents.HasValue ? MaxStudents.Value.ToString("N0") : "Unlimited";

    /// <summary>
    /// Teacher capacity limit. NULL indicates unlimited.
    /// </summary>
    public int? MaxTeachers { get; set; }
    public string MaxTeachersDisplay => MaxTeachers.HasValue ? MaxTeachers.Value.ToString("N0") : "Unlimited";

    /// <summary>
    /// Branch capacity limit. NULL indicates unlimited.
    /// </summary>
    public int? MaxBranches { get; set; }
    public string MaxBranchesDisplay => MaxBranches.HasValue ? MaxBranches.Value.ToString("N0") : "Unlimited";

    public bool IsActive { get; set; } = true;
    public int ActiveInstitutesCount { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class CreateSubscriptionPlanRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public BillingCycle BillingCycle { get; set; } = BillingCycle.Monthly;
    public int? MaxStudents { get; set; }
    public int? MaxTeachers { get; set; }
    public int? MaxBranches { get; set; }
    public bool IsActive { get; set; } = true;
}

public class CreateSubscriptionPlanRequestValidator : AbstractValidator<CreateSubscriptionPlanRequest>
{
    public CreateSubscriptionPlanRequestValidator()
    {
        ClassLevelCascadeMode = CascadeMode.Stop;
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Plan name is required.")
            .MaximumLength(100).WithMessage("Plan name cannot exceed 100 characters.");

        RuleFor(x => x.Price)
            .GreaterThanOrEqualTo(0).WithMessage("Price must be 0 or greater.");

        RuleFor(x => x.BillingCycle)
            .IsInEnum().WithMessage("Valid billing cycle must be selected.");

        RuleFor(x => x.MaxStudents)
            .GreaterThan(0).When(x => x.MaxStudents.HasValue).WithMessage("Max students must be greater than 0 if specified.");

        RuleFor(x => x.MaxTeachers)
            .GreaterThan(0).When(x => x.MaxTeachers.HasValue).WithMessage("Max teachers must be greater than 0 if specified.");

        RuleFor(x => x.MaxBranches)
            .GreaterThan(0).When(x => x.MaxBranches.HasValue).WithMessage("Max branches must be greater than 0 if specified.");
    }
}

public class UpdateSubscriptionPlanRequest
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public BillingCycle BillingCycle { get; set; } = BillingCycle.Monthly;
    public int? MaxStudents { get; set; }
    public int? MaxTeachers { get; set; }
    public int? MaxBranches { get; set; }
    public bool IsActive { get; set; } = true;
}

public class UpdateSubscriptionPlanRequestValidator : AbstractValidator<UpdateSubscriptionPlanRequest>
{
    public UpdateSubscriptionPlanRequestValidator()
    {
        ClassLevelCascadeMode = CascadeMode.Stop;
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Plan ID is required.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Plan name is required.")
            .MaximumLength(100).WithMessage("Plan name cannot exceed 100 characters.");

        RuleFor(x => x.Price)
            .GreaterThanOrEqualTo(0).WithMessage("Price must be 0 or greater.");

        RuleFor(x => x.BillingCycle)
            .IsInEnum().WithMessage("Valid billing cycle must be selected.");

        RuleFor(x => x.MaxStudents)
            .GreaterThan(0).When(x => x.MaxStudents.HasValue).WithMessage("Max students must be greater than 0 if specified.");

        RuleFor(x => x.MaxTeachers)
            .GreaterThan(0).When(x => x.MaxTeachers.HasValue).WithMessage("Max teachers must be greater than 0 if specified.");

        RuleFor(x => x.MaxBranches)
            .GreaterThan(0).When(x => x.MaxBranches.HasValue).WithMessage("Max branches must be greater than 0 if specified.");
    }
}
