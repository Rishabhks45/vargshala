using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vargshala.Application.Abstractions.Security;
using Vargshala.Application.Features.OrgAdmin.Teachers.Commands.CreateTeacher;
using Vargshala.Application.Features.OrgAdmin.Teachers.Commands.DeleteTeacher;
using Vargshala.Application.Features.OrgAdmin.Teachers.Commands.UpdateTeacher;
using Vargshala.Application.Features.OrgAdmin.Teachers.Queries.GetNextTeacherCode;
using Vargshala.Application.Features.OrgAdmin.Teachers.Queries.GetTeacherById;
using Vargshala.Application.Features.OrgAdmin.Teachers.Queries.GetTeachersPaged;
using Vargshala.Contracts.Common;
using Vargshala.Contracts.Teachers;

namespace Vargshala.API.Controllers.BranchAdmin;

[Route("api/v1/branchadmin/teachers")]
[Authorize(Roles = "BranchAdmin,4")]
public class TeachersController : BaseBranchAdminController
{
    public TeachersController(
        IMediator mediator,
        IBranchAuthorizationService branchAuthService)
        : base(mediator, branchAuthService)
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

        var canAccess = await BranchAuthService.CanAccessTeacherAsync(id, branchId, cancellationToken);
        if (!canAccess)
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

        // Automatically associate teacher with this branch
        await BranchAuthService.EnsureTeacherBranchAccessAsync(result.Data.Id, branchId, cancellationToken);

        return CreatedAtAction(nameof(GetById), new { id = result.Data.Id }, result);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateTeacherRequest request, CancellationToken cancellationToken)
    {
        var (isValid, branchId, errorResult) = await ValidateBranchAccessAsync(cancellationToken);
        if (!isValid) return errorResult!;

        var canAccess = await BranchAuthService.CanAccessTeacherAsync(id, branchId, cancellationToken);
        if (!canAccess)
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

        var canAccess = await BranchAuthService.CanAccessTeacherAsync(id, branchId, cancellationToken);
        if (!canAccess)
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
