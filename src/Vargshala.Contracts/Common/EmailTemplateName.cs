using System.ComponentModel.DataAnnotations;

namespace Vargshala.Contracts.Common;

public enum EmailTemplateName
{
    [Display(Name = "Welcome & Onboarding")]
    WelcomeOnboarding = 1,

    [Display(Name = "Forgot Password")]
    ForgotPassword = 2,

    [Display(Name = "Password Reset Link")]
    PasswordReset = 3,

    [Display(Name = "Verification OTP")]
    VerificationOtp = 4,

    [Display(Name = "Admission Confirmation")]
    AdmissionConfirmation = 5,

    [Display(Name = "Fee Payment Receipt")]
    FeeReceipt = 6,

    [Display(Name = "Fee Due Reminder")]
    FeeDueReminder = 7,

    [Display(Name = "Attendance Alert")]
    AttendanceAlert = 8,

    [Display(Name = "Exam & Quiz Notice")]
    ExamNotice = 9,

    [Display(Name = "General Announcement")]
    GeneralAnnouncement = 10
}

public static class EmailTemplateNameConstants
{
    public const string WelcomeOnboarding = "Welcome & Onboarding";
    public const string ForgotPassword = "Forgot Password";
    public const string PasswordReset = "Password Reset Link";
    public const string VerificationOtp = "Verification OTP";
    public const string AdmissionConfirmation = "Admission Confirmation";
    public const string FeeReceipt = "Fee Payment Receipt";
    public const string FeeDueReminder = "Fee Due Reminder";
    public const string AttendanceAlert = "Attendance Alert";
    public const string ExamNotice = "Exam & Quiz Notice";
    public const string GeneralAnnouncement = "General Announcement";
}

public static class EmailTemplateNameExtensions
{
    public static string GetDisplayName(this EmailTemplateName templateName) => templateName switch
    {
        EmailTemplateName.WelcomeOnboarding => "Welcome & Onboarding",
        EmailTemplateName.ForgotPassword => "Forgot Password",
        EmailTemplateName.PasswordReset => "Password Reset Link",
        EmailTemplateName.VerificationOtp => "Verification OTP",
        EmailTemplateName.AdmissionConfirmation => "Admission Confirmation",
        EmailTemplateName.FeeReceipt => "Fee Payment Receipt",
        EmailTemplateName.FeeDueReminder => "Fee Due Reminder",
        EmailTemplateName.AttendanceAlert => "Attendance Alert",
        EmailTemplateName.ExamNotice => "Exam & Quiz Notice",
        EmailTemplateName.GeneralAnnouncement => "General Announcement",
        _ => templateName.ToString()
    };

    public static string GetDefaultCode(this EmailTemplateName templateName) => templateName switch
    {
        EmailTemplateName.WelcomeOnboarding => "WELCOME_ONBOARD",
        EmailTemplateName.ForgotPassword => "FORGOT_PASSWORD",
        EmailTemplateName.PasswordReset => "PASSWORD_RESET",
        EmailTemplateName.VerificationOtp => "VERIFICATION_OTP",
        EmailTemplateName.AdmissionConfirmation => "ADMISSION_CONFIRMATION",
        EmailTemplateName.FeeReceipt => "FEE_RECEIPT",
        EmailTemplateName.FeeDueReminder => "FEE_DUE_REMINDER",
        EmailTemplateName.AttendanceAlert => "ATTENDANCE_ALERT",
        EmailTemplateName.ExamNotice => "EXAM_NOTICE",
        EmailTemplateName.GeneralAnnouncement => "GENERAL_ANNOUNCEMENT",
        _ => templateName.ToString().ToUpperInvariant()
    };

    /// <summary>
    /// Helper method that trims the display name, replaces inner spaces with an underscore (_),
    /// removes special characters like '&', and converts to uppercase for System Code generation.
    /// E.g. "Welcome & Onboarding" -> "WELCOME_ONBOARDING", "Fee Payment Receipt" -> "FEE_PAYMENT_RECEIPT"
    /// </summary>
    public static string ToSystemCode(string displayName)
    {
        if (string.IsNullOrWhiteSpace(displayName))
            return string.Empty;

        var cleaned = displayName.Trim().Replace("&", "").Replace("-", " ");
        var withUnderscores = System.Text.RegularExpressions.Regex.Replace(cleaned, @"\s+", "_");
        withUnderscores = System.Text.RegularExpressions.Regex.Replace(withUnderscores, @"_+", "_");
        return withUnderscores.Trim('_').ToUpperInvariant();
    }

    /// <summary>
    /// Extension method on EmailTemplateName enum to generate formatted System Code.
    /// </summary>
    public static string ToSystemCode(this EmailTemplateName templateName)
    {
        return ToSystemCode(templateName.GetDisplayName());
    }

    /// <summary>
    /// Returns the logical category for an EmailTemplateName.
    /// </summary>
    public static string GetCategory(this EmailTemplateName templateName) => templateName switch
    {
        EmailTemplateName.WelcomeOnboarding or EmailTemplateName.AdmissionConfirmation => "Onboarding",
        EmailTemplateName.ForgotPassword or EmailTemplateName.PasswordReset or EmailTemplateName.VerificationOtp => "Auth & Security",
        EmailTemplateName.FeeReceipt or EmailTemplateName.FeeDueReminder => "Billing",
        _ => "System"
    };

    /// <summary>
    /// Finds matching EmailTemplateName enum from a system code, display name, or enum name.
    /// </summary>
    public static EmailTemplateName? FromSystemCodeOrDisplayName(string? codeOrName)
    {
        if (string.IsNullOrWhiteSpace(codeOrName)) return null;

        var normalized = ToSystemCode(codeOrName);
        foreach (EmailTemplateName item in Enum.GetValues(typeof(EmailTemplateName)))
        {
            if (item.ToString().Equals(codeOrName, StringComparison.OrdinalIgnoreCase) ||
                item.GetDisplayName().Equals(codeOrName, StringComparison.OrdinalIgnoreCase) ||
                item.ToSystemCode().Equals(normalized, StringComparison.OrdinalIgnoreCase) ||
                item.GetDefaultCode().Equals(codeOrName, StringComparison.OrdinalIgnoreCase))
            {
                return item;
            }
        }
        return null;
    }
}
