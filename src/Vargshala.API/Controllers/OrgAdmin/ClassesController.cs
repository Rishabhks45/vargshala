using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vargshala.Application.Features.OrgAdmin.Classes.Commands.CreateClass;
using Vargshala.Application.Features.OrgAdmin.Classes.Commands.DeleteClass;
using Vargshala.Application.Features.OrgAdmin.Classes.Commands.ToggleClassStatus;
using Vargshala.Application.Features.OrgAdmin.Classes.Commands.UpdateClass;
using Vargshala.Application.Features.OrgAdmin.Classes.Queries.GetAllActiveClasses;
using Vargshala.Application.Features.OrgAdmin.Classes.Queries.GetClassById;
using Vargshala.Application.Features.OrgAdmin.Classes.Queries.GetClassesPaged;
using Vargshala.Contracts.Classes;
using Vargshala.Contracts.Common;

namespace Vargshala.API.Controllers.OrgAdmin;

[ApiController]
[Route("api/v1/orgadmin/classes")]
[Authorize(Roles = "OrganizationAdmin,1,SuperAdmin,1001,BranchAdmin,4")]
public class ClassesController : ControllerBase
{
    private readonly IMediator _mediator;

    public ClassesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetPaged(
        [FromQuery] PagedRequest request,
        [FromQuery] Guid? branchId = null,
        [FromQuery] bool? isActive = null)
    {
        var result = await _mediator.Send(new GetClassesPagedQuery(request, branchId, isActive));
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    [HttpGet("all-active")]
    public async Task<IActionResult> GetAllActive([FromQuery] Guid? branchId = null)
    {
        var result = await _mediator.Send(new GetAllActiveClassesQuery(branchId));
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _mediator.Send(new GetClassByIdQuery(id));
        if (!result.Success)
        {
            return NotFound(result);
        }

        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateClassRequest request)
    {
        var result = await _mediator.Send(new CreateClassCommand(request));
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateClassRequest request)
    {
        if (id != request.Id)
        {
            return BadRequest(ApiResponse<ClassDto>.FailureResponse("Mismatched Class ID."));
        }

        var result = await _mediator.Send(new UpdateClassCommand(request));
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _mediator.Send(new DeleteClassCommand(id));
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    [HttpPatch("{id:guid}/toggle-status")]
    public async Task<IActionResult> ToggleStatus(Guid id)
    {
        var result = await _mediator.Send(new ToggleClassStatusCommand(id));
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }
}
