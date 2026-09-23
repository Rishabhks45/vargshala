namespace Vargshala.Contracts.Payments;

public class PaymentLogDto
{
    public Guid PaymentId { get; set; }
    public string ReceiptNumber { get; set; } = string.Empty;
    public string? OrderId { get; set; }
    public string? TransactionReference { get; set; }
    public Guid OrganizationId { get; set; }
    public string InstituteName { get; set; } = string.Empty;
    public string? PayerEmail { get; set; }
    public decimal Amount { get; set; }
    public string PaymentMethod { get; set; } = "Razorpay";
    public DateTime PaymentDate { get; set; }
    public string Status { get; set; } = "Completed";
    public string PaymentType { get; set; } = "Subscription";
    public string? PlanName { get; set; }
    public string? FailureReason { get; set; }
    public string? Remarks { get; set; }
}

public class PaymentLogsStatsDto
{
    public decimal TotalVolume30Days { get; set; }
    public double SuccessRate { get; set; } = 100.0;
    public int FailedCount { get; set; }
    public int TotalTransactionsCount { get; set; }
    public string SettlementCycle { get; set; } = "T + 1 Days";
}
