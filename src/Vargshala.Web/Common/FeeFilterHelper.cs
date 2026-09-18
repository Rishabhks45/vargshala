using Vargshala.SharedKernel.Enums;
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

    /// <summary>
    /// Calculates the net payable amount after applying percentage or fixed discount.
    /// Used across OrgAdmin & BranchAdmin Fee Assignment Drawers.
    /// </summary>
    public static decimal CalculateNetPayable(decimal originalAmount, string? discountType, decimal? discountValue)
    {
        if (originalAmount <= 0) return 0;
        if (discountValue.HasValue && discountValue.Value > 0)
        {
            if (discountType == nameof(FeeDiscountType.Percentage))
            {
                var disc = Math.Round(originalAmount * (discountValue.Value / 100m), 2);
                return Math.Max(0, originalAmount - disc);
            }
            return Math.Max(0, originalAmount - discountValue.Value);
        }
        return originalAmount;
    }

    /// <summary>
    /// Gets the current financial/academic session string based on April cutoff.
    /// </summary>
    public static string GetCurrentAcademicSession()
    {
        return DateTime.UtcNow.Month >= 4
            ? $"{DateTime.UtcNow.Year}-{DateTime.UtcNow.Year + 1}"
            : $"{DateTime.UtcNow.Year - 1}-{DateTime.UtcNow.Year}";
    }

    /// <summary>
    /// Generates session dropdown options (e.g. 2025-2026, 2026-2027, 2027-2028).
    /// </summary>
    public static List<CustomSelect.CustomSelectOption> GetSessionOptions()
    {
        var currentYear = DateTime.UtcNow.Year;
        return new List<CustomSelect.CustomSelectOption>
        {
            new($"{currentYear - 1}-{currentYear}", $"{currentYear - 1}-{currentYear}"),
            new($"{currentYear}-{currentYear + 1}", $"{currentYear}-{currentYear + 1}"),
            new($"{currentYear + 1}-{currentYear + 2}", $"{currentYear + 1}-{currentYear + 2}")
        };
    }

    /// <summary>
    /// Generates session filter options with an optional "All Sessions" option.
    /// </summary>
    public static List<CustomSelect.CustomSelectOption> GetSessionFilterOptions(string allLabel = "All Sessions")
    {
        var options = new List<CustomSelect.CustomSelectOption> { new("", allLabel) };
        options.AddRange(GetSessionOptions());
        return options;
    }

    /// <summary>
    /// Generates status filter options (All, Active, Inactive).
    /// </summary>
    public static List<CustomSelect.CustomSelectOption> GetEntityStatusFilterOptions(string allLabel = "All Status")
    {
        return new List<CustomSelect.CustomSelectOption>
        {
            new("", allLabel),
            new("Active", "Active"),
            new("Inactive", "Inactive")
        };
    }

    /// <summary>
    /// Returns standardized Tailwind badge CSS class for student fee status.
    /// </summary>
    public static string GetStudentFeeStatusBadgeClass(string? status) => status switch
    {
        nameof(StudentFeeStatus.Paid) => "inline-flex items-center px-2 py-0.5 rounded-full text-[10px] font-bold bg-emerald-50 dark:bg-emerald-950/70 text-emerald-700 dark:text-emerald-300 border border-emerald-200/80 dark:border-emerald-800",
        nameof(StudentFeeStatus.PartiallyPaid) => "inline-flex items-center px-2 py-0.5 rounded-full text-[10px] font-bold bg-amber-50 dark:bg-amber-950/70 text-amber-800 dark:text-amber-300 border border-amber-200/80 dark:border-amber-800",
        nameof(StudentFeeStatus.Overdue) => "inline-flex items-center px-2 py-0.5 rounded-full text-[10px] font-bold bg-rose-50 dark:bg-rose-950/70 text-rose-700 dark:text-rose-300 border border-rose-200/80 dark:border-rose-800",
        _ => "inline-flex items-center px-2 py-0.5 rounded-full text-[10px] font-bold bg-slate-100 dark:bg-slate-800 text-slate-600 dark:text-slate-400 border border-slate-200 dark:border-slate-700"
    };

    /// <summary>
    /// Returns standardized user-friendly label for student fee status (e.g. "Partial" instead of "PartiallyPaid").
    /// </summary>
    public static string GetStudentFeeStatusDisplayName(string? status) => status switch
    {
        nameof(StudentFeeStatus.Paid) => "Paid",
        nameof(StudentFeeStatus.PartiallyPaid) => "Partial",
        nameof(StudentFeeStatus.Overdue) => "Overdue",
        _ => "Pending"
    };

    /// <summary>
    /// Returns standardized Tailwind badge CSS class for installment status.
    /// </summary>
    public static string GetInstallmentStatusBadgeClass(string? status) => status switch
    {
        "Paid" => "inline-flex items-center px-2 py-0.5 rounded-full text-[10px] font-bold bg-emerald-50 text-emerald-700 dark:bg-emerald-950/70 dark:text-emerald-300 border border-emerald-200 dark:border-emerald-800",
        "PartiallyPaid" => "inline-flex items-center px-2 py-0.5 rounded-full text-[10px] font-bold bg-amber-50 text-amber-700 dark:bg-amber-950/70 dark:text-amber-300 border border-amber-200 dark:border-amber-800",
        "Overdue" => "inline-flex items-center px-2 py-0.5 rounded-full text-[10px] font-bold bg-rose-50 text-rose-700 dark:bg-rose-950/70 dark:text-rose-300 border border-rose-200 dark:border-rose-800",
        _ => "inline-flex items-center px-2 py-0.5 rounded-full text-[10px] font-bold bg-slate-100 text-slate-600 dark:bg-slate-800 dark:text-slate-400 border border-slate-200 dark:border-slate-700"
    };
}
