using Vargshala.Contracts.Fees;
using Vargshala.Domain.Entities;

namespace Vargshala.Application.Features.OrgAdmin.Fees;

public static class FeeMappingExtensions
{
    public static StudentFeeDto ToDto(this StudentFee entity)
    {
        var studentName = entity.Student?.User != null
            ? $"{entity.Student.User.FirstName} {entity.Student.User.LastName}".Trim()
            : "Unknown Student";

        var branchName = entity.FeeStructure?.Branch?.Name ?? string.Empty;
        var className = entity.FeeStructure?.Class?.Name ?? entity.Student?.ClassName;

        // Try getting batch name from student's enrolled batches
        var batchName = entity.Student?.BatchStudents?.FirstOrDefault()?.Batch?.Name;

        var nextPendingInstallment = entity.Installments
            .Where(i => i.Status != "Paid" && i.Status != "Cancelled")
            .OrderBy(i => i.DueDate)
            .FirstOrDefault();

        var paidInstallmentsCount = entity.Installments.Count(i => i.Status == "Paid");

        return new StudentFeeDto
        {
            Id = entity.Id,
            OrganizationId = entity.OrganizationId,
            StudentId = entity.StudentId,
            StudentName = studentName,
            StudentCode = entity.Student?.StudentCode,
            RollNumber = entity.Student?.RollNumber,
            BranchId = entity.FeeStructure?.BranchId ?? Guid.Empty,
            BranchName = branchName,
            ClassId = entity.FeeStructure?.ClassId,
            ClassName = className,
            BatchName = batchName,
            FeeStructureId = entity.FeeStructureId,
            FeeStructureName = entity.FeeStructure?.Name ?? string.Empty,
            OriginalAmount = entity.OriginalAmount,
            DiscountAmount = entity.DiscountAmount,
            FinalAmount = entity.FinalAmount,
            PaidAmount = entity.PaidAmount,
            Status = entity.Status,
            AssignedAt = entity.AssignedAt,
            NextDueDate = nextPendingInstallment?.DueDate,
            InstallmentsCount = entity.Installments.Count,
            PaidInstallmentsCount = paidInstallmentsCount
        };
    }

    public static FeeInstallmentDto ToDto(this FeeInstallment entity)
    {
        return new FeeInstallmentDto
        {
            Id = entity.Id,
            InstallmentNumber = entity.InstallmentNumber,
            Amount = entity.Amount,
            PaidAmount = entity.PaidAmount,
            DueDate = entity.DueDate,
            Status = entity.Status
        };
    }

    public static FeeDiscountDto ToDto(this FeeDiscount entity)
    {
        var approver = entity.ApprovedByUser != null
            ? $"{entity.ApprovedByUser.FirstName} {entity.ApprovedByUser.LastName}".Trim()
            : null;

        return new FeeDiscountDto
        {
            Id = entity.Id,
            DiscountType = entity.DiscountType,
            Value = entity.Value,
            Amount = entity.Amount,
            Reason = entity.Reason,
            ApprovedByName = approver,
            ApprovedAt = entity.ApprovedAt
        };
    }

    public static PaymentDto ToDto(this Payment entity)
    {
        var studentName = entity.Student?.User != null
            ? $"{entity.Student.User.FirstName} {entity.Student.User.LastName}".Trim()
            : "Unknown Student";

        return new PaymentDto
        {
            Id = entity.Id,
            OrganizationId = entity.OrganizationId,
            BranchId = entity.BranchId,
            BranchName = entity.Branch?.Name,
            StudentId = entity.StudentId,
            StudentName = studentName,
            StudentRollNumber = entity.Student?.RollNumber,
            StudentCode = entity.Student?.StudentCode,
            ReceiptNumber = entity.ReceiptNumber,
            Amount = entity.Amount,
            PaymentDate = entity.PaymentDate,
            PaymentMethod = entity.PaymentMethod,
            TransactionReference = entity.TransactionReference,
            Status = entity.Status,
            Remarks = entity.Remarks,
            Allocations = entity.Allocations.Select(a => new PaymentAllocationDto
            {
                FeeInstallmentId = a.FeeInstallmentId,
                InstallmentNumber = a.FeeInstallment?.InstallmentNumber ?? 0,
                AllocatedAmount = a.AllocatedAmount,
                DueDate = a.FeeInstallment?.DueDate ?? DateOnly.MinValue
            }).ToList()
        };
    }

    public static StudentFeeDetailDto ToDetailDto(this StudentFee entity, List<Payment>? payments = null)
    {
        return new StudentFeeDetailDto
        {
            StudentFee = entity.ToDto(),
            Installments = entity.Installments.OrderBy(i => i.InstallmentNumber).Select(i => i.ToDto()).ToList(),
            Discounts = entity.Discounts.OrderByDescending(d => d.ApprovedAt).Select(d => d.ToDto()).ToList(),
            RecentPayments = (payments ?? new List<Payment>()).Select(p => p.ToDto()).ToList()
        };
    }
}
