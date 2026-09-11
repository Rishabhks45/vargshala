using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vargshala.Application.Abstractions.CurrentUser;
using Vargshala.Application.Abstractions.Persistence;
using Vargshala.Application.Features.OrgAdmin.Batches.Commands.AssignTeacherToBatch;
using Vargshala.Application.Features.OrgAdmin.Batches.Commands.CreateBatch;
using Vargshala.Application.Features.OrgAdmin.Batches.Commands.DeleteBatch;
using Vargshala.Application.Features.OrgAdmin.Batches.Commands.EnrollStudentToBatch;
using Vargshala.Application.Features.OrgAdmin.Batches.Commands.RemoveStudentFromBatch;
using Vargshala.Application.Features.OrgAdmin.Batches.Commands.RemoveTeacherFromBatch;
using Vargshala.Application.Features.OrgAdmin.Batches.Commands.ToggleBatchStatus;
using Vargshala.Application.Features.OrgAdmin.Batches.Commands.UpdateBatch;
using Vargshala.Application.Features.OrgAdmin.Batches.Queries.GetAllActiveBatches;
using Vargshala.Application.Features.OrgAdmin.Batches.Queries.GetBatchById;
using Vargshala.Application.Features.OrgAdmin.Batches.Queries.GetBatchesPaged;
using Vargshala.Application.Features.OrgAdmin.Batches.Queries.GetBatchStudents;
using Vargshala.Application.Features.OrgAdmin.Batches.Queries.GetBatchTeachers;
using Vargshala.Contracts.Batches;
using Vargshala.Contracts.Common;

namespace Vargshala.API.Controllers.BranchAdmin;

[Route("api/v1/branchadmin/batches")]
[Authorize(Roles = "BranchAdmin,4")]
public class BatchesController : BaseBranchAdminController
{
    public BatchesController(
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
        [FromQuery] Guid? subjectId = null,
        [FromQuery] bool? isActive = null,
        CancellationToken cancellationToken = default)
    {
        var (isValid, branchId, errorResult) = await ValidateBranchAccessAsync(cancellationToken);
        if (!isValid) return errorResult!;

        var result = await Mediator.Send(new GetBatchesPagedQuery(request, branchId, classId, subjectId, isActive), cancellationToken);
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    [HttpGet("all-active")]
    public async Task<IActionResult> GetAllActive(
        [FromQuery] Guid? classId = null,
        CancellationToken cancellationToken = default)
    {
        var (isValid, branchId, errorResult) = await ValidateBranchAccessAsync(cancellationToken);
        if (!isValid) return errorResult!;

        var result = await Mediator.Send(new GetAllActiveBatchesQuery(classId, branchId), cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var (isValid, branchId, errorResult) = await ValidateBranchAccessAsync(cancellationToken);
        if (!isValid) return errorResult!;

        var batch = await Db.Batches.AsNoTracking()
            .Include(b => b.Class)
            .FirstOrDefaultAsync(b => b.Id == id && !b.IsDeleted, cancellationToken);

        if (batch == null || batch.Class?.BranchId != branchId)
        {
            return NotFound(ApiResponse<BatchDto>.FailureResponse("Batch not found in this branch."));
        }

        var result = await Mediator.Send(new GetBatchByIdQuery(id), cancellationToken);
        if (!result.Success)
        {
            return NotFound(result);
        }

        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateBatchRequest request, CancellationToken cancellationToken)
    {
        var (isValid, branchId, errorResult) = await ValidateBranchAccessAsync(cancellationToken);
        if (!isValid) return errorResult!;

        var targetClass = await Db.Classes.AsNoTracking().FirstOrDefaultAsync(c => c.Id == request.ClassId && !c.IsDeleted, cancellationToken);
        if (targetClass == null || targetClass.BranchId != branchId)
        {
            return BadRequest(ApiResponse<BatchDto>.FailureResponse("Target class does not belong to your branch."));
        }

        var result = await Mediator.Send(new CreateBatchCommand(request), cancellationToken);
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateBatchRequest request, CancellationToken cancellationToken)
    {
        var (isValid, branchId, errorResult) = await ValidateBranchAccessAsync(cancellationToken);
        if (!isValid) return errorResult!;

        if (id != request.Id)
        {
            return BadRequest(ApiResponse<BatchDto>.FailureResponse("Mismatched Batch ID."));
        }

        var batch = await Db.Batches.AsNoTracking()
            .Include(b => b.Class)
            .FirstOrDefaultAsync(b => b.Id == id && !b.IsDeleted, cancellationToken);

        if (batch == null || batch.Class?.BranchId != branchId)
        {
            return NotFound(ApiResponse<BatchDto>.FailureResponse("Batch not found in this branch."));
        }

        var targetClass = await Db.Classes.AsNoTracking().FirstOrDefaultAsync(c => c.Id == request.ClassId && !c.IsDeleted, cancellationToken);
        if (targetClass == null || targetClass.BranchId != branchId)
        {
            return BadRequest(ApiResponse<BatchDto>.FailureResponse("Target class does not belong to your branch."));
        }

        var result = await Mediator.Send(new UpdateBatchCommand(request), cancellationToken);
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

        var batch = await Db.Batches.AsNoTracking()
            .Include(b => b.Class)
            .FirstOrDefaultAsync(b => b.Id == id && !b.IsDeleted, cancellationToken);

        if (batch == null || batch.Class?.BranchId != branchId)
        {
            return NotFound(ApiResponse<bool>.FailureResponse("Batch not found in this branch."));
        }

        var result = await Mediator.Send(new DeleteBatchCommand(id), cancellationToken);
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    [HttpPatch("{id:guid}/toggle-status")]
    public async Task<IActionResult> ToggleStatus(Guid id, CancellationToken cancellationToken)
    {
        var (isValid, branchId, errorResult) = await ValidateBranchAccessAsync(cancellationToken);
        if (!isValid) return errorResult!;

        var batch = await Db.Batches.AsNoTracking()
            .Include(b => b.Class)
            .FirstOrDefaultAsync(b => b.Id == id && !b.IsDeleted, cancellationToken);

        if (batch == null || batch.Class?.BranchId != branchId)
        {
            return NotFound(ApiResponse<bool>.FailureResponse("Batch not found in this branch."));
        }

        var result = await Mediator.Send(new ToggleBatchStatusCommand(id), cancellationToken);
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    #region Teacher Assignments
    [HttpGet("{id:guid}/teachers")]
    public async Task<IActionResult> GetTeachers(Guid id, CancellationToken cancellationToken)
    {
        var (isValid, branchId, errorResult) = await ValidateBranchAccessAsync(cancellationToken);
        if (!isValid) return errorResult!;

        var batch = await Db.Batches.AsNoTracking().Include(b => b.Class).FirstOrDefaultAsync(b => b.Id == id && !b.IsDeleted, cancellationToken);
        if (batch == null || batch.Class?.BranchId != branchId)
        {
            return NotFound(ApiResponse<List<BatchTeacherDto>>.FailureResponse("Batch not found in this branch."));
        }

        var result = await Mediator.Send(new GetBatchTeachersQuery(id), cancellationToken);
        return Ok(result);
    }

    [HttpPost("{id:guid}/teachers")]
    public async Task<IActionResult> AssignTeacher(Guid id, [FromBody] AssignTeacherToBatchRequest request, CancellationToken cancellationToken)
    {
        var (isValid, branchId, errorResult) = await ValidateBranchAccessAsync(cancellationToken);
        if (!isValid) return errorResult!;

        var batch = await Db.Batches.AsNoTracking().Include(b => b.Class).FirstOrDefaultAsync(b => b.Id == id && !b.IsDeleted, cancellationToken);
        if (batch == null || batch.Class?.BranchId != branchId)
        {
            return NotFound(ApiResponse<BatchTeacherDto>.FailureResponse("Batch not found in this branch."));
        }

        var result = await Mediator.Send(new AssignTeacherToBatchCommand(id, request), cancellationToken);
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    [HttpDelete("{id:guid}/teachers/{teacherId:guid}")]
    public async Task<IActionResult> RemoveTeacher(Guid id, Guid teacherId, [FromQuery] Guid? subjectId = null, CancellationToken cancellationToken = default)
    {
        var (isValid, branchId, errorResult) = await ValidateBranchAccessAsync(cancellationToken);
        if (!isValid) return errorResult!;

        var batch = await Db.Batches.AsNoTracking().Include(b => b.Class).FirstOrDefaultAsync(b => b.Id == id && !b.IsDeleted, cancellationToken);
        if (batch == null || batch.Class?.BranchId != branchId)
        {
            return NotFound(ApiResponse<bool>.FailureResponse("Batch not found in this branch."));
        }

        var result = await Mediator.Send(new RemoveTeacherFromBatchCommand(id, teacherId, subjectId), cancellationToken);
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }
    #endregion

    #region Student Enrollments
    [HttpGet("{id:guid}/students")]
    public async Task<IActionResult> GetStudents(Guid id, CancellationToken cancellationToken)
    {
        var (isValid, branchId, errorResult) = await ValidateBranchAccessAsync(cancellationToken);
        if (!isValid) return errorResult!;

        var batch = await Db.Batches.AsNoTracking().Include(b => b.Class).FirstOrDefaultAsync(b => b.Id == id && !b.IsDeleted, cancellationToken);
        if (batch == null || batch.Class?.BranchId != branchId)
        {
            return NotFound(ApiResponse<List<BatchStudentDto>>.FailureResponse("Batch not found in this branch."));
        }

        var result = await Mediator.Send(new GetBatchStudentsQuery(id), cancellationToken);
        return Ok(result);
    }

    [HttpPost("{id:guid}/students")]
    public async Task<IActionResult> EnrollStudent(Guid id, [FromBody] EnrollStudentToBatchRequest request, CancellationToken cancellationToken)
    {
        var (isValid, branchId, errorResult) = await ValidateBranchAccessAsync(cancellationToken);
        if (!isValid) return errorResult!;

        var batch = await Db.Batches.AsNoTracking().Include(b => b.Class).FirstOrDefaultAsync(b => b.Id == id && !b.IsDeleted, cancellationToken);
        if (batch == null || batch.Class?.BranchId != branchId)
        {
            return NotFound(ApiResponse<BatchStudentDto>.FailureResponse("Batch not found in this branch."));
        }

        var result = await Mediator.Send(new EnrollStudentToBatchCommand(id, request), cancellationToken);
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    [HttpDelete("{id:guid}/students/{studentId:guid}")]
    public async Task<IActionResult> RemoveStudent(Guid id, Guid studentId, CancellationToken cancellationToken)
    {
        var (isValid, branchId, errorResult) = await ValidateBranchAccessAsync(cancellationToken);
        if (!isValid) return errorResult!;

        var batch = await Db.Batches.AsNoTracking().Include(b => b.Class).FirstOrDefaultAsync(b => b.Id == id && !b.IsDeleted, cancellationToken);
        if (batch == null || batch.Class?.BranchId != branchId)
        {
            return NotFound(ApiResponse<bool>.FailureResponse("Batch not found in this branch."));
        }

        var result = await Mediator.Send(new RemoveStudentFromBatchCommand(id, studentId), cancellationToken);
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }
    #endregion
}
