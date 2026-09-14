using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vargshala.Application.Abstractions.CurrentUser;
using Vargshala.Application.Abstractions.Persistence;
using Vargshala.Application.Features.OrgAdmin.Fees.Commands.AssignStudentFee;
using Vargshala.Application.Features.OrgAdmin.Fees.Commands.CollectPayment;
using Vargshala.Application.Features.OrgAdmin.Fees.Queries.GetFeeStatistics;
using Vargshala.Application.Features.OrgAdmin.Fees.Queries.GetPaymentReceipt;
using Vargshala.Application.Features.OrgAdmin.Fees.Queries.GetStudentFeeDetails;
using Vargshala.Application.Features.OrgAdmin.Fees.Queries.GetStudentFeesPaged;
using Vargshala.Application.Features.OrgAdmin.Fees.Queries.GetStudentsForFeeAssignment;
using Vargshala.Contracts.Common;
using Vargshala.Contracts.Fees;

namespace Vargshala.API.Controllers.BranchAdmin;

[Route("api/v1/branchadmin/fees")]
[Authorize(Roles = "BranchAdmin,4")]
public class FeesController : BaseBranchAdminController
{
    public FeesController(
        IMediator mediator,
        ICurrentUser currentUser,
        IVargshalaDbContext db)
        : base(mediator, currentUser, db)
    {
    }

    [HttpGet]
    public async Task<IActionResult> GetPaged(
        [FromQuery] PagedRequest request,
        [FromQuery] Guid? classId = null,
        [FromQuery] Guid? batchId = null,
        [FromQuery] string? status = null,
        CancellationToken cancellationToken = default)
    {
        var (isValid, branchId, errorResult) = await ValidateBranchAccessAsync(cancellationToken);
        if (!isValid) return errorResult!;

        var result = await Mediator.Send(new GetStudentFeesPagedQuery(request, branchId, classId, batchId, status), cancellationToken);
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    [HttpGet("stats")]
    public async Task<IActionResult> GetStatistics(CancellationToken cancellationToken = default)
    {
        var (isValid, branchId, errorResult) = await ValidateBranchAccessAsync(cancellationToken);
        if (!isValid) return errorResult!;

        var result = await Mediator.Send(new GetFeeStatisticsQuery(branchId), cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetDetails(Guid id, CancellationToken cancellationToken = default)
    {
        var (isValid, branchId, errorResult) = await ValidateBranchAccessAsync(cancellationToken);
        if (!isValid) return errorResult!;

        var existsInBranch = await Db.StudentFees.AsNoTracking()
            .AnyAsync(sf => sf.Id == id && !sf.IsDeleted &&
                (sf.FeeStructure != null && sf.FeeStructure.BranchId == branchId) &&
                (sf.Student.BatchStudents.Any(bs => bs.IsActive && bs.Batch.Class.BranchId == branchId) ||
                 sf.Student.User.UserBranchAccesses.Any(uba => uba.IsActive && uba.BranchId == branchId)), cancellationToken);

        if (!existsInBranch)
        {
            return NotFound(ApiResponse<StudentFeeDetailDto>.FailureResponse("Student fee record not found in this branch."));
        }

        var result = await Mediator.Send(new GetStudentFeeDetailsQuery(id), cancellationToken);
        if (!result.Success)
        {
            return NotFound(result);
        }

        return Ok(result);
    }

    [HttpGet("students-lookup")]
    public async Task<IActionResult> GetStudentsLookup(
        [FromQuery] Guid? classId = null,
        CancellationToken cancellationToken = default)
    {
        var (isValid, branchId, errorResult) = await ValidateBranchAccessAsync(cancellationToken);
        if (!isValid) return errorResult!;

        var result = await Mediator.Send(new GetStudentsForFeeAssignmentQuery(branchId, classId), cancellationToken);
        return Ok(result);
    }

    [HttpPost("assign")]
    public async Task<IActionResult> AssignFee([FromBody] AssignStudentFeeRequest request, CancellationToken cancellationToken = default)
    {
        var (isValid, branchId, errorResult) = await ValidateBranchAccessAsync(cancellationToken);
        if (!isValid) return errorResult!;

        // Validate that the fee structure belongs to this branch
        var feeStructureBelongsToBranch = await Db.FeeStructures.AsNoTracking()
            .AnyAsync(fs => fs.Id == request.FeeStructureId && !fs.IsDeleted && fs.BranchId == branchId, cancellationToken);
        if (!feeStructureBelongsToBranch)
        {
            return BadRequest(ApiResponse<StudentFeeDto>.FailureResponse("Selected fee package does not belong to your branch."));
        }

        // Validate that the student belongs to this branch
        var studentBelongsToBranch = await Db.Students.AsNoTracking()
            .AnyAsync(s => s.Id == request.StudentId && !s.IsDeleted &&
                (s.BatchStudents.Any(bs => bs.IsActive && !bs.Batch.IsDeleted && bs.Batch.Class.BranchId == branchId) ||
                 s.User.UserBranchAccesses.Any(uba => uba.IsActive && uba.BranchId == branchId)), cancellationToken);
        if (!studentBelongsToBranch)
        {
            return BadRequest(ApiResponse<StudentFeeDto>.FailureResponse("Selected student does not belong to your branch."));
        }

        var result = await Mediator.Send(new AssignStudentFeeCommand(request), cancellationToken);
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    [HttpPost("collect")]
    public async Task<IActionResult> CollectFee([FromBody] CollectPaymentRequest request, CancellationToken cancellationToken = default)
    {
        var (isValid, branchId, errorResult) = await ValidateBranchAccessAsync(cancellationToken);
        if (!isValid) return errorResult!;

        // Validate that the student fee or student belongs to this branch
        bool recordBelongsToBranch = false;
        if (request.StudentFeeId.HasValue && request.StudentFeeId.Value != Guid.Empty)
        {
            recordBelongsToBranch = await Db.StudentFees.AsNoTracking()
                .AnyAsync(sf => sf.Id == request.StudentFeeId.Value && !sf.IsDeleted &&
                    (sf.FeeStructure != null && sf.FeeStructure.BranchId == branchId) &&
                    (sf.Student.BatchStudents.Any(bs => bs.IsActive && bs.Batch.Class.BranchId == branchId) ||
                     sf.Student.User.UserBranchAccesses.Any(uba => uba.IsActive && uba.BranchId == branchId)), cancellationToken);
        }
        else
        {
            recordBelongsToBranch = await Db.Students.AsNoTracking()
                .AnyAsync(s => s.Id == request.StudentId && !s.IsDeleted &&
                    (s.BatchStudents.Any(bs => bs.IsActive && bs.Batch.Class.BranchId == branchId) ||
                     s.User.UserBranchAccesses.Any(uba => uba.IsActive && uba.BranchId == branchId)), cancellationToken);
        }

        if (!recordBelongsToBranch)
        {
            return BadRequest(ApiResponse<PaymentDto>.FailureResponse("This fee account does not belong to your branch."));
        }

        var result = await Mediator.Send(new CollectPaymentCommand(request), cancellationToken);
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    [HttpGet("payments/{paymentId:guid}/receipt")]
    public async Task<IActionResult> GetReceipt(Guid paymentId, CancellationToken cancellationToken = default)
    {
        var (isValid, branchId, errorResult) = await ValidateBranchAccessAsync(cancellationToken);
        if (!isValid) return errorResult!;

        var result = await Mediator.Send(new GetPaymentReceiptQuery(paymentId), cancellationToken);
        if (!result.Success)
        {
            return NotFound(result);
        }

        return Ok(result);
    }

    [HttpGet("payments/{paymentId:guid}/receipt/pdf")]
    public async Task<IActionResult> GetPaymentReceiptPdf(Guid paymentId, CancellationToken cancellationToken = default)
    {
        var (isValid, branchId, errorResult) = await ValidateBranchAccessAsync(cancellationToken);
        if (!isValid) return errorResult!;

        var result = await Mediator.Send(new Vargshala.Application.Features.OrgAdmin.Fees.Queries.GetFeeReceiptPdf.GetPaymentReceiptPdfQuery(paymentId), cancellationToken);
        if (!result.Success || result.Data == null)
        {
            return NotFound(result);
        }

        return File(result.Data.FileBytes, result.Data.ContentType, result.Data.FileName);
    }

    [HttpGet("student-fees/{feeId:guid}/receipt/pdf")]
    public async Task<IActionResult> GetStudentFeeReceiptPdf(Guid feeId, CancellationToken cancellationToken = default)
    {
        var (isValid, branchId, errorResult) = await ValidateBranchAccessAsync(cancellationToken);
        if (!isValid) return errorResult!;

        var result = await Mediator.Send(new Vargshala.Application.Features.OrgAdmin.Fees.Queries.GetFeeReceiptPdf.GetStudentFeeReceiptPdfQuery(feeId), cancellationToken);
        if (!result.Success || result.Data == null)
        {
            return NotFound(result);
        }

        return File(result.Data.FileBytes, result.Data.ContentType, result.Data.FileName);
    }
}
