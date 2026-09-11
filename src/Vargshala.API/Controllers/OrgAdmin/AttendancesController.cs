using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vargshala.Application.Features.OrgAdmin.Attendances.Commands.SaveSessionAttendance;
using Vargshala.Application.Features.OrgAdmin.Attendances.Queries.GetBatchAttendanceOverview;
using Vargshala.Application.Features.OrgAdmin.Attendances.Queries.GetDateWiseReport;
using Vargshala.Application.Features.OrgAdmin.Attendances.Queries.GetSessionAttendanceSheet;
using Vargshala.Application.Features.OrgAdmin.Attendances.Queries.GetStudentWiseReport;
using Vargshala.Contracts.Attendances;
using Vargshala.Contracts.Common;

namespace Vargshala.API.Controllers.OrgAdmin;

[ApiController]
[Route("api/v1/orgadmin/attendances")]
[Authorize(Roles = "OrganizationAdmin,1,SuperAdmin,1001,Teacher,2")]
public class AttendancesController : ControllerBase
{
    private readonly IMediator _mediator;

    public AttendancesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("session/{sessionId:guid}")]
    public async Task<IActionResult> GetSessionAttendanceSheet(Guid sessionId)
    {
        var result = await _mediator.Send(new GetSessionAttendanceSheetQuery(sessionId));
        if (!result.Success)
        {
            return NotFound(result);
        }

        return Ok(result);
    }

    [HttpPost("session/{sessionId:guid}")]
    public async Task<IActionResult> SaveSessionAttendance(Guid sessionId, [FromBody] MarkSessionAttendanceRequest request)
    {
        if (request == null)
        {
            return BadRequest(ApiResponse<SessionAttendanceSheetDto>.FailureResponse("Invalid request body."));
        }

        request.ClassSessionId = sessionId;

        var result = await _mediator.Send(new SaveSessionAttendanceCommand(request));
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    [HttpGet("batch/{batchId:guid}/overview")]
    public async Task<IActionResult> GetBatchOverview(Guid batchId, [FromQuery] DateOnly? date)
    {
        var result = await _mediator.Send(new GetBatchAttendanceOverviewQuery(batchId, date));
        if (!result.Success)
        {
            return NotFound(result);
        }

        return Ok(result);
    }

    [HttpGet("batch/{batchId:guid}/date-report")]
    public async Task<IActionResult> GetDateWiseReport(Guid batchId, [FromQuery] DateOnly? fromDate, [FromQuery] DateOnly? toDate)
    {
        var result = await _mediator.Send(new GetDateWiseReportQuery(batchId, fromDate, toDate));
        return Ok(result);
    }

    [HttpGet("batch/{batchId:guid}/student-report")]
    public async Task<IActionResult> GetStudentWiseReport(Guid batchId)
    {
        var result = await _mediator.Send(new GetStudentWiseReportQuery(batchId));
        return Ok(result);
    }
}
