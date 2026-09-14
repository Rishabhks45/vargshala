namespace Vargshala.Contracts.Fees;

public class StudentFeeDto
{
    public Guid Id { get; set; }
    public Guid OrganizationId { get; set; }
    public Guid StudentId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public string? StudentCode { get; set; }
    public string? RollNumber { get; set; }
    public Guid BranchId { get; set; }
    public string BranchName { get; set; } = string.Empty;
    public Guid? ClassId { get; set; }
    public string? ClassName { get; set; }
    public string? BatchName { get; set; }
    public Guid FeeStructureId { get; set; }
    public string FeeStructureName { get; set; } = string.Empty;

    public decimal OriginalAmount { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal FinalAmount { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal DueAmount => Math.Max(0, FinalAmount - PaidAmount);
    public string Status { get; set; } = "Pending";
    public DateTime AssignedAt { get; set; }

    public DateOnly? NextDueDate { get; set; }
    public int InstallmentsCount { get; set; }
    public int PaidInstallmentsCount { get; set; }

    public string Initials
    {
        get
        {
            if (string.IsNullOrWhiteSpace(StudentName)) return "ST";
            var parts = StudentName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            return parts.Length >= 2
                ? $"{parts[0][0]}{parts[1][0]}".ToUpper()
                : parts[0].Length >= 2 ? parts[0][..2].ToUpper() : parts[0].ToUpper();
        }
    }
}

public class FeeInstallmentDto
{
    public Guid Id { get; set; }
    public int InstallmentNumber { get; set; }
    public decimal Amount { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal DueAmount => Math.Max(0, Amount - PaidAmount);
    public DateOnly DueDate { get; set; }
    public string Status { get; set; } = "Pending";
}

public class FeeDiscountDto
{
    public Guid Id { get; set; }
    public string DiscountType { get; set; } = string.Empty;
    public decimal Value { get; set; }
    public decimal Amount { get; set; }
    public string? Reason { get; set; }
    public string? ApprovedByName { get; set; }
    public DateTime ApprovedAt { get; set; }
}

public class PaymentAllocationDto
{
    public Guid FeeInstallmentId { get; set; }
    public int InstallmentNumber { get; set; }
    public decimal AllocatedAmount { get; set; }
    public DateOnly DueDate { get; set; }
}

public class PaymentDto
{
    public Guid Id { get; set; }
    public Guid OrganizationId { get; set; }
    public Guid? BranchId { get; set; }
    public string? BranchName { get; set; }
    public Guid StudentId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public string? StudentRollNumber { get; set; }
    public string? StudentCode { get; set; }
    public string? ReceiptNumber { get; set; }
    public decimal Amount { get; set; }
    public DateTime PaymentDate { get; set; }
    public string PaymentMethod { get; set; } = "Cash";
    public string? TransactionReference { get; set; }
    public string Status { get; set; } = "Completed";
    public string? Remarks { get; set; }
    public List<PaymentAllocationDto> Allocations { get; set; } = new();
}

public class StudentFeeDetailDto
{
    public StudentFeeDto StudentFee { get; set; } = null!;
    public List<FeeInstallmentDto> Installments { get; set; } = new();
    public List<FeeDiscountDto> Discounts { get; set; } = new();
    public List<PaymentDto> RecentPayments { get; set; } = new();
}

public class FeeStatisticsDto
{
    public decimal TotalExpected { get; set; }
    public decimal TotalCollected { get; set; }
    public decimal TotalPending { get; set; }
    public int OverdueStudentsCount { get; set; }
    public int CollectionRate => TotalExpected > 0 ? (int)Math.Round(TotalCollected / TotalExpected * 100) : 0;
}

public class StudentLookupForFeeDto
{
    public Guid StudentId { get; set; }
    public Guid UserId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string? RollNumber { get; set; }
    public string? StudentCode { get; set; }
    public Guid BranchId { get; set; }
    public string BranchName { get; set; } = string.Empty;
    public Guid? ClassId { get; set; }
    public string? ClassName { get; set; }
    public bool HasFeeAssigned { get; set; }
    public decimal PendingDue { get; set; }
}
