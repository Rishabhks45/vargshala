namespace Vargshala.Contracts.Subscriptions;

public class SubscriptionPaymentReceiptDto
{
    public Guid PaymentId { get; set; }
    public string ReceiptNumber { get; set; } = string.Empty;
    public DateTime PaymentDate { get; set; }
    public decimal Amount { get; set; }
    public decimal OriginalAmount { get; set; }
    public decimal DiscountAmount { get; set; }
    public string Currency { get; set; } = "INR";
    public string PaymentMethod { get; set; } = "Razorpay";
    public string? TransactionReference { get; set; }
    public string? RazorpayOrderId { get; set; }
    public string Status { get; set; } = "Completed";
    public string? Remarks { get; set; }
    public string? CouponCode { get; set; }

    // Subscriber (Organization) Details
    public Guid OrganizationId { get; set; }
    public string OrganizationName { get; set; } = string.Empty;
    public string? OrganizationEmail { get; set; }
    public string? OrganizationPhone { get; set; }
    public string? OrganizationAddress { get; set; }

    // Plan & Capacity Quotas
    public Guid? PlanId { get; set; }
    public string PlanName { get; set; } = string.Empty;
    public string BillingCycle { get; set; } = "Monthly";
    public string StudentQuota { get; set; } = "Unlimited";
    public string TeacherQuota { get; set; } = "Unlimited";
    public string BranchQuota { get; set; } = "Unlimited";
    public DateTime? SubscriptionStartDate { get; set; }
    public DateTime? SubscriptionEndDate { get; set; }

    // Issuer (Vargshala Platform) Details
    public string IssuerName { get; set; } = "Vargshala EdTech SaaS";
    public string IssuerWebsite { get; set; } = "https://vargshala.com";
    public string IssuerSupportEmail { get; set; } = "billing@vargshala.com";
}

public class SubscriptionReceiptPdfResult
{
    public byte[] FileBytes { get; set; } = Array.Empty<byte>();
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = "application/pdf";
}
