using FluentValidation;

namespace Vargshala.Contracts.FeeStructures;

public class CreateFeeStructureRequestValidator : AbstractValidator<CreateFeeStructureRequest>
{
    public CreateFeeStructureRequestValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;
        ClassLevelCascadeMode = CascadeMode.Stop;

        RuleFor(x => x.BranchId)
            .NotEmpty().WithMessage("Branch selection is required.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Fee structure name is required.")
            .MaximumLength(150).WithMessage("Fee structure name cannot exceed 150 characters.");

        RuleFor(x => x.TotalAmount)
            .GreaterThanOrEqualTo(0).WithMessage("Total amount must be greater than or equal to 0.");

        RuleFor(x => x.AcademicSession)
            .NotEmpty().WithMessage("Academic session is required.")
            .MaximumLength(50).WithMessage("Academic session cannot exceed 50 characters.");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Description cannot exceed 500 characters.");
    }
}

public class UpdateFeeStructureRequestValidator : AbstractValidator<UpdateFeeStructureRequest>
{
    public UpdateFeeStructureRequestValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;
        ClassLevelCascadeMode = CascadeMode.Stop;

        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Fee structure ID is required.");

        RuleFor(x => x.BranchId)
            .NotEmpty().WithMessage("Branch selection is required.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Fee structure name is required.")
            .MaximumLength(150).WithMessage("Fee structure name cannot exceed 150 characters.");

        RuleFor(x => x.TotalAmount)
            .GreaterThanOrEqualTo(0).WithMessage("Total amount must be greater than or equal to 0.");

        RuleFor(x => x.AcademicSession)
            .NotEmpty().WithMessage("Academic session is required.")
            .MaximumLength(50).WithMessage("Academic session cannot exceed 50 characters.");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Description cannot exceed 500 characters.");
    }
}
