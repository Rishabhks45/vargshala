namespace RazorpayUtility.Data;

/// <summary>
/// Database entity for storing Razorpay integration settings in PostgreSQL.
/// </summary>
public class RazorpaySettingsEntity
{
    public Guid Id { get; set; }
    public string KeyId { get; set; } = string.Empty;
    public string KeySecret { get; set; } = string.Empty;
    public string WebhookSecret { get; set; } = string.Empty;
    public string Currency { get; set; } = "INR";
    public string CompanyName { get; set; } = "Vargshala";
    public string ThemeColor { get; set; } = "#009488";
    public string SuccessUrl { get; set; } = string.Empty;
    public string CancelUrl { get; set; } = string.Empty;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
