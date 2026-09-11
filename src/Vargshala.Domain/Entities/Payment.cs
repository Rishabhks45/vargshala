using Vargshala.Domain.Common;

namespace Vargshala.Domain.Entities;

public class Payment : BaseEntity
{
    public Guid OrganizationId { get; set; }
    public Guid? BranchId { get; set; }
    public Guid StudentId { get; set; }

    public string? ReceiptNumber { get; set; }
    public decimal Amount { get; set; }
    public DateTime PaymentDate { get; set; } = DateTime.UtcNow;
    public string PaymentMethod { get; set; } = "Cash";
    public string? TransactionReference { get; set; }
    public string Status { get; set; } = "Completed";
    public string? Remarks { get; set; }

    // Navigation
    public Organization Organization { get; set; } = null!;
    public Branch? Branch { get; set; }
    public Student Student { get; set; } = null!;
    public User? CreatedByUser { get; set; }
    public ICollection<PaymentAllocation> Allocations { get; set; } = new List<PaymentAllocation>();
}
