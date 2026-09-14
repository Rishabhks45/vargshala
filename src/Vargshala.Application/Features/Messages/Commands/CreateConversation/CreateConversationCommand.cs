using FluentValidation;
using MediatR;
using Vargshala.Contracts.Common;
using Vargshala.Contracts.Messages;
using Vargshala.Contracts.Messages.Enums;

namespace Vargshala.Application.Features.Messages.Commands.CreateConversation;

public record CreateConversationCommand(CreateConversationRequest Request) 
    : IRequest<ApiResponse<ChatConversationDto>>;

public class CreateConversationCommandValidator : AbstractValidator<CreateConversationCommand>
{
    public CreateConversationCommandValidator()
    {
        ClassLevelCascadeMode = CascadeMode.Stop;
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(x => x.Request)
            .NotNull().WithMessage("Conversation request payload is required.");

        When(x => x.Request != null, () =>
        {
            RuleFor(x => x.Request.Type)
                .IsInEnum().WithMessage("A valid conversation type is required.");

            When(x => x.Request.Type == ConversationType.Direct, () =>
            {
                RuleFor(x => x.Request.TargetUserId)
                    .NotEmpty().WithMessage("Target user ID is required for a direct conversation.")
                    .NotEqual(Guid.Empty).WithMessage("Valid target user ID is required.");
            });

            When(x => x.Request.Type != ConversationType.Direct, () =>
            {
                RuleFor(x => x.Request.Name)
                    .NotEmpty().WithMessage("Group name is required.")
                    .MaximumLength(200).WithMessage("Group name cannot exceed 200 characters.");
            });

            RuleFor(x => x.Request.WhoCanReply)
                .IsInEnum().WithMessage("A valid WhoCanReply policy is required.");
        });
    }
}
