using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vargshala.Application.Abstractions.Security;
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
        IBranchAuthorizationService branchAuthService)
        : base(mediator, branchAuthService)
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

        if (!await BranchAuthService.CanAccessStudentFeeAsync(id, branchId, cancellationToken))
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

        if (!await BranchAuthService.CanAccessFeeStructureAsync(request.FeeStructureId, branchId, cancellationToken))
        {
            return BadRequest(ApiResponse<StudentFeeDto>.FailureResponse("Selected fee package does not belong to your branch."));
        }

        if (!await BranchAuthService.CanAccessStudentAsync(request.StudentId, branchId, cancellationToken))
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

        bool recordBelongsToBranch;
        if (request.StudentFeeId.HasValue && request.StudentFeeId.Value != Guid.Empty)
        {
            recordBelongsToBranch = await BranchAuthService.CanAccessStudentFeeAsync(request.StudentFeeId.Value, branchId, cancellationToken);
        }
        else
        {
            recordBelongsToBranch = await BranchAuthService.CanAccessStudentAsync(request.StudentId, branchId, cancellationToken);
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
        var (isValid, _, errorResult) = await ValidateBranchAccessAsync(cancellationToken);
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
        var (isValid, _, errorResult) = await ValidateBranchAccessAsync(cancellationToken);
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
        var (isValid, _, errorResult) = await ValidateBranchAccessAsync(cancellationToken);
        if (!isValid) return errorResult!;

        var result = await Mediator.Send(new Vargshala.Application.Features.OrgAdmin.Fees.Queries.GetFeeReceiptPdf.GetStudentFeeReceiptPdfQuery(feeId), cancellationToken);
        if (!result.Success || result.Data == null)
        {
            return NotFound(result);
        }

        return File(result.Data.FileBytes, result.Data.ContentType, result.Data.FileName);
    }
}
