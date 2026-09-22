using MediatR;
using Vargshala.Application.Abstractions.CurrentUser;
using Vargshala.Application.Features.SubscriptionPlans.Infrastructure;
using Vargshala.Contracts.Common;
using Vargshala.Contracts.Subscriptions;

namespace Vargshala.Application.Features.SubscriptionPlans.Commands.ToggleSubscriptionPlanStatus;

public record ToggleSubscriptionPlanStatusCommand(Guid Id) : IRequest<ApiResponse<SubscriptionPlanDto>>;

public class ToggleSubscriptionPlanStatusCommandHandler : IRequestHandler<ToggleSubscriptionPlanStatusCommand, ApiResponse<SubscriptionPlanDto>>
{
    private readonly ISubscriptionPlanRepository _repository;
    private readonly ICurrentUser _currentUser;

    public ToggleSubscriptionPlanStatusCommandHandler(
        ISubscriptionPlanRepository repository,
        ICurrentUser currentUser)
    {
        _repository = repository;
        _currentUser = currentUser;
    }

    public async Task<ApiResponse<SubscriptionPlanDto>> Handle(
        ToggleSubscriptionPlanStatusCommand command,
        CancellationToken cancellationToken)
    {
        var plan = await _repository.GetByIdForUpdateAsync(command.Id, cancellationToken);
        if (plan == null)
        {
            return ApiResponse<SubscriptionPlanDto>.FailureResponse("Subscription plan not found.");
        }

        plan.IsActive = !plan.IsActive;
        plan.UpdatedAt = DateTime.UtcNow;
        plan.UpdatedBy = _currentUser.UserId;

        _repository.Update(plan);
        await _repository.SaveChangesAsync(cancellationToken);

        var statusMessage = plan.IsActive ? "Subscription plan activated." : "Subscription plan deactivated.";
        return ApiResponse<SubscriptionPlanDto>.SuccessResponse(plan.ToDto(), statusMessage);
    }
}
