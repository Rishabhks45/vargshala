namespace Vargshala.Domain.Entities;

public class FeeInstallment
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid OrganizationId { get; set; }
    public Guid StudentFeeId { get; set; }

    public int InstallmentNumber { get; set; }
    public decimal Amount { get; set; }
    public decimal PaidAmount { get; set; }
    public DateOnly DueDate { get; set; }
    public string Status { get; set; } = "Pending";

    public Guid? CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public Guid? UpdatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }

    // Navigation
    public Organization Organization { get; set; } = null!;
    public StudentFee StudentFee { get; set; } = null!;
    public ICollection<PaymentAllocation> PaymentAllocations { get; set; } = new List<PaymentAllocation>();
}
