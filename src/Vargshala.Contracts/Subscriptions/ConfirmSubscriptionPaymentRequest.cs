using FluentValidation;

namespace Vargshala.Contracts.Subscriptions;

public class ConfirmSubscriptionPaymentRequest
{
    public Guid PlanId { get; set; }
    public string? CouponCode { get; set; }
    public string RazorpayOrderId { get; set; } = string.Empty;
    public string RazorpayPaymentId { get; set; } = string.Empty;
    public string RazorpaySignature { get; set; } = string.Empty;
}

public class ConfirmSubscriptionPaymentRequestValidator : AbstractValidator<ConfirmSubscriptionPaymentRequest>
{
    public ConfirmSubscriptionPaymentRequestValidator()
    {
        ClassLevelCascadeMode = CascadeMode.Stop;
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(x => x.PlanId)
            .NotEmpty().WithMessage("Subscription plan ID is required.");

        RuleFor(x => x.RazorpayOrderId)
            .NotEmpty().WithMessage("Razorpay Order ID is required.");

        RuleFor(x => x.RazorpayPaymentId)
            .NotEmpty().WithMessage("Razorpay Payment ID is required.");

        RuleFor(x => x.RazorpaySignature)
            .NotEmpty().WithMessage("Razorpay Signature is required.");
    }
}
