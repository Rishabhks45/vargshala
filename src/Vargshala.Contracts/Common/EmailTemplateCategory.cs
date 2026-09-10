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
