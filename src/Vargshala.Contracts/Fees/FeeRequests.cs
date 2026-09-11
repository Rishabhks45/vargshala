using Vargshala.Contracts.Common;

namespace Vargshala.Contracts.Fees;

public class AssignStudentFeeRequest
{
    public Guid StudentId { get; set; }
    public Guid FeeStructureId { get; set; }

    public string? DiscountType { get; set; }
    public decimal? DiscountValue { get; set; }
    public string? DiscountReason { get; set; }

    public int InstallmentsCount { get; set; } = 1;
    public DateOnly FirstDueDate { get; set; } = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7));
}

public class CollectPaymentRequest
{
    public Guid StudentId { get; set; }
    public Guid? StudentFeeId { get; set; }
    public decimal Amount { get; set; }
    public string PaymentMethod { get; set; } = "UPI";
    public string? TransactionReference { get; set; }
    public string? Remarks { get; set; }
    public DateTime? PaymentDate { get; set; }
}

public class GetStudentFeesPagedRequest : PagedRequest
{
    public Guid? BranchId { get; set; }
    public Guid? ClassId { get; set; }
    public Guid? BatchId { get; set; }
    public string? Status { get; set; }
}
