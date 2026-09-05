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

public static class CampaignCategoryNames
{
    public const string LaunchOffer = "Launch Offer";
    public const string Promotional = "Promotional";
    public const string Seasonal = "Seasonal / Festive";
    public const string VipPromo = "VIP / Corporate";
    public const string Retention = "Retention / Winback";
    public const string General = "General";
}

public static class CampaignCategoryExtensions
{
    public static string GetDisplayName(this CampaignCategory category) => category switch
    {
        CampaignCategory.LaunchOffer => "Launch Offer",
        CampaignCategory.Promotional => "Promotional",
        CampaignCategory.Seasonal => "Seasonal / Festive",
        CampaignCategory.VipPromo => "VIP / Corporate",
        CampaignCategory.Retention => "Retention / Winback",
        CampaignCategory.General => "General",
        _ => category.ToString()
    };
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

public static class DiscountTypeNames
{
    public const string Percentage = "Percentage";
    public const string FlatAmount = "Flat Amount";
}

public static class DiscountTypeExtensions
{
    public static string GetDisplayName(this DiscountType discountType) => discountType switch
    {
        DiscountType.Percentage => "Percentage (%)",
        DiscountType.FlatAmount => "Flat Amount (₹)",
        _ => discountType.ToString()
    };
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

public static class ApplicablePlanNames
{
    public const string AllPlans = "All Plans";
    public const string Standard = "Standard";
    public const string ProInstitute = "Pro Institute";
    public const string Enterprise = "Enterprise";
}

public static class ApplicablePlanExtensions
{
    public static string GetDisplayName(this ApplicablePlan plan) => plan switch
    {
        ApplicablePlan.AllPlans => "All Plans",
        ApplicablePlan.Standard => "Standard",
        ApplicablePlan.ProInstitute => "Pro Institute",
        ApplicablePlan.Enterprise => "Enterprise",
        _ => plan.ToString()
    };
}
