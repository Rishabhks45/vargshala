using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vargshala.Application.Features.OrgAdmin.Teachers.Commands.CreateTeacher;
using Vargshala.Application.Features.OrgAdmin.Teachers.Commands.DeleteTeacher;
using Vargshala.Application.Features.OrgAdmin.Teachers.Commands.UpdateTeacher;
using Vargshala.Application.Features.OrgAdmin.Teachers.Queries.GetNextTeacherCode;
using Vargshala.Application.Features.OrgAdmin.Teachers.Queries.GetTeacherById;
using Vargshala.Application.Features.OrgAdmin.Teachers.Queries.GetTeachersPaged;
using Vargshala.Contracts.Common;
using Vargshala.Contracts.Teachers;

namespace Vargshala.API.Controllers.OrgAdmin;

[ApiController]
[Route("api/v1/orgadmin/teachers")]
[Authorize(Roles = "OrganizationAdmin,1")]
public class TeachersController : ControllerBase
{
    private readonly IMediator _mediator;

    public TeachersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("generate-code")]
    public async Task<IActionResult> GenerateCode(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetNextTeacherCodeQuery(), cancellationToken);
        return Ok(result);
    }

    [HttpGet]
    public async Task<IActionResult> GetPaged(
        [FromQuery] PagedRequest request,
        [FromQuery] string? department = null,
        [FromQuery] string? designation = null,
        [FromQuery] bool? isActive = null)
    {
        var query = new GetTeachersPagedQuery(request, department, designation, isActive);
        var result = await _mediator.Send(query);

        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _mediator.Send(new GetTeacherByIdQuery(id));

        if (!result.Success)
        {
            return NotFound(result);
        }

        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateTeacherRequest request)
    {
        var result = await _mediator.Send(new CreateTeacherCommand(request));

        if (!result.Success)
        {
            return BadRequest(result);
        }

        return CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateTeacherRequest request)
    {
        if (id != request.Id)
        {
            request.Id = id;
        }

        var result = await _mediator.Send(new UpdateTeacherCommand(request));

        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _mediator.Send(new DeleteTeacherCommand(id));

        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }
}
