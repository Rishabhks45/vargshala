using FluentValidation;
using MediatR;
using Vargshala.Application.Abstractions.CurrentUser;
using Vargshala.Application.Features.OrganizationSubscriptions.Infrastructure;
using Vargshala.Contracts.Common;
using Vargshala.Contracts.Subscriptions;

namespace Vargshala.Application.Features.OrganizationSubscriptions.Commands.CancelSubscription;

public record CancelSubscriptionCommand(CancelSubscriptionRequest? Request = null)
    : IRequest<ApiResponse<bool>>;

public class CancelSubscriptionCommandValidator : AbstractValidator<CancelSubscriptionCommand>
{
    public CancelSubscriptionCommandValidator()
    {
        ClassLevelCascadeMode = CascadeMode.Stop;
        RuleLevelCascadeMode = CascadeMode.Stop;

        When(x => x.Request != null && !string.IsNullOrWhiteSpace(x.Request.Reason), () =>
        {
            RuleFor(x => x.Request!.Reason)
                .MaximumLength(500)
                .WithMessage("Cancellation reason cannot exceed 500 characters.");
        });
    }
}

public class CancelSubscriptionCommandHandler : IRequestHandler<CancelSubscriptionCommand, ApiResponse<bool>>
{
    private readonly IOrganizationSubscriptionRepository _repository;
    private readonly ICurrentUser _currentUser;

    public CancelSubscriptionCommandHandler(
        IOrganizationSubscriptionRepository repository,
        ICurrentUser currentUser)
    {
        _repository = repository;
        _currentUser = currentUser;
    }

    public async Task<ApiResponse<bool>> Handle(
        CancelSubscriptionCommand command,
        CancellationToken cancellationToken)
    {
        if (!_currentUser.OrganizationId.HasValue)
        {
            return ApiResponse<bool>.FailureResponse("User does not belong to an organization.");
        }

        bool cancelled = await _repository.CancelSubscriptionAsync(
            _currentUser.OrganizationId.Value,
            command.Request?.Reason,
            cancellationToken);

        if (!cancelled)
        {
            return ApiResponse<bool>.FailureResponse("No active subscription found to cancel, or subscription is already cancelled.");
        }

        return ApiResponse<bool>.SuccessResponse(true, "Your SaaS subscription has been cancelled successfully.");
    }
}
