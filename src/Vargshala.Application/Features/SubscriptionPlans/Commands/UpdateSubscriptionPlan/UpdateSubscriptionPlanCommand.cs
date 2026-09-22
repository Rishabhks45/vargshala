using MediatR;
using Vargshala.Application.Abstractions.CurrentUser;
using Vargshala.Application.Features.SubscriptionPlans.Infrastructure;
using Vargshala.Contracts.Common;
using Vargshala.Contracts.Subscriptions;

namespace Vargshala.Application.Features.SubscriptionPlans.Commands.UpdateSubscriptionPlan;

public record UpdateSubscriptionPlanCommand(UpdateSubscriptionPlanRequest Request) : IRequest<ApiResponse<SubscriptionPlanDto>>;

public class UpdateSubscriptionPlanCommandHandler : IRequestHandler<UpdateSubscriptionPlanCommand, ApiResponse<SubscriptionPlanDto>>
{
    private readonly ISubscriptionPlanRepository _repository;
    private readonly ICurrentUser _currentUser;

    public UpdateSubscriptionPlanCommandHandler(
        ISubscriptionPlanRepository repository,
        ICurrentUser currentUser)
    {
        _repository = repository;
        _currentUser = currentUser;
    }

    public async Task<ApiResponse<SubscriptionPlanDto>> Handle(
        UpdateSubscriptionPlanCommand command,
        CancellationToken cancellationToken)
    {
        var req = command.Request;
        var plan = await _repository.GetByIdForUpdateAsync(req.Id, cancellationToken);
        if (plan == null)
        {
            return ApiResponse<SubscriptionPlanDto>.FailureResponse("Subscription plan not found.");
        }

        var nameExists = await _repository.ExistsByNameAsync(req.Name.Trim(), plan.Id, cancellationToken);
        if (nameExists)
        {
            return ApiResponse<SubscriptionPlanDto>.FailureResponse($"Another plan with the name '{req.Name.Trim()}' already exists.");
        }

        plan.Name = req.Name.Trim();
        plan.Description = req.Description?.Trim();
        plan.Price = req.Price;
        plan.BillingCycle = req.BillingCycle;
        plan.MaxStudents = req.MaxStudents;
        plan.MaxTeachers = req.MaxTeachers;
        plan.MaxBranches = req.MaxBranches;
        plan.IsActive = req.IsActive;
        plan.UpdatedAt = DateTime.UtcNow;
        plan.UpdatedBy = _currentUser.UserId;

        _repository.Update(plan);
        await _repository.SaveChangesAsync(cancellationToken);

        return ApiResponse<SubscriptionPlanDto>.SuccessResponse(plan.ToDto(), "Subscription plan updated successfully.");
    }
}
