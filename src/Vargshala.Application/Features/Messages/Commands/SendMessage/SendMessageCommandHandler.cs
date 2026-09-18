using MediatR;
using Vargshala.Application.Abstractions.CurrentUser;
using Vargshala.Application.Features.Messages.Common;
using Vargshala.Application.Features.Messages.Infrastructure;
using Vargshala.Contracts.Common;
using Vargshala.Contracts.Messages;
using Vargshala.SharedKernel.Enums;
using Vargshala.Domain.Entities;

namespace Vargshala.Application.Features.Messages.Commands.SendMessage;

public class SendMessageCommandHandler : IRequestHandler<SendMessageCommand, ApiResponse<ChatMessageDto>>
{
    private readonly IMessageRepository _messageRepository;
    private readonly ICurrentUser _currentUser;
    private readonly Vargshala.Application.Features.Messages.Security.IConversationAuthorizationService _authService;
    private readonly IChatNotificationService _chatNotificationService;

    public SendMessageCommandHandler(
        IMessageRepository messageRepository,
        ICurrentUser currentUser,
        Vargshala.Application.Features.Messages.Security.IConversationAuthorizationService authService,
        IChatNotificationService chatNotificationService)
    {
        _messageRepository = messageRepository;
        _currentUser = currentUser;
        _authService = authService;
        _chatNotificationService = chatNotificationService;
    }

    public async Task<ApiResponse<ChatMessageDto>> Handle(
        SendMessageCommand command, 
        CancellationToken cancellationToken)
    {
        if (_currentUser.OrganizationId is null)
        {
            return ApiResponse<ChatMessageDto>.FailureResponse("You must belong to an organization to send messages.");
        }

        if (_currentUser.UserId == Guid.Empty)
        {
            return ApiResponse<ChatMessageDto>.FailureResponse("User identity could not be verified.");
        }

        var orgId = _currentUser.OrganizationId.Value;
        var currentUserId = _currentUser.UserId;
        var req = command.Request;

        // Check if conversation exists and belongs to current tenant
        var conversation = await _messageRepository.GetConversationByIdAsync(req.ConversationId, cancellationToken);
        if (conversation == null || conversation.OrganizationId != orgId)
        {
            return ApiResponse<ChatMessageDto>.FailureResponse("Conversation was not found.");
        }

        // Verify posting permissions via centralized authorization service
        var canPost = await _authService.CanSendMessageAsync(currentUserId, conversation.Id, cancellationToken);
        if (!canPost)
        {
            return ApiResponse<ChatMessageDto>.FailureResponse("You do not have permission to post in this conversation.");
        }

        var message = new Message
        {
            Id = Guid.NewGuid(),
            OrganizationId = orgId,
            ConversationId = conversation.Id,
            SenderId = currentUserId,
            ReplyToMessageId = req.ReplyToMessageId,
            MessageType = req.MessageType,
            MessageText = req.MessageText?.Trim(),
            SentAt = DateTime.UtcNow,
            IsPinned = false,
            IsDeleted = false
        };

        if (req.Attachments != null && req.Attachments.Any())
        {
            foreach (var att in req.Attachments)
            {
                message.Attachments.Add(new MessageAttachment
                {
                    Id = Guid.NewGuid(),
                    OrganizationId = orgId,
                    MessageId = message.Id,
                    FileName = att.FileName,
                    ContentType = att.ContentType,
                    FileSize = att.FileSize,
                    FileUrl = att.FileUrl,
                    ThumbnailUrl = att.ThumbnailUrl,
                    StorageKey = att.StorageKey,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = currentUserId,
                    IsDeleted = false
                });
            }
        }

        await _messageRepository.AddMessageAsync(message, cancellationToken);

        // Update conversation denormalized last message cache for blazing fast inbox listing
        var lastText = !string.IsNullOrWhiteSpace(message.MessageText) 
            ? (message.MessageText.Length > 150 ? message.MessageText[..147] + "..." : message.MessageText)
            : (message.Attachments.Any() ? "📎 Attachment" : string.Empty);

        conversation.LastMessageId = message.Id;
        conversation.LastMessageAt = message.SentAt;
        conversation.LastMessageText = lastText;
        conversation.LastMessageSenderId = currentUserId;

        _messageRepository.UpdateConversation(conversation);

        // Save message and conversation cache
        await _messageRepository.SaveChangesAsync(cancellationToken);

        // Mark as read for sender
        await _messageRepository.MarkMessagesAsReadAsync(conversation.Id, currentUserId, message.Id, cancellationToken);
        await _messageRepository.SaveChangesAsync(cancellationToken);

        // Fetch fresh message with sender details
        var savedMessage = await _messageRepository.GetMessageByIdAsync(message.Id, cancellationToken);
        var dto = (savedMessage ?? message).ToDto(currentUserId);

        // Real-time broadcast to conversation and participant channels
        var participantIds = conversation.Participants
            .Where(p => !p.IsDeleted && p.IsActive)
            .Select(p => p.UserId)
            .ToList();

        await _chatNotificationService.NotifyMessageReceivedAsync(
            conversation.Id, 
            dto, 
            participantIds, 
            cancellationToken);

        return ApiResponse<ChatMessageDto>.SuccessResponse(dto, "Message sent successfully.");
    }
}
