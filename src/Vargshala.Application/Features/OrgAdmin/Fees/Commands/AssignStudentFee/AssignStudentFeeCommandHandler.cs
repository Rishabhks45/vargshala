using MediatR;
using Vargshala.Application.Abstractions.CurrentUser;
using Vargshala.Application.Features.OrgAdmin.FeeStructures.Infrastructure;
using Vargshala.Application.Features.OrgAdmin.Fees.Infrastructure;
using Vargshala.Application.Features.OrgAdmin.Students.Infrastructure;
using Vargshala.Contracts.Common;
using Vargshala.Contracts.Fees;
using Vargshala.Domain.Entities;

namespace Vargshala.Application.Features.OrgAdmin.Fees.Commands.AssignStudentFee;

public class AssignStudentFeeCommandHandler : IRequestHandler<AssignStudentFeeCommand, ApiResponse<StudentFeeDto>>
{
    private readonly IFeeRepository _feeRepository;
    private readonly IFeeStructureRepository _feeStructureRepository;
    private readonly IStudentRepository _studentRepository;
    private readonly ICurrentUser _currentUser;

    public AssignStudentFeeCommandHandler(
        IFeeRepository feeRepository,
        IFeeStructureRepository feeStructureRepository,
        IStudentRepository studentRepository,
        ICurrentUser currentUser)
    {
        _feeRepository = feeRepository;
        _feeStructureRepository = feeStructureRepository;
        _studentRepository = studentRepository;
        _currentUser = currentUser;
    }

    public async Task<ApiResponse<StudentFeeDto>> Handle(AssignStudentFeeCommand command, CancellationToken cancellationToken)
    {
        var orgId = _currentUser.OrganizationId;
        if (!orgId.HasValue || orgId.Value == Guid.Empty)
        {
            return ApiResponse<StudentFeeDto>.FailureResponse("No active organization context found.");
        }

        var req = command.Request;

        // 1. Verify student exists in current organization
        var student = await _studentRepository.GetByIdAsync(req.StudentId, cancellationToken);
        if (student == null || student.User?.OrganizationId != orgId.Value)
        {
            return ApiResponse<StudentFeeDto>.FailureResponse("Student not found in your organization.");
        }

        // 2. Verify Fee Structure exists in current organization
        var feeStructure = await _feeStructureRepository.GetByIdAsync(req.FeeStructureId, cancellationToken);
        if (feeStructure == null || feeStructure.OrganizationId != orgId.Value)
        {
            return ApiResponse<StudentFeeDto>.FailureResponse("Fee structure not found in your organization.");
        }

        // 3. Check if student already has this fee structure assigned and not cancelled
        var existingActiveFee = await _feeRepository.GetActiveStudentFeeByStudentIdAsync(req.StudentId, cancellationToken);
        if (existingActiveFee != null && existingActiveFee.FeeStructureId == req.FeeStructureId && existingActiveFee.Status != "Cancelled")
        {
            return ApiResponse<StudentFeeDto>.FailureResponse("This fee structure has already been assigned to this student.");
        }

        // 4. Calculate Discount
        decimal discountAmount = 0;
        if (!string.IsNullOrWhiteSpace(req.DiscountType) && req.DiscountValue.HasValue && req.DiscountValue.Value > 0)
        {
            if (req.DiscountType.Equals("Percentage", StringComparison.OrdinalIgnoreCase))
            {
                discountAmount = Math.Round(feeStructure.TotalAmount * (req.DiscountValue.Value / 100m), 2);
            }
            else
            {
                discountAmount = Math.Min(feeStructure.TotalAmount, req.DiscountValue.Value);
            }
        }

        var finalAmount = Math.Max(0, feeStructure.TotalAmount - discountAmount);

        // 5. Build StudentFee
        var studentFeeId = Guid.NewGuid();
        var studentFee = new StudentFee
        {
            Id = studentFeeId,
            OrganizationId = orgId.Value,
            StudentId = req.StudentId,
            FeeStructureId = req.FeeStructureId,
            OriginalAmount = feeStructure.TotalAmount,
            DiscountAmount = discountAmount,
            FinalAmount = finalAmount,
            PaidAmount = 0,
            Status = finalAmount == 0 ? "Paid" : "Pending",
            AssignedAt = DateTime.UtcNow,
            CreatedBy = _currentUser.UserId,
            CreatedAt = DateTime.UtcNow
        };

        // 6. Add Discount if any
        if (discountAmount > 0)
        {
            studentFee.Discounts.Add(new FeeDiscount
            {
                Id = Guid.NewGuid(),
                OrganizationId = orgId.Value,
                StudentFeeId = studentFeeId,
                DiscountType = req.DiscountType ?? "FixedAmount",
                Value = req.DiscountValue ?? discountAmount,
                Amount = discountAmount,
                Reason = req.DiscountReason?.Trim(),
                ApprovedBy = _currentUser.UserId,
                ApprovedAt = DateTime.UtcNow,
                CreatedBy = _currentUser.UserId,
                CreatedAt = DateTime.UtcNow
            });
        }

        // 7. Generate Installments
        var count = Math.Max(1, Math.Min(12, req.InstallmentsCount));
        var baseAmount = Math.Floor(finalAmount / count * 100m) / 100m;
        var lastAmount = finalAmount - (baseAmount * (count - 1));

        for (int i = 1; i <= count; i++)
        {
            var instAmount = (i == count) ? lastAmount : baseAmount;
            var dueDate = req.FirstDueDate.AddMonths(i - 1);

            studentFee.Installments.Add(new FeeInstallment
            {
                Id = Guid.NewGuid(),
                OrganizationId = orgId.Value,
                StudentFeeId = studentFeeId,
                InstallmentNumber = i,
                Amount = instAmount,
                PaidAmount = 0,
                DueDate = dueDate,
                Status = finalAmount == 0 ? "Paid" : "Pending",
                CreatedBy = _currentUser.UserId,
                CreatedAt = DateTime.UtcNow
            });
        }

        await _feeRepository.AddStudentFeeAsync(studentFee, cancellationToken);
        await _feeRepository.SaveChangesAsync(cancellationToken);

        // Fetch loaded detail for DTO mapping
        var resultEntity = await _feeRepository.GetStudentFeeByIdAsync(studentFeeId, cancellationToken);
        return ApiResponse<StudentFeeDto>.SuccessResponse(
            (resultEntity ?? studentFee).ToDto(),
            "Fee structure successfully assigned to student.");
    }
}
