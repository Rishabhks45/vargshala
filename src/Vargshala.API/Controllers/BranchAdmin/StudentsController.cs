using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vargshala.Application.Abstractions.CurrentUser;
using Vargshala.Application.Abstractions.Persistence;
using Vargshala.Application.Features.OrgAdmin.Students.Commands.CreateStudent;
using Vargshala.Application.Features.OrgAdmin.Students.Commands.DeleteStudent;
using Vargshala.Application.Features.OrgAdmin.Students.Commands.UpdateStudent;
using Vargshala.Application.Features.OrgAdmin.Students.Queries.GetNextStudentCode;
using Vargshala.Application.Features.OrgAdmin.Students.Queries.GetStudentBatches;
using Vargshala.Application.Features.OrgAdmin.Students.Queries.GetStudentById;
using Vargshala.Application.Features.OrgAdmin.Students.Queries.GetStudentsPaged;
using Vargshala.Contracts.Common;
using Vargshala.Contracts.Students;

namespace Vargshala.API.Controllers.BranchAdmin;

[Route("api/v1/branchadmin/students")]
[Authorize(Roles = "BranchAdmin,4")]
public class StudentsController : BaseBranchAdminController
{
    public StudentsController(
        IMediator mediator,
        ICurrentUser currentUser,
        IVargshalaDbContext db)
        : base(mediator, currentUser, db)
    {
    }

    [HttpGet("generate-code")]
    public async Task<IActionResult> GenerateCode(CancellationToken cancellationToken)
    {
        var (isValid, _, errorResult) = await ValidateBranchAccessAsync(cancellationToken);
        if (!isValid) return errorResult!;

        var result = await Mediator.Send(new GetNextStudentCodeQuery(), cancellationToken);
        return Ok(result);
    }

    [HttpGet]
    public async Task<IActionResult> GetPaged(
        [FromQuery] PagedRequest request,
        [FromQuery] string? className = null,
        [FromQuery] string? section = null,
        [FromQuery] bool? isActive = null,
        CancellationToken cancellationToken = default)
    {
        var (isValid, branchId, errorResult) = await ValidateBranchAccessAsync(cancellationToken);
        if (!isValid) return errorResult!;

        // Strictly scope to students enrolled in this branch's academic batches
        var query = new GetStudentsPagedQuery(request, className, section, isActive, branchId);
        var result = await Mediator.Send(query, cancellationToken);

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

        var studentExistsInBranch = await Db.Students.AsNoTracking()
            .AnyAsync(s => s.Id == id && !s.IsDeleted && s.BatchStudents.Any(bs => bs.Batch.Class.BranchId == branchId), cancellationToken);

        if (!studentExistsInBranch)
        {
            return NotFound(ApiResponse<StudentDto>.FailureResponse("Student not found in this branch."));
        }

        var result = await Mediator.Send(new GetStudentByIdQuery(id), cancellationToken);
        if (!result.Success)
        {
            return NotFound(result);
        }

        return Ok(result);
    }

    [HttpGet("{id:guid}/batches")]
    public async Task<IActionResult> GetBatches(Guid id, CancellationToken cancellationToken)
    {
        var (isValid, branchId, errorResult) = await ValidateBranchAccessAsync(cancellationToken);
        if (!isValid) return errorResult!;

        var result = await Mediator.Send(new GetStudentBatchesQuery(id), cancellationToken);
        if (!result.Success || result.Data == null)
        {
            return Ok(result);
        }

        // Filter batches to only this branch
        var branchBatches = result.Data.Where(b => b.BranchId == branchId).ToList();
        return Ok(ApiResponse<List<Vargshala.Contracts.Batches.StudentBatchEnrollmentDto>>.SuccessResponse(branchBatches));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateStudentRequest request, CancellationToken cancellationToken)
    {
        var (isValid, _, errorResult) = await ValidateBranchAccessAsync(cancellationToken);
        if (!isValid) return errorResult!;

        var result = await Mediator.Send(new CreateStudentCommand(request), cancellationToken);
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateStudentRequest request, CancellationToken cancellationToken)
    {
        var (isValid, branchId, errorResult) = await ValidateBranchAccessAsync(cancellationToken);
        if (!isValid) return errorResult!;

        var studentExistsInBranch = await Db.Students.AsNoTracking()
            .AnyAsync(s => s.Id == id && !s.IsDeleted && s.BatchStudents.Any(bs => bs.Batch.Class.BranchId == branchId), cancellationToken);

        if (!studentExistsInBranch)
        {
            return NotFound(ApiResponse<StudentDto>.FailureResponse("Student not found in this branch."));
        }

        if (id != request.Id)
        {
            request.Id = id;
        }

        var result = await Mediator.Send(new UpdateStudentCommand(request), cancellationToken);
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

        var studentExistsInBranch = await Db.Students.AsNoTracking()
            .AnyAsync(s => s.Id == id && !s.IsDeleted && s.BatchStudents.Any(bs => bs.Batch.Class.BranchId == branchId), cancellationToken);

        if (!studentExistsInBranch)
        {
            return NotFound(ApiResponse<bool>.FailureResponse("Student not found in this branch."));
        }

        var result = await Mediator.Send(new DeleteStudentCommand(id), cancellationToken);
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }
}
