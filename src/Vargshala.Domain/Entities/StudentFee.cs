using Vargshala.Domain.Common;

namespace Vargshala.Domain.Entities;

public class StudentFee : BaseEntity
{
    public Guid OrganizationId { get; set; }
    public Guid StudentId { get; set; }
    public Guid FeeStructureId { get; set; }

    public decimal OriginalAmount { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal FinalAmount { get; set; }
    public decimal PaidAmount { get; set; }
    public string Status { get; set; } = "Pending";
    public DateTime AssignedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public Organization Organization { get; set; } = null!;
    public Student Student { get; set; } = null!;
    public FeeStructure FeeStructure { get; set; } = null!;
    public ICollection<FeeDiscount> Discounts { get; set; } = new List<FeeDiscount>();
    public ICollection<FeeInstallment> Installments { get; set; } = new List<FeeInstallment>();
}
