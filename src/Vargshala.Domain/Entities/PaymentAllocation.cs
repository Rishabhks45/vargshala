using Vargshala.Domain.Common;

namespace Vargshala.Domain.Entities;

public class PaymentAllocation : BaseEntity
{
    public Guid PaymentId { get; set; }
    public Guid FeeInstallmentId { get; set; }

    public decimal AllocatedAmount { get; set; }

    // Navigation
    public Payment Payment { get; set; } = null!;
    public FeeInstallment FeeInstallment { get; set; } = null!;
}
