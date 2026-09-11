using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vargshala.Application.Abstractions.CurrentUser;
using Vargshala.Application.Abstractions.Persistence;
using Vargshala.Application.Features.BranchAdmin.Dashboard.Queries;

namespace Vargshala.API.Controllers.BranchAdmin;

[Route("api/v1/branchadmin/dashboard")]
[Authorize(Roles = "BranchAdmin,4")]
public class DashboardController : BaseBranchAdminController
{
    public DashboardController(
        IMediator mediator,
        ICurrentUser currentUser,
        IVargshalaDbContext db)
        : base(mediator, currentUser, db)
    {
    }

    [HttpGet]
    public async Task<IActionResult> GetDashboard(CancellationToken cancellationToken)
    {
        var (isValid, branchId, errorResult) = await ValidateBranchAccessAsync(cancellationToken);
        if (!isValid) return errorResult!;

        var result = await Mediator.Send(new GetBranchDashboardQuery(branchId), cancellationToken);
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }
}
