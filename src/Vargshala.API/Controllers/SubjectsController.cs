using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vargshala.Application.Features.Subjects.Commands.CreateSubject;
using Vargshala.Application.Features.Subjects.Commands.DeleteSubject;
using Vargshala.Application.Features.Subjects.Commands.ToggleSubjectStatus;
using Vargshala.Application.Features.Subjects.Commands.UpdateSubject;
using Vargshala.Application.Features.Subjects.Queries.GetAllActiveSubjects;
using Vargshala.Application.Features.Subjects.Queries.GetSubjectById;
using Vargshala.Application.Features.Subjects.Queries.GetSubjects;
using Vargshala.Contracts.Common;
using Vargshala.Contracts.Subjects;

namespace Vargshala.API.Controllers;

[ApiController]
[Route("api/v1/subjects")]
[Authorize]
public class SubjectsController : ControllerBase
{
    private readonly IMediator _mediator;

    public SubjectsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetSubjects([FromQuery] PagedRequest request, [FromQuery] bool? isActive = null)
    {
        var result = await _mediator.Send(new GetSubjectsQuery(request, isActive));
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    [HttpGet("all-active")]
    public async Task<IActionResult> GetAllActiveSubjects()
    {
        var result = await _mediator.Send(new GetAllActiveSubjectsQuery());
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetSubjectById(Guid id)
    {
        var result = await _mediator.Send(new GetSubjectByIdQuery(id));
        if (!result.Success)
        {
            return NotFound(result);
        }

        return Ok(result);
    }

    [HttpPost]
    [Authorize(Roles = "OrganizationAdmin,SuperAdmin,1,1001")]
    public async Task<IActionResult> CreateSubject([FromBody] CreateSubjectRequest request)
    {
        var result = await _mediator.Send(new CreateSubjectCommand(request));
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return CreatedAtAction(nameof(GetSubjectById), new { id = result.Data!.Id }, result);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "OrganizationAdmin,SuperAdmin,1,1001")]
    public async Task<IActionResult> UpdateSubject(Guid id, [FromBody] UpdateSubjectRequest request)
    {
        if (id != request.Id)
        {
            request.Id = id;
        }

        var result = await _mediator.Send(new UpdateSubjectCommand(request));
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "OrganizationAdmin,SuperAdmin,1,1001")]
    public async Task<IActionResult> DeleteSubject(Guid id)
    {
        var result = await _mediator.Send(new DeleteSubjectCommand(id));
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    [HttpPatch("{id:guid}/toggle-status")]
    [Authorize(Roles = "OrganizationAdmin,SuperAdmin,1,1001")]
    public async Task<IActionResult> ToggleSubjectStatus(Guid id)
    {
        var result = await _mediator.Send(new ToggleSubjectStatusCommand(id));
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }
}
