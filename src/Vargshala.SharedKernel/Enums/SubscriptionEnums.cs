using System.ComponentModel.DataAnnotations;

namespace Vargshala.SharedKernel.Enums;

/// <summary>
/// Identifies the purpose and context of a financial payment transaction in Vargshala.
/// </summary>
public enum PaymentType
{
    [Display(Name = "Student Fee")]
    StudentFee = 1,

    [Display(Name = "SaaS Subscription")]
    Subscription = 2
}

/// <summary>
/// Lifecycle state of an educational organization's SaaS subscription.
/// </summary>
public enum SubscriptionStatus
{
    [Display(Name = "Free Trial")]
    Trial = 1,

    [Display(Name = "Active")]
    Active = 2,

    [Display(Name = "Expired")]
    Expired = 3,

    [Display(Name = "Cancelled")]
    Cancelled = 4,

    [Display(Name = "Suspended")]
    Suspended = 5
}

/// <summary>
/// Billing frequency for SaaS subscription plans.
/// </summary>
public enum BillingCycle
{
    [Display(Name = "Monthly")]
    Monthly = 1,

    [Display(Name = "Quarterly")]
    Quarterly = 2,

    [Display(Name = "Yearly")]
    Yearly = 3,

    [Display(Name = "7-Day Trial")]
    Trial = 4
}
