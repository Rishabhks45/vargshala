using FluentValidation;
using MediatR;
using Vargshala.Contracts.Common;
using Vargshala.Contracts.Messages;
using Vargshala.SharedKernel.Enums;

namespace Vargshala.Application.Features.Messages.Commands.SendMessage;

public record SendMessageCommand(SendMessageRequest Request) 
    : IRequest<ApiResponse<ChatMessageDto>>;

public class SendMessageCommandValidator : AbstractValidator<SendMessageCommand>
{
    public SendMessageCommandValidator()
    {
        ClassLevelCascadeMode = CascadeMode.Stop;
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(x => x.Request)
            .NotNull().WithMessage("Message request payload is required.");

        When(x => x.Request != null, () =>
        {
            RuleFor(x => x.Request.ConversationId)
                .NotEmpty().WithMessage("Conversation ID is required.")
                .NotEqual(Guid.Empty).WithMessage("Valid conversation ID is required.");

            RuleFor(x => x.Request.MessageType)
                .IsInEnum().WithMessage("A valid message type is required.");

            RuleFor(x => x.Request)
                .Must(r => !string.IsNullOrWhiteSpace(r.MessageText) || (r.Attachments != null && r.Attachments.Any()))
                .WithMessage("Message must contain either text or an attachment.");
        });
    }
}
