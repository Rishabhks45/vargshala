using MediatR;
using Vargshala.Application.Abstractions.CurrentUser;
using Vargshala.Application.Features.Messages.Common;
using Vargshala.Application.Features.Messages.Infrastructure;
using Vargshala.Contracts.Common;
using Vargshala.Contracts.Messages;
using Vargshala.SharedKernel.Enums;

namespace Vargshala.Application.Features.Messages.Commands.RestoreMessage;

public class RestoreMessageCommandHandler 
    : IRequestHandler<RestoreMessageCommand, ApiResponse<ChatMessageDto>>
{
    private readonly IMessageRepository _messageRepository;
    private readonly ICurrentUser _currentUser;
    private readonly IChatNotificationService _chatNotificationService;

    public RestoreMessageCommandHandler(
        IMessageRepository messageRepository,
        ICurrentUser currentUser,
        IChatNotificationService chatNotificationService)
    {
        _messageRepository = messageRepository;
        _currentUser = currentUser;
        _chatNotificationService = chatNotificationService;
    }

    public async Task<ApiResponse<ChatMessageDto>> Handle(
        RestoreMessageCommand command, 
        CancellationToken cancellationToken)
    {
        if (_currentUser.OrganizationId is null || _currentUser.UserId == Guid.Empty)
        {
            return ApiResponse<ChatMessageDto>.FailureResponse("User identity could not be verified.");
        }

        var orgId = _currentUser.OrganizationId.Value;
        var userId = _currentUser.UserId;
        var role = _currentUser.UserRole ?? UserRole.Student;

        var message = await _messageRepository.GetMessageForRestoreAsync(command.MessageId, cancellationToken);
        if (message == null || message.OrganizationId != orgId || !message.IsDeleted)
        {
            return ApiResponse<ChatMessageDto>.FailureResponse("Message not found or not deleted.");
        }

        // Check permission: original author, person who deleted it, or admin
        var isAuthor = message.SenderId == userId;
        var isDeleter = message.DeletedBy == userId;
        var isSystemAdmin = role == UserRole.SuperAdmin || role == UserRole.OrganizationAdmin;
        var isConversationAdmin = false;
        if (!isAuthor && !isDeleter && !isSystemAdmin)
        {
            isConversationAdmin = await _messageRepository.IsAdminAsync(message.ConversationId, userId, cancellationToken);
        }

        if (!isAuthor && !isDeleter && !isSystemAdmin && !isConversationAdmin)
        {
            return ApiResponse<ChatMessageDto>.FailureResponse("You do not have permission to restore this message.");
        }

        _messageRepository.RestoreMessage(message);

        // Update conversation last message cache if restored message is newer than current last message
        var conversation = await _messageRepository.GetConversationForUpdateAsync(message.ConversationId, orgId, cancellationToken);
        if (conversation != null && (conversation.LastMessageAt == null || message.SentAt >= conversation.LastMessageAt))
        {
            var text = !string.IsNullOrWhiteSpace(message.MessageText)
                ? (message.MessageText.Length > 150 ? message.MessageText[..147] + "..." : message.MessageText)
                : string.Empty;

            conversation.LastMessageId = message.Id;
            conversation.LastMessageAt = message.SentAt;
            conversation.LastMessageText = text;
            conversation.LastMessageSenderId = message.SenderId;
            _messageRepository.UpdateConversation(conversation);
        }

        await _messageRepository.SaveChangesAsync(cancellationToken);

        var fullMessage = await _messageRepository.GetMessageByIdAsync(message.Id, cancellationToken);
        var dto = (fullMessage ?? message).ToDto(userId);

        // Broadcast real-time restoration
        await _chatNotificationService.NotifyMessageRestoredAsync(message.ConversationId, dto, cancellationToken);

        return ApiResponse<ChatMessageDto>.SuccessResponse(dto, "Message restored successfully.");
    }
}
