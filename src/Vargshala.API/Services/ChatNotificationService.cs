using Microsoft.AspNetCore.SignalR;
using Vargshala.API.Hubs;
using Vargshala.Application.Features.Messages.Infrastructure;
using Vargshala.Contracts.Messages;

namespace Vargshala.API.Services;

public class ChatNotificationService : IChatNotificationService
{
    private readonly IHubContext<ChatHub> _hubContext;

    public ChatNotificationService(IHubContext<ChatHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task NotifyMessageReceivedAsync(
        Guid conversationId, 
        ChatMessageDto message, 
        IEnumerable<Guid> participantUserIds, 
        CancellationToken cancellationToken = default)
    {
        // Broadcast to active conversation stream
        await _hubContext.Clients.Group($"conv_{conversationId}")
            .SendAsync("ReceiveMessage", message, cancellationToken);

        // Also broadcast to individual participant user channels (updates conversation list & unread badge)
        if (participantUserIds != null)
        {
            foreach (var userId in participantUserIds)
            {
                await _hubContext.Clients.Group($"user_{userId}")
                    .SendAsync("ReceiveMessage", message, cancellationToken);
            }
        }
    }

    public async Task NotifyMessagesReadAsync(
        Guid conversationId, 
        Guid readByUserId, 
        Guid latestMessageId, 
        CancellationToken cancellationToken = default)
    {
        var notification = new MessagesReadNotification
        {
            ConversationId = conversationId,
            ReadByUserId = readByUserId,
            LatestMessageId = latestMessageId,
            ReadAt = DateTime.UtcNow
        };

        // Broadcast to everyone in the conversation
        await _hubContext.Clients.Group($"conv_{conversationId}")
            .SendAsync("MessagesRead", notification, cancellationToken);
    }

    public async Task NotifyConversationUpdatedAsync(
        Guid conversationId, 
        ChatConversationDto conversation, 
        CancellationToken cancellationToken = default)
    {
        await _hubContext.Clients.Group($"conv_{conversationId}")
            .SendAsync("ConversationUpdated", conversation, cancellationToken);
    }

    public async Task NotifyMessageDeletedAsync(
        Guid conversationId, 
        Guid messageId, 
        CancellationToken cancellationToken = default)
    {
        await _hubContext.Clients.Group($"conv_{conversationId}")
            .SendAsync("MessageDeleted", new MessageDeletedNotification { ConversationId = conversationId, MessageId = messageId }, cancellationToken);
    }

    public async Task NotifyMessageRestoredAsync(
        Guid conversationId, 
        ChatMessageDto message, 
        CancellationToken cancellationToken = default)
    {
        await _hubContext.Clients.Group($"conv_{conversationId}")
            .SendAsync("MessageRestored", message, cancellationToken);
    }
}
