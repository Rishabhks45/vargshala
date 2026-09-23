using FluentValidation;

namespace Vargshala.Contracts.Subscriptions;

public class SubscriptionCheckoutRequest
{
    public Guid PlanId { get; set; }
    public string? CouponCode { get; set; }
}

public class SubscriptionCheckoutRequestValidator : AbstractValidator<SubscriptionCheckoutRequest>
{
    public SubscriptionCheckoutRequestValidator()
    {
        ClassLevelCascadeMode = CascadeMode.Stop;
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(x => x.PlanId)
            .NotEmpty().WithMessage("Subscription plan must be selected.");
    }
}

public class SubscriptionCheckoutResponse
{
    public string RazorpayOrderId { get; set; } = string.Empty;
    public string RazorpayKeyId { get; set; } = string.Empty;
    public decimal OriginalAmount { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal FinalAmount { get; set; }
    public string Currency { get; set; } = "INR";
    public string PlanName { get; set; } = string.Empty;
    public string CompanyName { get; set; } = "Vargshala";
    public string ThemeColor { get; set; } = "#009488";
    public string? CouponCode { get; set; }
    public string? CouponDescription { get; set; }
    public bool IsFreePlan { get; set; }
}
