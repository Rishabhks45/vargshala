using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vargshala.Application.Abstractions.Security;
using Vargshala.Application.Features.OrgAdmin.Attendances.Commands.SaveSessionAttendance;
using Vargshala.Application.Features.OrgAdmin.Attendances.Queries.GetBatchAttendanceOverview;
using Vargshala.Application.Features.OrgAdmin.Attendances.Queries.GetDateWiseReport;
using Vargshala.Application.Features.OrgAdmin.Attendances.Queries.GetSessionAttendanceSheet;
using Vargshala.Application.Features.OrgAdmin.Attendances.Queries.GetStudentWiseReport;
using Vargshala.Contracts.Attendances;
using Vargshala.Contracts.Common;

namespace Vargshala.API.Controllers.BranchAdmin;

[Route("api/v1/branchadmin/attendances")]
[Authorize(Roles = "BranchAdmin,4")]
public class AttendancesController : BaseBranchAdminController
{
    public AttendancesController(
        IMediator mediator,
        IBranchAuthorizationService branchAuthService)
        : base(mediator, branchAuthService)
    {
    }

    [HttpGet("session/{sessionId:guid}")]
    public async Task<IActionResult> GetSessionAttendanceSheet(Guid sessionId, CancellationToken cancellationToken)
    {
        var (isValid, branchId, errorResult) = await ValidateBranchAccessAsync(cancellationToken);
        if (!isValid) return errorResult!;

        if (!await BranchAuthService.CanAccessClassSessionAsync(sessionId, branchId, cancellationToken))
        {
            return NotFound(ApiResponse<SessionAttendanceSheetDto>.FailureResponse("Session not found in this branch."));
        }

        var result = await Mediator.Send(new GetSessionAttendanceSheetQuery(sessionId), cancellationToken);
        if (!result.Success)
        {
            return NotFound(result);
        }

        return Ok(result);
    }

    [HttpPost("session/{sessionId:guid}")]
    public async Task<IActionResult> SaveSessionAttendance(Guid sessionId, [FromBody] MarkSessionAttendanceRequest request, CancellationToken cancellationToken)
    {
        var (isValid, branchId, errorResult) = await ValidateBranchAccessAsync(cancellationToken);
        if (!isValid) return errorResult!;

        if (request == null)
        {
            return BadRequest(ApiResponse<SessionAttendanceSheetDto>.FailureResponse("Invalid request body."));
        }

        if (!await BranchAuthService.CanAccessClassSessionAsync(sessionId, branchId, cancellationToken))
        {
            return NotFound(ApiResponse<SessionAttendanceSheetDto>.FailureResponse("Session not found in this branch."));
        }

        request.ClassSessionId = sessionId;

        var result = await Mediator.Send(new SaveSessionAttendanceCommand(request), cancellationToken);
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    [HttpGet("batch/{batchId:guid}/overview")]
    public async Task<IActionResult> GetBatchOverview(Guid batchId, [FromQuery] DateOnly? date, CancellationToken cancellationToken)
    {
        var (isValid, branchId, errorResult) = await ValidateBranchAccessAsync(cancellationToken);
        if (!isValid) return errorResult!;

        if (!await BranchAuthService.CanAccessBatchAsync(batchId, branchId, cancellationToken))
        {
            return NotFound(ApiResponse<BatchAttendanceOverviewDto>.FailureResponse("Batch not found in this branch."));
        }

        var result = await Mediator.Send(new GetBatchAttendanceOverviewQuery(batchId, date), cancellationToken);
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    [HttpGet("batch/{batchId:guid}/student-report")]
    public async Task<IActionResult> GetStudentWiseReport(Guid batchId, CancellationToken cancellationToken)
    {
        var (isValid, branchId, errorResult) = await ValidateBranchAccessAsync(cancellationToken);
        if (!isValid) return errorResult!;

        if (!await BranchAuthService.CanAccessBatchAsync(batchId, branchId, cancellationToken))
        {
            return NotFound(ApiResponse<string>.FailureResponse("Batch not found in this branch."));
        }

        var result = await Mediator.Send(new GetStudentWiseReportQuery(batchId), cancellationToken);
        return Ok(result);
    }

    [HttpGet("batch/{batchId:guid}/date-report")]
    public async Task<IActionResult> GetDateWiseReport(
        Guid batchId,
        [FromQuery] DateOnly? fromDate,
        [FromQuery] DateOnly? toDate,
        CancellationToken cancellationToken)
    {
        var (isValid, branchId, errorResult) = await ValidateBranchAccessAsync(cancellationToken);
        if (!isValid) return errorResult!;

        if (!await BranchAuthService.CanAccessBatchAsync(batchId, branchId, cancellationToken))
        {
            return NotFound(ApiResponse<string>.FailureResponse("Batch not found in this branch."));
        }

        var result = await Mediator.Send(new GetDateWiseReportQuery(batchId, fromDate, toDate), cancellationToken);
        return Ok(result);
    }
}
