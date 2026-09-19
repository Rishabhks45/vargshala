using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vargshala.Application.Abstractions.Security;
using Vargshala.Contracts.Common;

namespace Vargshala.API.Controllers.BranchAdmin;

[ApiController]
[Authorize(Roles = "BranchAdmin,4")]
public abstract class BaseBranchAdminController : ControllerBase
{
    protected readonly IMediator Mediator;
    protected readonly IBranchAuthorizationService BranchAuthService;

    protected BaseBranchAdminController(
        IMediator mediator,
        IBranchAuthorizationService branchAuthService)
    {
        Mediator = mediator;
        BranchAuthService = branchAuthService;
    }

    protected async Task<(bool IsValid, Guid BranchId, IActionResult? ErrorResult)> ValidateBranchAccessAsync(CancellationToken cancellationToken = default)
    {
        var result = await BranchAuthService.ValidateBranchAccessAsync(cancellationToken);

        if (!result.IsValid)
        {
            var response = ApiResponse<string>.FailureResponse(result.ErrorMessage ?? "Access Denied.");
            var actionResult = result.StatusCode switch
            {
                StatusCodes.Status401Unauthorized => (IActionResult)Unauthorized(response),
                StatusCodes.Status403Forbidden => StatusCode(StatusCodes.Status403Forbidden, response),
                StatusCodes.Status409Conflict => StatusCode(StatusCodes.Status409Conflict, response),
                _ => StatusCode(result.StatusCode, response)
            };

            return (false, Guid.Empty, actionResult);
        }

        return (true, result.BranchId, null);
    }
}
