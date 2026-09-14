using FluentValidation;
using MediatR;
using Vargshala.Contracts.Common;

namespace Vargshala.Application.Features.Messages.Commands.MarkConversationAsRead;

public record MarkConversationAsReadCommand(Guid ConversationId, Guid LatestMessageId) 
    : IRequest<ApiResponse<bool>>;

public class MarkConversationAsReadCommandValidator : AbstractValidator<MarkConversationAsReadCommand>
{
    public MarkConversationAsReadCommandValidator()
    {
        ClassLevelCascadeMode = CascadeMode.Stop;
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(x => x.ConversationId)
            .NotEmpty().WithMessage("Conversation ID is required.")
            .NotEqual(Guid.Empty).WithMessage("Valid conversation ID is required.");

        RuleFor(x => x.LatestMessageId)
            .NotEmpty().WithMessage("Latest message ID is required.")
            .NotEqual(Guid.Empty).WithMessage("Valid latest message ID is required.");
    }
}
