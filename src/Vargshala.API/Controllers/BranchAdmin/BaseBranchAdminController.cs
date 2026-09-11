using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vargshala.Application.Abstractions.CurrentUser;
using Vargshala.Application.Abstractions.Persistence;
using Vargshala.Contracts.Common;

namespace Vargshala.API.Controllers.BranchAdmin;

[ApiController]
[Authorize(Roles = "BranchAdmin,4")]
public abstract class BaseBranchAdminController : ControllerBase
{
    protected readonly IMediator Mediator;
    protected readonly ICurrentUser CurrentUser;
    protected readonly IVargshalaDbContext Db;

    protected BaseBranchAdminController(
        IMediator mediator,
        ICurrentUser currentUser,
        IVargshalaDbContext db)
    {
        Mediator = mediator;
        CurrentUser = currentUser;
        Db = db;
    }

    protected async Task<(bool IsValid, Guid BranchId, IActionResult? ErrorResult)> ValidateBranchAccessAsync(CancellationToken cancellationToken = default)
    {
        var branchId = CurrentUser.BranchId;
        var userId = CurrentUser.UserId;
        var orgId = CurrentUser.OrganizationId;

        if (!branchId.HasValue || branchId.Value == Guid.Empty ||
            userId == Guid.Empty ||
            !orgId.HasValue || orgId.Value == Guid.Empty)
        {
            return (false, Guid.Empty, Unauthorized(ApiResponse<string>.FailureResponse("Unauthorized: Missing tenant or branch identity.")));
        }

        var activeAccesses = await Db.UserBranchAccesses
            .AsNoTracking()
            .Where(a => a.UserId == userId && a.IsActive && !a.Branch.IsDeleted && a.Branch.OrganizationId == orgId.Value)
            .ToListAsync(cancellationToken);

        if (activeAccesses.Count == 0)
        {
            return (false, Guid.Empty, StatusCode(StatusCodes.Status403Forbidden, ApiResponse<string>.FailureResponse("Access Denied: No active branch assignment found for this account. Please contact your Institute Administrator.")));
        }

        if (activeAccesses.Count > 1)
        {
            return (false, Guid.Empty, StatusCode(StatusCodes.Status409Conflict, ApiResponse<string>.FailureResponse("Configuration Conflict: Multiple active branch assignments detected for this Branch Admin account. Exactly one active branch is permitted.")));
        }

        if (activeAccesses[0].BranchId != branchId.Value)
        {
            return (false, Guid.Empty, StatusCode(StatusCodes.Status403Forbidden, ApiResponse<string>.FailureResponse("Access Denied: Token branch identity does not match current active branch assignment.")));
        }

        return (true, branchId.Value, null);
    }
}
