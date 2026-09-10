using System.ComponentModel.DataAnnotations;

namespace Vargshala.Contracts.Common;

/// <summary>
/// Defines marketing and promotional campaign categories for coupons and discounts.
/// </summary>
public enum CampaignCategory
{
    [Display(Name = "Launch Offer")]
    LaunchOffer = 1,

    [Display(Name = "Promotional")]
    Promotional = 2,

    [Display(Name = "Seasonal / Festive")]
    Seasonal = 3,

    [Display(Name = "VIP / Corporate")]
    VipPromo = 4,

    [Display(Name = "Retention / Winback")]
    Retention = 5,

    [Display(Name = "General")]
    General = 6
}

/// <summary>
/// Defines how a discount is calculated: percentage or flat monetary deduction.
/// </summary>
public enum DiscountType
{
    [Display(Name = "Percentage (%)")]
    Percentage = 1,

    [Display(Name = "Flat Amount (₹)")]
    FlatAmount = 2
}

/// <summary>
/// Defines the subscription plan tier or scope to which a coupon applies.
/// </summary>
public enum ApplicablePlan
{
    [Display(Name = "All Plans")]
    AllPlans = 1,

    [Display(Name = "Standard Plan")]
    Standard = 2,

    [Display(Name = "Pro Institute")]
    ProInstitute = 3,

    [Display(Name = "Enterprise")]
    Enterprise = 4
}
