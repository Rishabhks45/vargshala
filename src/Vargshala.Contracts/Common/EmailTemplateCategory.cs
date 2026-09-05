using System.ComponentModel.DataAnnotations;

namespace Vargshala.Contracts.Common;

public enum EmailTemplateCategory
{
    [Display(Name = "Onboarding")]
    Onboarding = 1,

    [Display(Name = "Auth & Security")]
    AuthAndSecurity = 2,

    [Display(Name = "Billing & Invoicing")]
    BillingAndInvoicing = 3,

    [Display(Name = "System Notices")]
    SystemNotices = 4
}

public static class EmailTemplateCategoryNames
{
    public const string Onboarding = "Onboarding";
    public const string AuthAndSecurity = "Auth & Security";
    public const string BillingAndInvoicing = "Billing & Invoicing";
    public const string SystemNotices = "System Notices";
}

public static class EmailTemplateCategoryExtensions
{
    public static string GetDisplayName(this EmailTemplateCategory category) => category switch
    {
        EmailTemplateCategory.Onboarding => "Onboarding",
        EmailTemplateCategory.AuthAndSecurity => "Auth & Security",
        EmailTemplateCategory.BillingAndInvoicing => "Billing & Invoicing",
        EmailTemplateCategory.SystemNotices => "System Notices",
        _ => category.ToString()
    };
}
