using Vargshala.Contracts.Common;
using Vargshala.SharedKernel.Enums;
using Vargshala.Domain.Entities;

namespace Vargshala.Application.Features.Messages.Infrastructure;

#region Interface
public interface IMessageRepository
{
    // Conversation Queries
    Task<Conversation?> GetConversationByIdAsync(Guid conversationId, CancellationToken cancellationToken = default);
    Task<Conversation?> GetConversationWithParticipantsAsync(Guid conversationId, CancellationToken cancellationToken = default);
    Task<Conversation?> FindDirectConversationAsync(Guid organizationId, Guid user1Id, Guid user2Id, CancellationToken cancellationToken = default);
    Task<bool> ConversationExistsAsync(Guid conversationId, Guid organizationId, CancellationToken cancellationToken = default);
    Task<(List<Conversation> Items, int TotalRecords)> GetUserConversationsPagedAsync(
        Guid organizationId, 
        Guid userId, 
        PagedRequest request, 
        ConversationType? type = null, 
        CancellationToken cancellationToken = default);

    // Conversation Commands
    Task AddConversationAsync(Conversation conversation, CancellationToken cancellationToken = default);
    void UpdateConversation(Conversation conversation);
    void DeleteConversation(Conversation conversation, Guid deletedBy);

    // Participant Operations
    Task<ConversationParticipant?> GetParticipantAsync(Guid conversationId, Guid userId, CancellationToken cancellationToken = default);
    Task<List<ConversationParticipant>> GetParticipantsAsync(Guid conversationId, CancellationToken cancellationToken = default);
    Task<bool> IsParticipantAsync(Guid conversationId, Guid userId, CancellationToken cancellationToken = default);
    Task<bool> IsAdminAsync(Guid conversationId, Guid userId, CancellationToken cancellationToken = default);
    Task AddParticipantAsync(ConversationParticipant participant, CancellationToken cancellationToken = default);
    Task AddParticipantsRangeAsync(IEnumerable<ConversationParticipant> participants, CancellationToken cancellationToken = default);
    void UpdateParticipant(ConversationParticipant participant);

    // Message Operations
    Task<Message?> GetMessageByIdAsync(Guid messageId, CancellationToken cancellationToken = default);
    Task<Message?> GetMessageWithAttachmentsAsync(Guid messageId, CancellationToken cancellationToken = default);
    Task<(List<Message> Items, int TotalRecords)> GetMessagesPagedAsync(
        Guid conversationId, 
        PagedRequest request, 
        CancellationToken cancellationToken = default);
    Task AddMessageAsync(Message message, CancellationToken cancellationToken = default);
    void UpdateMessage(Message message);
    void SoftDeleteMessage(Message message, Guid deletedBy);

    // Read Tracking
    Task MarkMessagesAsReadAsync(Guid conversationId, Guid userId, Guid latestMessageId, CancellationToken cancellationToken = default);
    Task<int> GetUnreadCountAsync(Guid conversationId, Guid userId, CancellationToken cancellationToken = default);

    // Posting Permissions
    Task<bool> CanUserPostAsync(Guid conversationId, Guid userId, UserRole userRole, CancellationToken cancellationToken = default);

    // Persistence
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
#endregion
