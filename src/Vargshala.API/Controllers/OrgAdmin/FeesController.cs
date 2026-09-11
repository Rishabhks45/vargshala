using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vargshala.Application.Features.OrgAdmin.Fees.Commands.AssignStudentFee;
using Vargshala.Application.Features.OrgAdmin.Fees.Commands.CollectPayment;
using Vargshala.Application.Features.OrgAdmin.Fees.Queries.GetFeeStatistics;
using Vargshala.Application.Features.OrgAdmin.Fees.Queries.GetPaymentReceipt;
using Vargshala.Application.Features.OrgAdmin.Fees.Queries.GetStudentFeeDetails;
using Vargshala.Application.Features.OrgAdmin.Fees.Queries.GetStudentFeesPaged;
using Vargshala.Application.Features.OrgAdmin.Fees.Queries.GetStudentsForFeeAssignment;
using Vargshala.Contracts.Common;
using Vargshala.Contracts.Fees;

namespace Vargshala.API.Controllers.OrgAdmin;

[ApiController]
[Route("api/v1/orgadmin/fees")]
[Authorize(Roles = "OrganizationAdmin,1,SuperAdmin,1001")]
public class FeesController : ControllerBase
{
    private readonly IMediator _mediator;

    public FeesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetPaged(
        [FromQuery] PagedRequest request,
        [FromQuery] Guid? branchId = null,
        [FromQuery] Guid? classId = null,
        [FromQuery] Guid? batchId = null,
        [FromQuery] string? status = null)
    {
        var result = await _mediator.Send(new GetStudentFeesPagedQuery(request, branchId, classId, batchId, status));
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    [HttpGet("stats")]
    public async Task<IActionResult> GetStatistics([FromQuery] Guid? branchId = null)
    {
        var result = await _mediator.Send(new GetFeeStatisticsQuery(branchId));
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetDetails(Guid id)
    {
        var result = await _mediator.Send(new GetStudentFeeDetailsQuery(id));
        if (!result.Success)
        {
            return NotFound(result);
        }

        return Ok(result);
    }

    [HttpGet("students-lookup")]
    public async Task<IActionResult> GetStudentsLookup(
        [FromQuery] Guid? branchId = null,
        [FromQuery] Guid? classId = null)
    {
        var result = await _mediator.Send(new GetStudentsForFeeAssignmentQuery(branchId, classId));
        return Ok(result);
    }

    [HttpPost("assign")]
    public async Task<IActionResult> AssignFee([FromBody] AssignStudentFeeRequest request)
    {
        var result = await _mediator.Send(new AssignStudentFeeCommand(request));
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    [HttpPost("collect")]
    public async Task<IActionResult> CollectFee([FromBody] CollectPaymentRequest request)
    {
        var result = await _mediator.Send(new CollectPaymentCommand(request));
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    [HttpGet("payments/{paymentId:guid}/receipt")]
    public async Task<IActionResult> GetReceipt(Guid paymentId)
    {
        var result = await _mediator.Send(new GetPaymentReceiptQuery(paymentId));
        if (!result.Success)
        {
            return NotFound(result);
        }

        return Ok(result);
    }
}
