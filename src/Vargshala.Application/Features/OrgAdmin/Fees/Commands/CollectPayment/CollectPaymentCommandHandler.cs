using MediatR;
using Vargshala.Application.Abstractions.CurrentUser;
using Vargshala.Application.Features.OrgAdmin.Fees.Infrastructure;
using Vargshala.Contracts.Common;
using Vargshala.Contracts.Fees;
using Vargshala.Domain.Entities;

namespace Vargshala.Application.Features.OrgAdmin.Fees.Commands.CollectPayment;

public class CollectPaymentCommandHandler : IRequestHandler<CollectPaymentCommand, ApiResponse<PaymentDto>>
{
    private readonly IFeeRepository _feeRepository;
    private readonly ICurrentUser _currentUser;

    public CollectPaymentCommandHandler(
        IFeeRepository feeRepository,
        ICurrentUser currentUser)
    {
        _feeRepository = feeRepository;
        _currentUser = currentUser;
    }

    public async Task<ApiResponse<PaymentDto>> Handle(CollectPaymentCommand command, CancellationToken cancellationToken)
    {
        var orgId = _currentUser.OrganizationId;
        if (!orgId.HasValue || orgId.Value == Guid.Empty)
        {
            return ApiResponse<PaymentDto>.FailureResponse("No active organization context found.");
        }

        var req = command.Request;

        // 1. Locate student fee
        StudentFee? studentFee;
        if (req.StudentFeeId.HasValue && req.StudentFeeId.Value != Guid.Empty)
        {
            studentFee = await _feeRepository.GetStudentFeeByIdAsync(req.StudentFeeId.Value, orgId.Value, cancellationToken);
        }
        else
        {
            studentFee = await _feeRepository.GetActiveStudentFeeByStudentIdAsync(req.StudentId, orgId.Value, cancellationToken);
        }

        if (studentFee == null)
        {
            return ApiResponse<PaymentDto>.FailureResponse("No active fee account found for this student.");
        }

        // 2. Validate outstanding balance
        var remainingDue = studentFee.FinalAmount - studentFee.PaidAmount;
        if (remainingDue <= 0)
        {
            return ApiResponse<PaymentDto>.FailureResponse("This student has already cleared all fee dues.");
        }

        if (req.Amount <= 0)
        {
            return ApiResponse<PaymentDto>.FailureResponse("Payment amount must be greater than zero.");
        }

        if (req.Amount > remainingDue)
        {
            return ApiResponse<PaymentDto>.FailureResponse($"Payment amount (₹{req.Amount:N2}) exceeds outstanding due balance of ₹{remainingDue:N2}.");
        }

        // 3. Generate Receipt Number
        var receiptNumber = await _feeRepository.GenerateReceiptNumberAsync(orgId.Value, cancellationToken);

        // 4. Create Payment
        var paymentId = Guid.NewGuid();
        var payment = new Payment
        {
            Id = paymentId,
            OrganizationId = orgId.Value,
            BranchId = studentFee.FeeStructure?.BranchId,
            StudentId = req.StudentId,
            ReceiptNumber = receiptNumber,
            Amount = req.Amount,
            PaymentDate = req.PaymentDate ?? DateTime.UtcNow,
            PaymentMethod = req.PaymentMethod,
            TransactionReference = string.IsNullOrWhiteSpace(req.TransactionReference) ? null : req.TransactionReference.Trim(),
            Remarks = string.IsNullOrWhiteSpace(req.Remarks) ? null : req.Remarks.Trim(),
            Status = "Completed",
            CreatedBy = _currentUser.UserId,
            CreatedAt = DateTime.UtcNow
        };

        // 5. FIFO Installment Allocation
        var remainingPayment = req.Amount;
        var pendingInstallments = studentFee.Installments
            .Where(i => i.PaidAmount < i.Amount)
            .OrderBy(i => i.DueDate)
            .ThenBy(i => i.InstallmentNumber)
            .ToList();

        foreach (var inst in pendingInstallments)
        {
            if (remainingPayment <= 0) break;

            var instDue = inst.Amount - inst.PaidAmount;
            var alloc = Math.Min(remainingPayment, instDue);

            inst.PaidAmount += alloc;
            inst.Status = inst.PaidAmount >= inst.Amount ? "Paid" : "PartiallyPaid";
            inst.UpdatedAt = DateTime.UtcNow;
            inst.UpdatedBy = _currentUser.UserId;

            payment.Allocations.Add(new PaymentAllocation
            {
                Id = Guid.NewGuid(),
                PaymentId = paymentId,
                FeeInstallmentId = inst.Id,
                AllocatedAmount = alloc,
                CreatedBy = _currentUser.UserId,
                CreatedAt = DateTime.UtcNow
            });

            _feeRepository.UpdateInstallment(inst);
            remainingPayment -= alloc;
        }

        // 6. Update StudentFee Totals & Status
        studentFee.PaidAmount += req.Amount;
        studentFee.Status = studentFee.PaidAmount >= studentFee.FinalAmount ? "Paid" : "PartiallyPaid";
        studentFee.UpdatedAt = DateTime.UtcNow;
        studentFee.UpdatedBy = _currentUser.UserId;

        _feeRepository.UpdateStudentFee(studentFee);
        await _feeRepository.AddPaymentAsync(payment, cancellationToken);
        await _feeRepository.SaveChangesAsync(cancellationToken);

        // 7. Return detailed receipt DTO
        var receiptResult = await _feeRepository.GetPaymentReceiptByIdAsync(paymentId, orgId.Value, cancellationToken);
        return ApiResponse<PaymentDto>.SuccessResponse(
            (receiptResult ?? payment).ToDto(),
            $"Payment of ₹{req.Amount:N2} recorded successfully with Receipt #{receiptNumber}.");
    }
}
