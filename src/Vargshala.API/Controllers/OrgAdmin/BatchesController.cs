using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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

namespace Vargshala.API.Controllers.OrgAdmin;

[ApiController]
[Route("api/v1/orgadmin/batches")]
[Authorize(Roles = "OrganizationAdmin,1,SuperAdmin,1001,BranchAdmin,4,Teacher,2")]
public class BatchesController : ControllerBase
{
    private readonly IMediator _mediator;

    public BatchesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetPaged(
        [FromQuery] PagedRequest request,
        [FromQuery] Guid? branchId = null,
        [FromQuery] Guid? classId = null,
        [FromQuery] Guid? subjectId = null,
        [FromQuery] bool? isActive = null)
    {
        var result = await _mediator.Send(new GetBatchesPagedQuery(request, branchId, classId, subjectId, isActive));
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    [HttpGet("all-active")]
    public async Task<IActionResult> GetAllActive([FromQuery] Guid? classId = null)
    {
        var result = await _mediator.Send(new GetAllActiveBatchesQuery(classId));
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _mediator.Send(new GetBatchByIdQuery(id));
        if (!result.Success)
        {
            return NotFound(result);
        }

        return Ok(result);
    }

    [HttpPost]
    [Authorize(Roles = "OrganizationAdmin,1,SuperAdmin,1001,BranchAdmin,4")]
    public async Task<IActionResult> Create([FromBody] CreateBatchRequest request)
    {
        var result = await _mediator.Send(new CreateBatchCommand(request));
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "OrganizationAdmin,1,SuperAdmin,1001,BranchAdmin,4")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateBatchRequest request)
    {
        if (id != request.Id)
        {
            return BadRequest(ApiResponse<BatchDto>.FailureResponse("Mismatched Batch ID."));
        }

        var result = await _mediator.Send(new UpdateBatchCommand(request));
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "OrganizationAdmin,1,SuperAdmin,1001,BranchAdmin,4")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _mediator.Send(new DeleteBatchCommand(id));
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    [HttpPatch("{id:guid}/toggle-status")]
    [Authorize(Roles = "OrganizationAdmin,1,SuperAdmin,1001,BranchAdmin,4")]
    public async Task<IActionResult> ToggleStatus(Guid id)
    {
        var result = await _mediator.Send(new ToggleBatchStatusCommand(id));
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    #region Teacher Assignments
    [HttpGet("{id:guid}/teachers")]
    public async Task<IActionResult> GetTeachers(Guid id)
    {
        var result = await _mediator.Send(new GetBatchTeachersQuery(id));
        return Ok(result);
    }

    [HttpPost("{id:guid}/teachers")]
    [Authorize(Roles = "OrganizationAdmin,1,SuperAdmin,1001,BranchAdmin,4")]
    public async Task<IActionResult> AssignTeacher(Guid id, [FromBody] AssignTeacherToBatchRequest request)
    {
        var result = await _mediator.Send(new AssignTeacherToBatchCommand(id, request));
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    [HttpDelete("{id:guid}/teachers/{teacherId:guid}")]
    [Authorize(Roles = "OrganizationAdmin,1,SuperAdmin,1001,BranchAdmin,4")]
    public async Task<IActionResult> RemoveTeacher(Guid id, Guid teacherId, [FromQuery] Guid? subjectId = null)
    {
        var result = await _mediator.Send(new RemoveTeacherFromBatchCommand(id, teacherId, subjectId));
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }
    #endregion

    #region Student Enrollments
    [HttpGet("{id:guid}/students")]
    public async Task<IActionResult> GetStudents(Guid id)
    {
        var result = await _mediator.Send(new GetBatchStudentsQuery(id));
        return Ok(result);
    }

    [HttpPost("{id:guid}/students")]
    [Authorize(Roles = "OrganizationAdmin,1,SuperAdmin,1001,BranchAdmin,4")]
    public async Task<IActionResult> EnrollStudent(Guid id, [FromBody] EnrollStudentToBatchRequest request)
    {
        var result = await _mediator.Send(new EnrollStudentToBatchCommand(id, request));
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    [HttpDelete("{id:guid}/students/{studentId:guid}")]
    [Authorize(Roles = "OrganizationAdmin,1,SuperAdmin,1001,BranchAdmin,4")]
    public async Task<IActionResult> RemoveStudent(Guid id, Guid studentId)
    {
        var result = await _mediator.Send(new RemoveStudentFromBatchCommand(id, studentId));
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }
    #endregion
}
