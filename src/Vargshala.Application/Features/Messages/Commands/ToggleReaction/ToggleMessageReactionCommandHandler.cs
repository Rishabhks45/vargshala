using MediatR;
using Vargshala.Application.Abstractions.CurrentUser;
using Vargshala.Application.Features.Messages.Infrastructure;
using Vargshala.Contracts.Common;
using Vargshala.Contracts.Messages;

namespace Vargshala.Application.Features.Messages.Commands.ToggleReaction;

public class ToggleMessageReactionCommandHandler 
    : IRequestHandler<ToggleMessageReactionCommand, ApiResponse<ToggleReactionResultDto>>
{
    private readonly IMessageRepository _messageRepository;
    private readonly ICurrentUser _currentUser;

    public ToggleMessageReactionCommandHandler(
        IMessageRepository messageRepository,
        ICurrentUser currentUser)
    {
        _messageRepository = messageRepository;
        _currentUser = currentUser;
    }

    public async Task<ApiResponse<ToggleReactionResultDto>> Handle(
        ToggleMessageReactionCommand command, 
        CancellationToken cancellationToken)
    {
        if (_currentUser.OrganizationId is null || _currentUser.UserId == Guid.Empty)
        {
            return ApiResponse<ToggleReactionResultDto>.FailureResponse("User identity could not be verified.");
        }

        if (string.IsNullOrWhiteSpace(command.Emoji))
        {
            return ApiResponse<ToggleReactionResultDto>.FailureResponse("Emoji cannot be empty.");
        }

        var orgId = _currentUser.OrganizationId.Value;
        var userId = _currentUser.UserId;

        var message = await _messageRepository.GetMessageByIdAsync(command.MessageId, cancellationToken);
        if (message == null || message.OrganizationId != orgId)
        {
            return ApiResponse<ToggleReactionResultDto>.FailureResponse("Message not found.");
        }

        var (reactions, activeEmoji) = await _messageRepository.ToggleReactionAsync(
            command.MessageId,
            userId,
            orgId,
            command.Emoji.Trim(),
            cancellationToken);

        var result = new ToggleReactionResultDto
        {
            MessageId = command.MessageId,
            Reactions = reactions,
            ActiveEmoji = activeEmoji
        };

        return ApiResponse<ToggleReactionResultDto>.SuccessResponse(result);
    }
}
