namespace Vargshala.Domain.Entities;

public class FeeDiscount
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid OrganizationId { get; set; }
    public Guid StudentFeeId { get; set; }

    public string DiscountType { get; set; } = "FixedAmount";
    public decimal Value { get; set; }
    public decimal Amount { get; set; }
    public string? Reason { get; set; }
    public Guid? ApprovedBy { get; set; }
    public DateTime ApprovedAt { get; set; } = DateTime.UtcNow;

    public Guid? CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public Organization Organization { get; set; } = null!;
    public StudentFee StudentFee { get; set; } = null!;
    public User? ApprovedByUser { get; set; }
}
