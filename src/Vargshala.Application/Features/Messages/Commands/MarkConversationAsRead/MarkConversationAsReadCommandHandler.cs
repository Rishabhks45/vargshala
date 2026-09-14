using MediatR;
using Vargshala.Application.Abstractions.CurrentUser;
using Vargshala.Application.Features.Messages.Infrastructure;
using Vargshala.Contracts.Common;

namespace Vargshala.Application.Features.Messages.Commands.MarkConversationAsRead;

public class MarkConversationAsReadCommandHandler 
    : IRequestHandler<MarkConversationAsReadCommand, ApiResponse<bool>>
{
    private readonly IMessageRepository _messageRepository;
    private readonly ICurrentUser _currentUser;
    private readonly IChatNotificationService _chatNotificationService;

    public MarkConversationAsReadCommandHandler(
        IMessageRepository messageRepository,
        ICurrentUser currentUser,
        IChatNotificationService chatNotificationService)
    {
        _messageRepository = messageRepository;
        _currentUser = currentUser;
        _chatNotificationService = chatNotificationService;
    }

    public async Task<ApiResponse<bool>> Handle(
        MarkConversationAsReadCommand command, 
        CancellationToken cancellationToken)
    {
        if (_currentUser.UserId == Guid.Empty)
        {
            return ApiResponse<bool>.FailureResponse("User identity could not be verified.");
        }

        var currentUserId = _currentUser.UserId;

        await _messageRepository.MarkMessagesAsReadAsync(
            command.ConversationId, 
            currentUserId, 
            command.LatestMessageId, 
            cancellationToken);

        await _messageRepository.SaveChangesAsync(cancellationToken);

        // Real-time broadcast that messages up to latestMessageId have been read
        await _chatNotificationService.NotifyMessagesReadAsync(
            command.ConversationId, 
            currentUserId, 
            command.LatestMessageId, 
            cancellationToken);

        return ApiResponse<bool>.SuccessResponse(true, "Conversation marked as read.");
    }
}
