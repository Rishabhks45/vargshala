using Vargshala.Contracts.Messages.Enums;

namespace Vargshala.Application.Features.Messages.Security;

/// <summary>
/// Mandatory Communication Authorization Matrix Service (Source of Truth).
/// Centralizes all messaging, group creation, administration, and posting authorization decisions.
/// </summary>
public interface IConversationAuthorizationService
{
    /// <summary>
    /// Validates whether the authenticated caller can initiate or participate in direct 1-on-1 messaging with the target user.
    /// </summary>
    Task<bool> CanMessageUserAsync(Guid currentUserId, Guid targetUserId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates whether the authenticated caller can create a group with the requested members, type, batch, and branch.
    /// </summary>
    Task<bool> CanCreateGroupAsync(
        Guid currentUserId, 
        IEnumerable<Guid> memberUserIds, 
        ConversationType groupType, 
        Guid? batchId, 
        Guid? branchId, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates whether the authenticated caller can add a participant to an existing conversation.
    /// Requires caller to be an active Group Admin, and the target to satisfy group eligibility.
    /// </summary>
    Task<bool> CanAddParticipantAsync(Guid currentUserId, Guid conversationId, Guid targetUserId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates whether the authenticated caller can remove a participant from an existing conversation.
    /// Requires caller to be an active Group Admin.
    /// </summary>
    Task<bool> CanRemoveParticipantAsync(Guid currentUserId, Guid conversationId, Guid targetUserId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates whether the authenticated caller can promote another active member to Group Admin.
    /// Requires caller to be an active Group Admin.
    /// </summary>
    Task<bool> CanPromoteAdminAsync(Guid currentUserId, Guid conversationId, Guid targetUserId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates whether the authenticated caller can update the conversation's display picture/photo.
    /// Requires caller to be an active Group Admin.
    /// </summary>
    Task<bool> CanChangeGroupPhotoAsync(Guid currentUserId, Guid conversationId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates whether the caller can send a message in the given conversation.
    /// </summary>
    Task<bool> CanSendMessageAsync(Guid currentUserId, Guid conversationId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates whether the caller can reply to an announcement channel.
    /// </summary>
    Task<bool> CanReplyToAnnouncementAsync(Guid currentUserId, Guid conversationId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves all user IDs that the caller is authorized to directly communicate with under the matrix rules.
    /// Used for contact search and user discovery without leaking unauthorized users.
    /// </summary>
    Task<List<Guid>> GetEligibleDirectMessageRecipientUserIdsAsync(Guid currentUserId, CancellationToken cancellationToken = default);
}
