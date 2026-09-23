namespace Vargshala.Contracts.Subscriptions;

public class SubscriptionBillingHistoryDto
{
    public Guid PaymentId { get; set; }
    public string ReceiptNumber { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTime PaymentDate { get; set; }
    public string PaymentMethod { get; set; } = "Razorpay";
    public string? TransactionReference { get; set; } // Razorpay Payment ID
    public string Status { get; set; } = "Completed";
    public string PlanName { get; set; } = string.Empty;
    public string? Remarks { get; set; }
}
