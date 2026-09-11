using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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

namespace Vargshala.API.Controllers.OrgAdmin;

[ApiController]
[Route("api/v1/orgadmin/class-sessions")]
[Authorize(Roles = "OrganizationAdmin,1,SuperAdmin,1001,Teacher,2")]
public class ClassSessionsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ClassSessionsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetPaged(
        [FromQuery] PagedRequest request,
        [FromQuery] Guid? branchId = null,
        [FromQuery] Guid? classId = null,
        [FromQuery] Guid? batchId = null,
        [FromQuery] Guid? teacherId = null,
        [FromQuery] DateOnly? fromDate = null,
        [FromQuery] DateOnly? toDate = null,
        [FromQuery] string? status = null)
    {
        var result = await _mediator.Send(new GetClassSessionsPagedQuery(
            request, branchId, classId, batchId, teacherId, fromDate, toDate, status));

        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _mediator.Send(new GetClassSessionByIdQuery(id));
        if (!result.Success)
        {
            return NotFound(result);
        }

        return Ok(result);
    }

    [HttpPost]
    [Authorize(Roles = "OrganizationAdmin,1,SuperAdmin,1001")]
    public async Task<IActionResult> Create([FromBody] CreateClassSessionRequest request)
    {
        var result = await _mediator.Send(new CreateClassSessionCommand(request));
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateClassSessionRequest request)
    {
        if (id != request.Id)
        {
            return BadRequest(ApiResponse<ClassSessionDto>.FailureResponse("Mismatched Class Session ID."));
        }

        var result = await _mediator.Send(new UpdateClassSessionCommand(id, request));
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "OrganizationAdmin,1,SuperAdmin,1001")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _mediator.Send(new DeleteClassSessionCommand(id));
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    [HttpPatch("{id:guid}/cancel")]
    public async Task<IActionResult> Cancel(Guid id, [FromBody] CancelClassSessionRequest request)
    {
        var result = await _mediator.Send(new CancelClassSessionCommand(id, request));
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    [HttpPatch("{id:guid}/complete")]
    public async Task<IActionResult> Complete(Guid id, [FromBody] CompleteClassSessionRequest request)
    {
        var result = await _mediator.Send(new CompleteClassSessionCommand(id, request));
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    [HttpPost("generate")]
    [Authorize(Roles = "OrganizationAdmin,1,SuperAdmin,1001")]
    public async Task<IActionResult> GenerateFromSchedule([FromBody] GenerateSessionsFromScheduleRequest request)
    {
        var result = await _mediator.Send(new GenerateSessionsFromScheduleCommand(request));
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }
}
