using FluentValidation;
using MediatR;
using Vargshala.Application.Abstractions.CurrentUser;
using Vargshala.Application.Features.OrganizationSubscriptions.Infrastructure;
using Vargshala.Contracts.Common;
using Vargshala.Contracts.Subscriptions;

namespace Vargshala.Application.Features.OrganizationSubscriptions.Commands.ChangeSubscriptionPlan;

public record ChangeSubscriptionPlanCommand(ChangeSubscriptionPlanRequest Request)
    : IRequest<ApiResponse<OrganizationSubscriptionDto>>;

public class ChangeSubscriptionPlanCommandValidator : AbstractValidator<ChangeSubscriptionPlanCommand>
{
    public ChangeSubscriptionPlanCommandValidator()
    {
        ClassLevelCascadeMode = CascadeMode.Stop;
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(x => x.Request).NotNull().WithMessage("Change subscription plan request is required.");
        RuleFor(x => x.Request.TargetPlanId).NotEmpty().WithMessage("Target subscription plan must be specified.");
    }
}

public class ChangeSubscriptionPlanCommandHandler
    : IRequestHandler<ChangeSubscriptionPlanCommand, ApiResponse<OrganizationSubscriptionDto>>
{
    private readonly IOrganizationSubscriptionRepository _repository;
    private readonly ICurrentUser _currentUser;

    public ChangeSubscriptionPlanCommandHandler(
        IOrganizationSubscriptionRepository repository,
        ICurrentUser currentUser)
    {
        _repository = repository;
        _currentUser = currentUser;
    }

    public async Task<ApiResponse<OrganizationSubscriptionDto>> Handle(
        ChangeSubscriptionPlanCommand command,
        CancellationToken cancellationToken)
    {
        if (!_currentUser.OrganizationId.HasValue)
        {
            return ApiResponse<OrganizationSubscriptionDto>.FailureResponse("User does not belong to an organization.");
        }

        var req = command.Request;
        var targetPlan = await _repository.GetPlanByIdAsync(req.TargetPlanId, cancellationToken);
        if (targetPlan == null || !targetPlan.IsActive)
        {
            return ApiResponse<OrganizationSubscriptionDto>.FailureResponse("Selected target subscription plan was not found or is inactive.");
        }

        var currentSub = await _repository.GetCurrentActiveSubscriptionAsync(_currentUser.OrganizationId.Value, cancellationToken);
        if (currentSub != null && currentSub.PlanId == targetPlan.Id && currentSub.Status == SharedKernel.Enums.SubscriptionStatus.Active)
        {
            return ApiResponse<OrganizationSubscriptionDto>.FailureResponse("Your organization is already on this active plan.");
        }

        var newSub = await _repository.ChangeSubscriptionPlanDirectAsync(
            _currentUser.OrganizationId.Value,
            targetPlan.Id,
            req.Remarks,
            cancellationToken);

        var dto = new OrganizationSubscriptionDto
        {
            Id = newSub.Id,
            OrganizationId = newSub.OrganizationId,
            OrganizationName = newSub.Organization?.Name ?? string.Empty,
            PlanId = newSub.PlanId,
            PlanName = targetPlan.Name,
            PlanPrice = targetPlan.Price,
            BillingCycle = targetPlan.BillingCycle,
            MaxStudents = targetPlan.MaxStudents,
            MaxTeachers = targetPlan.MaxTeachers,
            MaxBranches = targetPlan.MaxBranches,
            StartDate = newSub.StartDate,
            EndDate = newSub.EndDate,
            Status = newSub.Status,
            AutoRenew = newSub.AutoRenew,
            CreatedAt = newSub.CreatedAt
        };

        return ApiResponse<OrganizationSubscriptionDto>.SuccessResponse(dto, $"Successfully changed plan to {targetPlan.Name}!");
    }
}
