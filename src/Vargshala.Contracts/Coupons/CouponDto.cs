using FluentValidation;
using Vargshala.Contracts.Common;

namespace Vargshala.Contracts.Coupons;

public class CouponDto
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid? OrganizationId { get; set; }

    public string Code { get; set; } = string.Empty;
    public CampaignCategory Category { get; set; } = CampaignCategory.Promotional;
    public string CategoryName => Category.GetDisplayName();
    public string? Description { get; set; }

    public DiscountType DiscountType { get; set; } = DiscountType.Percentage;
    public string DiscountTypeName => DiscountType.GetDisplayName();
    public decimal DiscountValue { get; set; }
    public decimal? MinOrderAmount { get; set; }
    public decimal? MaxDiscountAmount { get; set; }

    public ApplicablePlan ApplicablePlan { get; set; } = ApplicablePlan.AllPlans;
    public string ApplicablePlanName => ApplicablePlan.GetDisplayName();
    public int UsedCount { get; set; } = 0;
    public int MaxUses { get; set; } = 100;

    public DateTime ExpiryDate { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsExpired => ExpiryDate < DateTime.UtcNow.Date;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}

public class CreateCouponRequest
{
    public Guid? OrganizationId { get; set; }
    public string Code { get; set; } = string.Empty;
    public CampaignCategory Category { get; set; } = CampaignCategory.Promotional;
    public string? Description { get; set; }

    public DiscountType DiscountType { get; set; } = DiscountType.Percentage;
    public decimal DiscountValue { get; set; } = 10;
    public decimal? MinOrderAmount { get; set; }
    public decimal? MaxDiscountAmount { get; set; }

    public ApplicablePlan ApplicablePlan { get; set; } = ApplicablePlan.AllPlans;
    public int MaxUses { get; set; } = 100;
    public DateTime ExpiryDate { get; set; } = DateTime.UtcNow.AddMonths(1);
    public bool IsActive { get; set; } = true;
}

public class CreateCouponRequestValidator : AbstractValidator<CreateCouponRequest>
{
    public CreateCouponRequestValidator()
    {
        ClassLevelCascadeMode = CascadeMode.Stop;
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Coupon code is required.")
            .MaximumLength(50).WithMessage("Coupon code cannot exceed 50 characters.")
            .Matches(@"^[A-Z0-9_\-]+$").WithMessage("Code must be uppercase alphanumeric (e.g. WELCOME50).");

        RuleFor(x => x.DiscountValue)
            .GreaterThan(0).WithMessage("Discount value must be greater than zero.");

        When(x => x.DiscountType == DiscountType.Percentage, () =>
        {
            RuleFor(x => x.DiscountValue)
                .InclusiveBetween(1, 100).WithMessage("Percentage discount must be between 1% and 100%.");
        });

        RuleFor(x => x.MaxUses)
            .GreaterThan(0).WithMessage("Max redemptions quota must be at least 1.");

        RuleFor(x => x.ExpiryDate)
            .GreaterThan(DateTime.UtcNow.Date).WithMessage("Expiry date must be in the future.");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Description cannot exceed 500 characters.");
    }
}

public class UpdateCouponRequest
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public CampaignCategory Category { get; set; } = CampaignCategory.Promotional;
    public string? Description { get; set; }

    public DiscountType DiscountType { get; set; } = DiscountType.Percentage;
    public decimal DiscountValue { get; set; }
    public decimal? MinOrderAmount { get; set; }
    public decimal? MaxDiscountAmount { get; set; }

    public ApplicablePlan ApplicablePlan { get; set; } = ApplicablePlan.AllPlans;
    public int MaxUses { get; set; } = 100;
    public DateTime ExpiryDate { get; set; }
    public bool IsActive { get; set; } = true;
}

public class UpdateCouponRequestValidator : AbstractValidator<UpdateCouponRequest>
{
    public UpdateCouponRequestValidator()
    {
        ClassLevelCascadeMode = CascadeMode.Stop;
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Coupon identifier is required.");

        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Coupon code is required.")
            .MaximumLength(50).WithMessage("Coupon code cannot exceed 50 characters.")
            .Matches(@"^[A-Z0-9_\-]+$").WithMessage("Code must be uppercase alphanumeric (e.g. FESTIVE25).");

        RuleFor(x => x.DiscountValue)
            .GreaterThan(0).WithMessage("Discount value must be greater than zero.");

        When(x => x.DiscountType == DiscountType.Percentage, () =>
        {
            RuleFor(x => x.DiscountValue)
                .InclusiveBetween(1, 100).WithMessage("Percentage discount must be between 1% and 100%.");
        });

        RuleFor(x => x.MaxUses)
            .GreaterThan(0).WithMessage("Max redemptions quota must be at least 1.");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Description cannot exceed 500 characters.");
    }
}
