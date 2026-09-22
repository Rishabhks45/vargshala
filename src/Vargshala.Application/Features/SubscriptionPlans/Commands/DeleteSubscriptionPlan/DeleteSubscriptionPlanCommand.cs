using MediatR;
using Vargshala.Application.Abstractions.CurrentUser;
using Vargshala.Application.Features.SubscriptionPlans.Infrastructure;
using Vargshala.Contracts.Common;

namespace Vargshala.Application.Features.SubscriptionPlans.Commands.DeleteSubscriptionPlan;

public record DeleteSubscriptionPlanCommand(Guid Id) : IRequest<ApiResponse<bool>>;

public class DeleteSubscriptionPlanCommandHandler : IRequestHandler<DeleteSubscriptionPlanCommand, ApiResponse<bool>>
{
    private readonly ISubscriptionPlanRepository _repository;
    private readonly ICurrentUser _currentUser;

    public DeleteSubscriptionPlanCommandHandler(
        ISubscriptionPlanRepository repository,
        ICurrentUser currentUser)
    {
        _repository = repository;
        _currentUser = currentUser;
    }

    public async Task<ApiResponse<bool>> Handle(
        DeleteSubscriptionPlanCommand command,
        CancellationToken cancellationToken)
    {
        var plan = await _repository.GetByIdForUpdateAsync(command.Id, cancellationToken);
        if (plan == null)
        {
            return ApiResponse<bool>.FailureResponse("Subscription plan not found.");
        }

        // Soft delete
        plan.IsDeleted = true;
        plan.DeletedAt = DateTime.UtcNow;
        plan.DeletedBy = _currentUser.UserId;

        _repository.Update(plan);
        await _repository.SaveChangesAsync(cancellationToken);

        return ApiResponse<bool>.SuccessResponse(true, "Subscription plan deleted successfully.");
    }
}
