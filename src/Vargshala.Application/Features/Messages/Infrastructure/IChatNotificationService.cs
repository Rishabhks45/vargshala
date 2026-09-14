using Vargshala.Contracts.Messages;

namespace Vargshala.Application.Features.Messages.Infrastructure;

public interface IChatNotificationService
{
    Task NotifyMessageReceivedAsync(
        Guid conversationId, 
        ChatMessageDto message, 
        IEnumerable<Guid> participantUserIds, 
        CancellationToken cancellationToken = default);

    Task NotifyMessagesReadAsync(
        Guid conversationId, 
        Guid readByUserId, 
        Guid latestMessageId, 
        CancellationToken cancellationToken = default);

    Task NotifyConversationUpdatedAsync(
        Guid conversationId, 
        ChatConversationDto conversation, 
        CancellationToken cancellationToken = default);
}
