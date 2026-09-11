using Vargshala.Contracts.Fees.Enums;
using Vargshala.Web.Components.UI.Inputs;

namespace Vargshala.Web.Common;

/// <summary>
/// Universal UI helper for building standardized CustomSelect options from Fee Enums.
/// Dynamically reads enum values and [Display] names.
/// </summary>
public static class FeeFilterHelper
{
    public static List<CustomSelect.CustomSelectOption> GetStatusFilterOptions(string allLabel = "All Status")
    {
        var options = new List<CustomSelect.CustomSelectOption>
        {
            new("", allLabel)
        };

        foreach (var status in Enum.GetValues<StudentFeeStatus>())
        {
            options.Add(new(status.ToString(), status.GetDisplayName()));
        }

        return options;
    }

    public static List<CustomSelect.CustomSelectOption> GetPaymentMethodOptions()
    {
        return Enum.GetValues<PaymentMethod>()
            .Select(m => new CustomSelect.CustomSelectOption(m.ToString(), m.GetDisplayName()))
            .ToList();
    }

    public static List<CustomSelect.CustomSelectOption> GetDiscountTypeOptions(string noneLabel = "No Discount")
    {
        var options = new List<CustomSelect.CustomSelectOption>
        {
            new("", noneLabel)
        };

        foreach (var discount in Enum.GetValues<FeeDiscountType>())
        {
            options.Add(new(discount.ToString(), discount.GetDisplayName()));
        }

        return options;
    }

    public static List<CustomSelect.CustomSelectOption> GetInstallmentOptions()
    {
        var plans = new (int count, string label)[]
        {
            (1, "1 Installment (Full Payment)"),
            (2, "2 Installments (Bi-Annual)"),
            (3, "3 Installments (Term-wise)"),
            (4, "4 Installments (Quarterly)"),
            (6, "6 Installments (Bi-Monthly)"),
            (12, "12 Installments (Monthly)")
        };

        return plans
            .Select(p => new CustomSelect.CustomSelectOption(p.count.ToString(), p.label))
            .ToList();
    }
}
