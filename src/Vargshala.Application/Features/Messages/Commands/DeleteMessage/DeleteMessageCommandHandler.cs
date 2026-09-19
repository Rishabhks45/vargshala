using MediatR;
using Vargshala.Application.Abstractions.CurrentUser;
using Vargshala.Application.Features.Messages.Infrastructure;
using Vargshala.Contracts.Common;
using Vargshala.SharedKernel.Enums;

namespace Vargshala.Application.Features.Messages.Commands.DeleteMessage;

public class DeleteMessageCommandHandler 
    : IRequestHandler<DeleteMessageCommand, ApiResponse<bool>>
{
    private readonly IMessageRepository _messageRepository;
    private readonly ICurrentUser _currentUser;
    private readonly IChatNotificationService _chatNotificationService;

    public DeleteMessageCommandHandler(
        IMessageRepository messageRepository,
        ICurrentUser currentUser,
        IChatNotificationService chatNotificationService)
    {
        _messageRepository = messageRepository;
        _currentUser = currentUser;
        _chatNotificationService = chatNotificationService;
    }

    public async Task<ApiResponse<bool>> Handle(
        DeleteMessageCommand command, 
        CancellationToken cancellationToken)
    {
        if (_currentUser.OrganizationId is null || _currentUser.UserId == Guid.Empty)
        {
            return ApiResponse<bool>.FailureResponse("User identity could not be verified.");
        }

        var orgId = _currentUser.OrganizationId.Value;
        var userId = _currentUser.UserId;
        var role = _currentUser.UserRole ?? UserRole.Student;

        var message = await _messageRepository.GetMessageForUpdateAsync(command.MessageId, cancellationToken);
        if (message == null || message.OrganizationId != orgId)
        {
            return ApiResponse<bool>.FailureResponse("Message not found.");
        }

        // Check permission: author can delete, admins can delete
        var isAuthor = message.SenderId == userId;
        var isSystemAdmin = role == UserRole.SuperAdmin || role == UserRole.OrganizationAdmin;
        var isConversationAdmin = false;
        if (!isAuthor && !isSystemAdmin)
        {
            isConversationAdmin = await _messageRepository.IsAdminAsync(message.ConversationId, userId, cancellationToken);
        }

        if (!isAuthor && !isSystemAdmin && !isConversationAdmin)
        {
            return ApiResponse<bool>.FailureResponse("You do not have permission to delete this message.");
        }

        _messageRepository.SoftDeleteMessage(message, userId);

        // Update conversation last message cache if deleted message was the latest one
        var conversation = await _messageRepository.GetConversationForUpdateAsync(message.ConversationId, orgId, cancellationToken);
        if (conversation != null && conversation.LastMessageId == message.Id)
        {
            var previous = await _messageRepository.GetLatestActiveMessageAsync(message.ConversationId, cancellationToken);
            if (previous != null)
            {
                var text = !string.IsNullOrWhiteSpace(previous.MessageText)
                    ? (previous.MessageText.Length > 150 ? previous.MessageText[..147] + "..." : previous.MessageText)
                    : (previous.Attachments.Any(a => !a.IsDeleted) ? "📎 Attachment" : string.Empty);

                conversation.LastMessageId = previous.Id;
                conversation.LastMessageAt = previous.SentAt;
                conversation.LastMessageText = text;
                conversation.LastMessageSenderId = previous.SenderId;
            }
            else
            {
                conversation.LastMessageId = null;
                conversation.LastMessageAt = null;
                conversation.LastMessageText = string.Empty;
                conversation.LastMessageSenderId = null;
            }

            _messageRepository.UpdateConversation(conversation);
        }

        await _messageRepository.SaveChangesAsync(cancellationToken);

        // Broadcast real-time deletion
        await _chatNotificationService.NotifyMessageDeletedAsync(message.ConversationId, message.Id, cancellationToken);

        return ApiResponse<bool>.SuccessResponse(true, "Message deleted successfully.");
    }
}
