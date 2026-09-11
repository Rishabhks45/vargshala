using FluentValidation;

namespace Vargshala.Contracts.Fees;

public class AssignStudentFeeRequestValidator : AbstractValidator<AssignStudentFeeRequest>
{
    public AssignStudentFeeRequestValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;
        ClassLevelCascadeMode = CascadeMode.Stop;

        RuleFor(x => x.StudentId)
            .NotEmpty().WithMessage("Please select a valid student.");

        RuleFor(x => x.FeeStructureId)
            .NotEmpty().WithMessage("Please select a valid fee structure.");

        RuleFor(x => x.InstallmentsCount)
            .InclusiveBetween(1, 12).WithMessage("Installments count must be between 1 and 12.");

        When(x => !string.IsNullOrWhiteSpace(x.DiscountType), () =>
        {
            RuleFor(x => x.DiscountValue)
                .NotNull().WithMessage("Discount value is required when discount type is selected.")
                .GreaterThan(0).WithMessage("Discount value must be greater than 0.");
        });

        When(x => x.DiscountType == "Percentage", () =>
        {
            RuleFor(x => x.DiscountValue)
                .LessThanOrEqualTo(100).WithMessage("Discount percentage cannot exceed 100%.");
        });
    }
}

public class CollectPaymentRequestValidator : AbstractValidator<CollectPaymentRequest>
{
    public CollectPaymentRequestValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;
        ClassLevelCascadeMode = CascadeMode.Stop;

        RuleFor(x => x.StudentId)
            .NotEmpty().WithMessage("Student selection is required.");

        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Payment amount must be greater than 0.");

        RuleFor(x => x.PaymentMethod)
            .NotEmpty().WithMessage("Payment method is required.")
            .Must(m => new[] { "Cash", "UPI", "NetBanking", "Card", "Cheque", "DemandDraft" }.Contains(m))
            .WithMessage("Invalid payment method selected.");

        When(x => x.PaymentMethod is "UPI" or "NetBanking" or "Card" or "Cheque" or "DemandDraft", () =>
        {
            RuleFor(x => x.TransactionReference)
                .MaximumLength(100).WithMessage("Transaction reference cannot exceed 100 characters.");
        });

        RuleFor(x => x.Remarks)
            .MaximumLength(500).WithMessage("Remarks cannot exceed 500 characters.");
    }
}
