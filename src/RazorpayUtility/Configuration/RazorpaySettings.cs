namespace RazorpayUtility.Configuration;

/// <summary>
/// Configuration settings for Razorpay Payment Gateway integration.
/// 
/// Contains API credentials, webhook secrets, and branding customization
/// for standard checkout modal and dynamic UPI QR code generation.
/// </summary>
public class RazorpaySettings
{
    /// <summary>
    /// Razorpay Key ID (e.g., rzp_test_... or rzp_live_...)
    /// </summary>
    public string KeyId { get; set; } = string.Empty;

    /// <summary>
    /// Razorpay Key Secret for authenticated server-side API requests and signature validation.
    /// </summary>
    public string KeySecret { get; set; } = string.Empty;

    /// <summary>
    /// Webhook Secret configured in the Razorpay Dashboard to verify incoming webhook payloads.
    /// </summary>
    public string WebhookSecret { get; set; } = string.Empty;

    /// <summary>
    /// Currency code (Default: "INR").
    /// </summary>
    public string Currency { get; set; } = "INR";

    /// <summary>
    /// Brand/Company name displayed in Razorpay Standard Checkout modal.
    /// </summary>
    public string CompanyName { get; set; } = "Vargshala";

    /// <summary>
    /// Brand theme accent color (Deep Teal: #009488).
    /// </summary>
    public string ThemeColor { get; set; } = "#009488";

    /// <summary>
    /// Redirect URL on payment completion.
    /// </summary>
    public string SuccessUrl { get; set; } = string.Empty;

    /// <summary>
    /// Redirect URL on payment cancellation/failure.
    /// </summary>
    public string CancelUrl { get; set; } = string.Empty;

    /// <summary>
    /// Validates whether the required credentials are set.
    /// </summary>
    public bool IsValid() =>
        !string.IsNullOrWhiteSpace(KeyId) &&
        !string.IsNullOrWhiteSpace(KeySecret);
}
