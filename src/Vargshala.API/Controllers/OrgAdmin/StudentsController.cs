using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vargshala.Application.Features.OrgAdmin.Students.Commands.CreateStudent;
using Vargshala.Application.Features.OrgAdmin.Students.Commands.DeleteStudent;
using Vargshala.Application.Features.OrgAdmin.Students.Commands.UpdateStudent;
using Vargshala.Application.Features.OrgAdmin.Students.Queries.GetNextStudentCode;
using Vargshala.Application.Features.OrgAdmin.Students.Queries.GetStudentById;
using Vargshala.Application.Features.OrgAdmin.Students.Queries.GetStudentsPaged;
using Vargshala.Contracts.Common;
using Vargshala.Contracts.Students;

namespace Vargshala.API.Controllers.OrgAdmin;

[ApiController]
[Route("api/v1/orgadmin/students")]
[Authorize(Roles = "OrganizationAdmin,1")]
public class StudentsController : ControllerBase
{
    private readonly IMediator _mediator;

    public StudentsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("generate-code")]
    public async Task<IActionResult> GenerateCode(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetNextStudentCodeQuery(), cancellationToken);
        return Ok(result);
    }

    [HttpGet]
    public async Task<IActionResult> GetPaged(
        [FromQuery] PagedRequest request,
        [FromQuery] string? className = null,
        [FromQuery] string? section = null,
        [FromQuery] bool? isActive = null)
    {
        var query = new GetStudentsPagedQuery(request, className, section, isActive);
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
        var result = await _mediator.Send(new GetStudentByIdQuery(id));

        if (!result.Success)
        {
            return NotFound(result);
        }

        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateStudentRequest request)
    {
        var result = await _mediator.Send(new CreateStudentCommand(request));

        if (!result.Success)
        {
            return BadRequest(result);
        }

        return CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateStudentRequest request)
    {
        if (id != request.Id)
        {
            request.Id = id;
        }

        var result = await _mediator.Send(new UpdateStudentCommand(request));

        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _mediator.Send(new DeleteStudentCommand(id));

        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }
}
