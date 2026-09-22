using MediatR;
using Vargshala.Application.Abstractions.CurrentUser;
using Vargshala.Application.Features.SubscriptionPlans.Infrastructure;
using Vargshala.Contracts.Common;
using Vargshala.Contracts.Subscriptions;
using Vargshala.Domain.Entities;

namespace Vargshala.Application.Features.SubscriptionPlans.Commands.CreateSubscriptionPlan;

public record CreateSubscriptionPlanCommand(CreateSubscriptionPlanRequest Request) : IRequest<ApiResponse<SubscriptionPlanDto>>;

public class CreateSubscriptionPlanCommandHandler : IRequestHandler<CreateSubscriptionPlanCommand, ApiResponse<SubscriptionPlanDto>>
{
    private readonly ISubscriptionPlanRepository _repository;
    private readonly ICurrentUser _currentUser;

    public CreateSubscriptionPlanCommandHandler(
        ISubscriptionPlanRepository repository,
        ICurrentUser currentUser)
    {
        _repository = repository;
        _currentUser = currentUser;
    }

    public async Task<ApiResponse<SubscriptionPlanDto>> Handle(
        CreateSubscriptionPlanCommand command,
        CancellationToken cancellationToken)
    {
        var req = command.Request;
        var nameExists = await _repository.ExistsByNameAsync(req.Name.Trim(), null, cancellationToken);
        if (nameExists)
        {
            return ApiResponse<SubscriptionPlanDto>.FailureResponse($"A plan with the name '{req.Name.Trim()}' already exists.");
        }

        var plan = new SubscriptionPlan
        {
            Id = Guid.NewGuid(),
            Name = req.Name.Trim(),
            Description = req.Description?.Trim(),
            Price = req.Price,
            BillingCycle = req.BillingCycle,
            MaxStudents = req.MaxStudents,
            MaxTeachers = req.MaxTeachers,
            MaxBranches = req.MaxBranches,
            IsActive = req.IsActive,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = _currentUser.UserId
        };

        await _repository.AddAsync(plan, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        return ApiResponse<SubscriptionPlanDto>.SuccessResponse(plan.ToDto(), "Subscription plan created successfully.");
    }
}
