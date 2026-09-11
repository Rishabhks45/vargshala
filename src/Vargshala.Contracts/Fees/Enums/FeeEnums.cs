using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace Vargshala.Contracts.Fees.Enums;

public enum StudentFeeStatus
{
    [Display(Name = "Pending")]
    Pending = 1,

    [Display(Name = "Partially Paid")]
    PartiallyPaid = 2,

    [Display(Name = "Paid")]
    Paid = 3,

    [Display(Name = "Overdue")]
    Overdue = 4,

    [Display(Name = "Cancelled")]
    Cancelled = 5
}

public enum PaymentMethod
{
    [Display(Name = "UPI / QR Code")]
    UPI = 1,

    [Display(Name = "Cash")]
    Cash = 2,

    [Display(Name = "Net Banking / NEFT")]
    NetBanking = 3,

    [Display(Name = "Debit / Credit Card")]
    Card = 4,

    [Display(Name = "Cheque")]
    Cheque = 5,

    [Display(Name = "Demand Draft")]
    DemandDraft = 6
}

public enum PaymentStatus
{
    [Display(Name = "Completed")]
    Completed = 1,

    [Display(Name = "Pending")]
    Pending = 2,

    [Display(Name = "Failed")]
    Failed = 3,

    [Display(Name = "Refunded")]
    Refunded = 4
}

public enum FeeDiscountType
{
    [Display(Name = "Fixed Amount (₹)")]
    FixedAmount = 1,

    [Display(Name = "Percentage (%)")]
    Percentage = 2,

    [Display(Name = "Merit Scholarship")]
    Scholarship = 3,

    [Display(Name = "Sibling Waiver")]
    Sibling = 4
}

public static class FeeEnumExtensions
{
    public static string GetDisplayName(this Enum enumValue)
    {
        var displayAttribute = enumValue.GetType()
            .GetField(enumValue.ToString())?
            .GetCustomAttribute<DisplayAttribute>();

        return displayAttribute?.Name ?? enumValue.ToString();
    }
}
