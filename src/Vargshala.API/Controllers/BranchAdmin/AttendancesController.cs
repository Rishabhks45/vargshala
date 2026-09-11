using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vargshala.Application.Abstractions.CurrentUser;
using Vargshala.Application.Abstractions.Persistence;
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
        ICurrentUser currentUser,
        IVargshalaDbContext db)
        : base(mediator, currentUser, db)
    {
    }

    [HttpGet("session/{sessionId:guid}")]
    public async Task<IActionResult> GetSessionAttendanceSheet(Guid sessionId, CancellationToken cancellationToken)
    {
        var (isValid, branchId, errorResult) = await ValidateBranchAccessAsync(cancellationToken);
        if (!isValid) return errorResult!;

        var session = await Db.ClassSessions.AsNoTracking()
            .Include(s => s.Batch).ThenInclude(b => b.Class)
            .FirstOrDefaultAsync(s => s.Id == sessionId && !s.IsDeleted, cancellationToken);

        if (session == null || session.Batch?.Class?.BranchId != branchId)
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

        var session = await Db.ClassSessions.AsNoTracking()
            .Include(s => s.Batch).ThenInclude(b => b.Class)
            .FirstOrDefaultAsync(s => s.Id == sessionId && !s.IsDeleted, cancellationToken);

        if (session == null || session.Batch?.Class?.BranchId != branchId)
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

        var batch = await Db.Batches.AsNoTracking()
            .Include(b => b.Class)
            .FirstOrDefaultAsync(b => b.Id == batchId && !b.IsDeleted, cancellationToken);

        if (batch == null || batch.Class?.BranchId != branchId)
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

        var batch = await Db.Batches.AsNoTracking()
            .Include(b => b.Class)
            .FirstOrDefaultAsync(b => b.Id == batchId && !b.IsDeleted, cancellationToken);

        if (batch == null || batch.Class?.BranchId != branchId)
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

        var batch = await Db.Batches.AsNoTracking()
            .Include(b => b.Class)
            .FirstOrDefaultAsync(b => b.Id == batchId && !b.IsDeleted, cancellationToken);

        if (batch == null || batch.Class?.BranchId != branchId)
        {
            return NotFound(ApiResponse<string>.FailureResponse("Batch not found in this branch."));
        }

        var result = await Mediator.Send(new GetDateWiseReportQuery(batchId, fromDate, toDate), cancellationToken);
        return Ok(result);
    }
}
