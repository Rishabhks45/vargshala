using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vargshala.Application.Abstractions.CurrentUser;
using Vargshala.Application.Abstractions.Persistence;
using Vargshala.Application.Features.OrgAdmin.ClassSessions.Commands.CancelClassSession;
using Vargshala.Application.Features.OrgAdmin.ClassSessions.Commands.CompleteClassSession;
using Vargshala.Application.Features.OrgAdmin.ClassSessions.Commands.CreateClassSession;
using Vargshala.Application.Features.OrgAdmin.ClassSessions.Commands.DeleteClassSession;
using Vargshala.Application.Features.OrgAdmin.ClassSessions.Commands.GenerateSessionsFromSchedule;
using Vargshala.Application.Features.OrgAdmin.ClassSessions.Commands.UpdateClassSession;
using Vargshala.Application.Features.OrgAdmin.ClassSessions.Queries.GetClassSessionById;
using Vargshala.Application.Features.OrgAdmin.ClassSessions.Queries.GetClassSessionsPaged;
using Vargshala.Contracts.ClassSessions;
using Vargshala.Contracts.Common;

namespace Vargshala.API.Controllers.BranchAdmin;

[Route("api/v1/branchadmin/class-sessions")]
[Authorize(Roles = "BranchAdmin,4")]
public class ClassSessionsController : BaseBranchAdminController
{
    public ClassSessionsController(
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
        [FromQuery] Guid? teacherId = null,
        [FromQuery] DateOnly? fromDate = null,
        [FromQuery] DateOnly? toDate = null,
        [FromQuery] string? status = null,
        CancellationToken cancellationToken = default)
    {
        var (isValid, branchId, errorResult) = await ValidateBranchAccessAsync(cancellationToken);
        if (!isValid) return errorResult!;

        var result = await Mediator.Send(new GetClassSessionsPagedQuery(
            request, branchId, classId, batchId, teacherId, fromDate, toDate, status), cancellationToken);

        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var (isValid, branchId, errorResult) = await ValidateBranchAccessAsync(cancellationToken);
        if (!isValid) return errorResult!;

        var session = await Db.ClassSessions.AsNoTracking()
            .Include(s => s.Batch).ThenInclude(b => b.Class)
            .FirstOrDefaultAsync(s => s.Id == id && !s.IsDeleted, cancellationToken);

        if (session == null || session.Batch?.Class?.BranchId != branchId)
        {
            return NotFound(ApiResponse<ClassSessionDto>.FailureResponse("Class session not found in this branch."));
        }

        var result = await Mediator.Send(new GetClassSessionByIdQuery(id), cancellationToken);
        if (!result.Success)
        {
            return NotFound(result);
        }

        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateClassSessionRequest request, CancellationToken cancellationToken)
    {
        var (isValid, branchId, errorResult) = await ValidateBranchAccessAsync(cancellationToken);
        if (!isValid) return errorResult!;

        var batch = await Db.Batches.AsNoTracking()
            .Include(b => b.Class)
            .FirstOrDefaultAsync(b => b.Id == request.BatchId && !b.IsDeleted, cancellationToken);

        if (batch == null || batch.Class?.BranchId != branchId)
        {
            return BadRequest(ApiResponse<ClassSessionDto>.FailureResponse("Target batch does not belong to your branch."));
        }

        var result = await Mediator.Send(new CreateClassSessionCommand(request), cancellationToken);
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateClassSessionRequest request, CancellationToken cancellationToken)
    {
        var (isValid, branchId, errorResult) = await ValidateBranchAccessAsync(cancellationToken);
        if (!isValid) return errorResult!;

        var session = await Db.ClassSessions.AsNoTracking()
            .Include(s => s.Batch).ThenInclude(b => b.Class)
            .FirstOrDefaultAsync(s => s.Id == id && !s.IsDeleted, cancellationToken);

        if (session == null || session.Batch?.Class?.BranchId != branchId)
        {
            return NotFound(ApiResponse<ClassSessionDto>.FailureResponse("Class session not found in this branch."));
        }

        request.Id = id;
        var result = await Mediator.Send(new UpdateClassSessionCommand(id, request), cancellationToken);
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var (isValid, branchId, errorResult) = await ValidateBranchAccessAsync(cancellationToken);
        if (!isValid) return errorResult!;

        var session = await Db.ClassSessions.AsNoTracking()
            .Include(s => s.Batch).ThenInclude(b => b.Class)
            .FirstOrDefaultAsync(s => s.Id == id && !s.IsDeleted, cancellationToken);

        if (session == null || session.Batch?.Class?.BranchId != branchId)
        {
            return NotFound(ApiResponse<bool>.FailureResponse("Class session not found in this branch."));
        }

        var result = await Mediator.Send(new DeleteClassSessionCommand(id), cancellationToken);
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    [HttpPatch("{id:guid}/cancel")]
    public async Task<IActionResult> Cancel(Guid id, [FromBody] CancelClassSessionRequest request, CancellationToken cancellationToken)
    {
        var (isValid, branchId, errorResult) = await ValidateBranchAccessAsync(cancellationToken);
        if (!isValid) return errorResult!;

        var session = await Db.ClassSessions.AsNoTracking()
            .Include(s => s.Batch).ThenInclude(b => b.Class)
            .FirstOrDefaultAsync(s => s.Id == id && !s.IsDeleted, cancellationToken);

        if (session == null || session.Batch?.Class?.BranchId != branchId)
        {
            return NotFound(ApiResponse<bool>.FailureResponse("Class session not found in this branch."));
        }

        var result = await Mediator.Send(new CancelClassSessionCommand(id, request), cancellationToken);
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    [HttpPatch("{id:guid}/complete")]
    public async Task<IActionResult> Complete(Guid id, [FromBody] CompleteClassSessionRequest request, CancellationToken cancellationToken)
    {
        var (isValid, branchId, errorResult) = await ValidateBranchAccessAsync(cancellationToken);
        if (!isValid) return errorResult!;

        var session = await Db.ClassSessions.AsNoTracking()
            .Include(s => s.Batch).ThenInclude(b => b.Class)
            .FirstOrDefaultAsync(s => s.Id == id && !s.IsDeleted, cancellationToken);

        if (session == null || session.Batch?.Class?.BranchId != branchId)
        {
            return NotFound(ApiResponse<bool>.FailureResponse("Class session not found in this branch."));
        }

        var result = await Mediator.Send(new CompleteClassSessionCommand(id, request), cancellationToken);
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    [HttpPost("generate-from-schedule")]
    public async Task<IActionResult> GenerateFromSchedule([FromBody] GenerateSessionsFromScheduleRequest request, CancellationToken cancellationToken)
    {
        var (isValid, branchId, errorResult) = await ValidateBranchAccessAsync(cancellationToken);
        if (!isValid) return errorResult!;

        var batch = await Db.Batches.AsNoTracking()
            .Include(b => b.Class)
            .FirstOrDefaultAsync(b => b.Id == request.BatchId && !b.IsDeleted, cancellationToken);

        if (batch == null || batch.Class?.BranchId != branchId)
        {
            return BadRequest(ApiResponse<List<ClassSessionDto>>.FailureResponse("Target batch does not belong to your branch."));
        }

        var result = await Mediator.Send(new GenerateSessionsFromScheduleCommand(request), cancellationToken);
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }
}
