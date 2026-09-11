using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vargshala.Application.Abstractions.CurrentUser;
using Vargshala.Application.Abstractions.Persistence;
using Vargshala.Application.Features.OrgAdmin.Teachers.Commands.CreateTeacher;
using Vargshala.Application.Features.OrgAdmin.Teachers.Commands.DeleteTeacher;
using Vargshala.Application.Features.OrgAdmin.Teachers.Commands.UpdateTeacher;
using Vargshala.Application.Features.OrgAdmin.Teachers.Queries.GetNextTeacherCode;
using Vargshala.Application.Features.OrgAdmin.Teachers.Queries.GetTeacherById;
using Vargshala.Application.Features.OrgAdmin.Teachers.Queries.GetTeachersPaged;
using Vargshala.Contracts.Common;
using Vargshala.Contracts.Teachers;
using Vargshala.Domain.Entities;

namespace Vargshala.API.Controllers.BranchAdmin;

[Route("api/v1/branchadmin/teachers")]
[Authorize(Roles = "BranchAdmin,4")]
public class TeachersController : BaseBranchAdminController
{
    public TeachersController(
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

        var result = await Mediator.Send(new GetNextTeacherCodeQuery(), cancellationToken);
        return Ok(result);
    }

    [HttpGet]
    public async Task<IActionResult> GetPaged(
        [FromQuery] PagedRequest request,
        [FromQuery] string? department = null,
        [FromQuery] string? designation = null,
        [FromQuery] bool? isActive = null,
        CancellationToken cancellationToken = default)
    {
        var (isValid, branchId, errorResult) = await ValidateBranchAccessAsync(cancellationToken);
        if (!isValid) return errorResult!;

        var query = new GetTeachersPagedQuery(request, department, designation, isActive, branchId);
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

        var teacher = await Db.Teachers.AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == id && !t.IsDeleted, cancellationToken);

        if (teacher == null)
        {
            return NotFound(ApiResponse<TeacherDto>.FailureResponse("Teacher not found."));
        }

        var hasBranchAccess = await Db.UserBranchAccesses.AnyAsync(
            uba => uba.UserId == teacher.UserId && uba.BranchId == branchId && uba.IsActive,
            cancellationToken) || !await Db.UserBranchAccesses.AnyAsync(uba => uba.UserId == teacher.UserId && uba.IsActive, cancellationToken);

        if (!hasBranchAccess)
        {
            return NotFound(ApiResponse<TeacherDto>.FailureResponse("Teacher not found in this branch."));
        }

        var result = await Mediator.Send(new GetTeacherByIdQuery(id), cancellationToken);
        if (!result.Success)
        {
            return NotFound(result);
        }

        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateTeacherRequest request, CancellationToken cancellationToken)
    {
        var (isValid, branchId, errorResult) = await ValidateBranchAccessAsync(cancellationToken);
        if (!isValid) return errorResult!;

        var result = await Mediator.Send(new CreateTeacherCommand(request), cancellationToken);
        if (!result.Success || result.Data == null)
        {
            return BadRequest(result);
        }

        // Automatically associate teacher with this branch in UserBranchAccess
        var accessExists = await Db.UserBranchAccesses.AnyAsync(
            a => a.UserId == result.Data.UserId && a.BranchId == branchId,
            cancellationToken);

        if (!accessExists)
        {
            await Db.UserBranchAccesses.AddAsync(new UserBranchAccess
            {
                Id = Guid.NewGuid(),
                UserId = result.Data.UserId,
                BranchId = branchId,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = CurrentUser.UserId
            }, cancellationToken);

            await Db.SaveChangesAsync(cancellationToken);
        }

        return CreatedAtAction(nameof(GetById), new { id = result.Data.Id }, result);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateTeacherRequest request, CancellationToken cancellationToken)
    {
        var (isValid, branchId, errorResult) = await ValidateBranchAccessAsync(cancellationToken);
        if (!isValid) return errorResult!;

        var teacher = await Db.Teachers.AsNoTracking().FirstOrDefaultAsync(t => t.Id == id && !t.IsDeleted, cancellationToken);
        if (teacher == null)
        {
            return NotFound(ApiResponse<TeacherDto>.FailureResponse("Teacher not found."));
        }

        var hasBranchAccess = await Db.UserBranchAccesses.AnyAsync(
            uba => uba.UserId == teacher.UserId && uba.BranchId == branchId && uba.IsActive,
            cancellationToken) || !await Db.UserBranchAccesses.AnyAsync(uba => uba.UserId == teacher.UserId && uba.IsActive, cancellationToken);

        if (!hasBranchAccess)
        {
            return NotFound(ApiResponse<TeacherDto>.FailureResponse("Teacher not found in this branch."));
        }

        if (id != request.Id)
        {
            request.Id = id;
        }

        var result = await Mediator.Send(new UpdateTeacherCommand(request), cancellationToken);
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

        var teacher = await Db.Teachers.AsNoTracking().FirstOrDefaultAsync(t => t.Id == id && !t.IsDeleted, cancellationToken);
        if (teacher == null)
        {
            return NotFound(ApiResponse<bool>.FailureResponse("Teacher not found."));
        }

        var hasBranchAccess = await Db.UserBranchAccesses.AnyAsync(
            uba => uba.UserId == teacher.UserId && uba.BranchId == branchId && uba.IsActive,
            cancellationToken) || !await Db.UserBranchAccesses.AnyAsync(uba => uba.UserId == teacher.UserId && uba.IsActive, cancellationToken);

        if (!hasBranchAccess)
        {
            return NotFound(ApiResponse<bool>.FailureResponse("Teacher not found in this branch."));
        }

        var result = await Mediator.Send(new DeleteTeacherCommand(id), cancellationToken);
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }
}
