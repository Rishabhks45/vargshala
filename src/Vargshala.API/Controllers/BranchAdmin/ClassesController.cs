using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vargshala.Application.Abstractions.Security;
using Vargshala.Application.Features.OrgAdmin.Classes.Commands.CreateClass;
using Vargshala.Application.Features.OrgAdmin.Classes.Commands.DeleteClass;
using Vargshala.Application.Features.OrgAdmin.Classes.Commands.ToggleClassStatus;
using Vargshala.Application.Features.OrgAdmin.Classes.Commands.UpdateClass;
using Vargshala.Application.Features.OrgAdmin.Classes.Queries.GetAllActiveClasses;
using Vargshala.Application.Features.OrgAdmin.Classes.Queries.GetClassById;
using Vargshala.Application.Features.OrgAdmin.Classes.Queries.GetClassesPaged;
using Vargshala.Contracts.Classes;
using Vargshala.Contracts.Common;

namespace Vargshala.API.Controllers.BranchAdmin;

[Route("api/v1/branchadmin/classes")]
[Authorize(Roles = "BranchAdmin,4")]
public class ClassesController : BaseBranchAdminController
{
    public ClassesController(
        IMediator mediator,
        IBranchAuthorizationService branchAuthService)
        : base(mediator, branchAuthService)
    {
    }

    [HttpGet]
    public async Task<IActionResult> GetPaged(
        [FromQuery] PagedRequest request,
        [FromQuery] bool? isActive = null,
        CancellationToken cancellationToken = default)
    {
        var (isValid, branchId, errorResult) = await ValidateBranchAccessAsync(cancellationToken);
        if (!isValid) return errorResult!;

        var result = await Mediator.Send(new GetClassesPagedQuery(request, branchId, isActive), cancellationToken);
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    [HttpGet("all-active")]
    public async Task<IActionResult> GetAllActive(CancellationToken cancellationToken)
    {
        var (isValid, branchId, errorResult) = await ValidateBranchAccessAsync(cancellationToken);
        if (!isValid) return errorResult!;

        var result = await Mediator.Send(new GetAllActiveClassesQuery(branchId), cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var (isValid, branchId, errorResult) = await ValidateBranchAccessAsync(cancellationToken);
        if (!isValid) return errorResult!;

        if (!await BranchAuthService.CanAccessClassAsync(id, branchId, cancellationToken))
        {
            return NotFound(ApiResponse<ClassDto>.FailureResponse("Class not found in this branch."));
        }

        var result = await Mediator.Send(new GetClassByIdQuery(id), cancellationToken);
        if (!result.Success)
        {
            return NotFound(result);
        }

        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateClassRequest request, CancellationToken cancellationToken)
    {
        var (isValid, branchId, errorResult) = await ValidateBranchAccessAsync(cancellationToken);
        if (!isValid) return errorResult!;

        // Strictly bind to authenticated branch
        request.BranchId = branchId;

        var result = await Mediator.Send(new CreateClassCommand(request), cancellationToken);
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateClassRequest request, CancellationToken cancellationToken)
    {
        var (isValid, branchId, errorResult) = await ValidateBranchAccessAsync(cancellationToken);
        if (!isValid) return errorResult!;

        if (id != request.Id)
        {
            return BadRequest(ApiResponse<ClassDto>.FailureResponse("Mismatched Class ID."));
        }

        if (!await BranchAuthService.CanAccessClassAsync(id, branchId, cancellationToken))
        {
            return NotFound(ApiResponse<ClassDto>.FailureResponse("Class not found in this branch."));
        }

        // Strictly bind to authenticated branch
        request.BranchId = branchId;

        var result = await Mediator.Send(new UpdateClassCommand(request), cancellationToken);
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

        if (!await BranchAuthService.CanAccessClassAsync(id, branchId, cancellationToken))
        {
            return NotFound(ApiResponse<bool>.FailureResponse("Class not found in this branch."));
        }

        var result = await Mediator.Send(new DeleteClassCommand(id), cancellationToken);
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

        if (!await BranchAuthService.CanAccessClassAsync(id, branchId, cancellationToken))
        {
            return NotFound(ApiResponse<bool>.FailureResponse("Class not found in this branch."));
        }

        var result = await Mediator.Send(new ToggleClassStatusCommand(id), cancellationToken);
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }
}
