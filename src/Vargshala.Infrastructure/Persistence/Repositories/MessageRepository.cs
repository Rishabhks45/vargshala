using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Vargshala.Application.Abstractions.Persistence;
using Vargshala.Application.Common;
using Vargshala.Application.Features.Messages.Infrastructure;
using Vargshala.Contracts.Common;
using Vargshala.SharedKernel.Enums;
using Vargshala.Domain.Entities;

namespace Vargshala.Infrastructure.Persistence.Repositories;

public class MessageRepository : IMessageRepository
{
    #region Fields & Constructor
    private readonly IVargshalaDbContext _db;

    public MessageRepository(IVargshalaDbContext db)
    {
        _db = db;
    }
    #endregion

    #region Search & Sort Mappings
    private static Func<string, Expression<Func<Conversation, bool>>> ConversationSearchPredicate => term =>
    {
        var lowerTerm = $"%{term.ToLower()}%";
        return c => (c.Name != null && EF.Functions.Like(c.Name.ToLower(), lowerTerm))
                 || (c.Description != null && EF.Functions.Like(c.Description.ToLower(), lowerTerm))
                 || (c.DirectUser1 != null && (EF.Functions.Like(c.DirectUser1.FirstName.ToLower(), lowerTerm) || EF.Functions.Like(c.DirectUser1.LastName.ToLower(), lowerTerm)))
                 || (c.DirectUser2 != null && (EF.Functions.Like(c.DirectUser2.FirstName.ToLower(), lowerTerm) || EF.Functions.Like(c.DirectUser2.LastName.ToLower(), lowerTerm)));
    };

    private static readonly Dictionary<string, Expression<Func<Conversation, object>>> ConversationSortMappings = new(StringComparer.OrdinalIgnoreCase)
    {
        ["lastmessageat"] = c => c.LastMessageAt ?? c.CreatedAt,
        ["createdat"] = c => c.CreatedAt,
        ["name"] = c => c.Name ?? string.Empty,
        ["type"] = c => c.Type
    };

    private static readonly Dictionary<string, Expression<Func<Message, object>>> MessageSortMappings = new(StringComparer.OrdinalIgnoreCase)
    {
        ["sentat"] = m => m.SentAt,
        ["createdat"] = m => m.SentAt,
        ["ispinned"] = m => m.IsPinned
    };
    #endregion

    #region Conversation Queries
    public async Task<Conversation?> GetConversationByIdAsync(Guid conversationId, CancellationToken cancellationToken = default)
    {
        return await _db.Conversations
            .AsNoTracking()
            .Include(c => c.DirectUser1)
            .Include(c => c.DirectUser2)
            .Include(c => c.LastMessageSender)
            .FirstOrDefaultAsync(c => c.Id == conversationId && !c.IsDeleted, cancellationToken);
    }

    public async Task<Conversation?> GetConversationWithParticipantsAsync(Guid conversationId, CancellationToken cancellationToken = default)
    {
        return await _db.Conversations
            .Include(c => c.Participants.Where(p => !p.IsDeleted))
                .ThenInclude(p => p.User)
            .Include(c => c.Admins.Where(a => !a.IsDeleted))
            .Include(c => c.ReplyPermissions.Where(p => !p.IsDeleted))
            .FirstOrDefaultAsync(c => c.Id == conversationId && !c.IsDeleted, cancellationToken);
    }

    public async Task<Conversation?> FindDirectConversationAsync(Guid organizationId, Guid user1Id, Guid user2Id, CancellationToken cancellationToken = default)
    {
        var u1 = user1Id.CompareTo(user2Id) < 0 ? user1Id : user2Id;
        var u2 = user1Id.CompareTo(user2Id) < 0 ? user2Id : user1Id;

        return await _db.Conversations
            .AsNoTracking()
            .Include(c => c.DirectUser1)
            .Include(c => c.DirectUser2)
            .Include(c => c.LastMessageSender)
            .FirstOrDefaultAsync(c => 
                c.OrganizationId == organizationId 
                && c.Type == ConversationType.Direct 
                && c.DirectUser1Id == u1 
                && c.DirectUser2Id == u2 
                && !c.IsDeleted, cancellationToken);
    }

    public async Task<bool> ConversationExistsAsync(Guid conversationId, Guid organizationId, CancellationToken cancellationToken = default)
    {
        return await _db.Conversations
            .AnyAsync(c => c.Id == conversationId && c.OrganizationId == organizationId && !c.IsDeleted, cancellationToken);
    }

    public async Task<(List<Conversation> Items, int TotalRecords)> GetUserConversationsPagedAsync(
        Guid organizationId, 
        Guid userId, 
        PagedRequest request, 
        ConversationType? type = null, 
        CancellationToken cancellationToken = default)
    {
        var query = _db.Conversations
            .AsNoTracking()
            .Include(c => c.DirectUser1)
            .Include(c => c.DirectUser2)
            .Include(c => c.LastMessageSender)
            .Include(c => c.Participants.Where(p => !p.IsDeleted && p.IsActive))
                .ThenInclude(p => p.User)
            .Where(c => c.OrganizationId == organizationId && !c.IsDeleted && c.IsActive)
            .Where(c => c.Participants.Any(p => p.UserId == userId && !p.IsDeleted && p.IsActive));

        if (type.HasValue)
        {
            query = query.Where(c => c.Type == type.Value);
        }

        return await query.ToPagedResultAsync(
            request,
            searchPredicate: ConversationSearchPredicate,
            sortMappings: ConversationSortMappings,
            defaultSortExpression: c => c.LastMessageAt ?? c.CreatedAt,
            defaultAscending: false,
            cancellationToken: cancellationToken);
    }
    #endregion

    #region Conversation Commands
    public async Task AddConversationAsync(Conversation conversation, CancellationToken cancellationToken = default)
    {
        await _db.Conversations.AddAsync(conversation, cancellationToken);
    }

    public void UpdateConversation(Conversation conversation)
    {
        conversation.UpdatedAt = DateTime.UtcNow;
        _db.Conversations.Update(conversation);
    }

    public void DeleteConversation(Conversation conversation, Guid deletedBy)
    {
        conversation.IsDeleted = true;
        conversation.DeletedBy = deletedBy;
        conversation.DeletedAt = DateTime.UtcNow;
    }

    public async Task<Conversation?> GetConversationForUpdateAsync(Guid conversationId, Guid organizationId, CancellationToken cancellationToken = default)
    {
        return await _db.Conversations
            .FirstOrDefaultAsync(c => c.Id == conversationId && c.OrganizationId == organizationId && !c.IsDeleted, cancellationToken);
    }
    #endregion

    #region Participant Operations
    public async Task<ConversationParticipant?> GetParticipantAsync(Guid conversationId, Guid userId, CancellationToken cancellationToken = default)
    {
        return await _db.ConversationParticipants
            .Include(p => p.User)
            .FirstOrDefaultAsync(p => p.ConversationId == conversationId && p.UserId == userId && !p.IsDeleted, cancellationToken);
    }

    public async Task<ConversationParticipant?> GetActiveParticipantAsync(Guid conversationId, Guid userId, CancellationToken cancellationToken = default)
    {
        return await _db.ConversationParticipants
            .FirstOrDefaultAsync(cp => cp.ConversationId == conversationId && cp.UserId == userId && cp.IsActive, cancellationToken);
    }

    public async Task<ConversationParticipant?> GetParticipantForUpdateAsync(Guid conversationId, Guid userId, CancellationToken cancellationToken = default)
    {
        return await _db.ConversationParticipants
            .FirstOrDefaultAsync(cp => cp.ConversationId == conversationId && cp.UserId == userId, cancellationToken);
    }

    public async Task<List<ConversationParticipant>> GetParticipantsAsync(Guid conversationId, CancellationToken cancellationToken = default)
    {
        return await _db.ConversationParticipants
            .AsNoTracking()
            .Include(p => p.User)
            .Where(p => p.ConversationId == conversationId && !p.IsDeleted && p.IsActive)
            .OrderBy(p => p.JoinedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> IsParticipantAsync(Guid conversationId, Guid userId, CancellationToken cancellationToken = default)
    {
        return await _db.ConversationParticipants
            .AnyAsync(p => p.ConversationId == conversationId && p.UserId == userId && !p.IsDeleted && p.IsActive, cancellationToken);
    }

    public async Task<bool> IsAdminAsync(Guid conversationId, Guid userId, CancellationToken cancellationToken = default)
    {
        return await _db.ConversationParticipants
            .AnyAsync(p => p.ConversationId == conversationId && p.UserId == userId && p.IsAdmin && !p.IsDeleted && p.IsActive, cancellationToken)
            || await _db.ConversationAdmins
            .AnyAsync(a => a.ConversationId == conversationId && a.UserId == userId && !a.IsDeleted && a.IsActive, cancellationToken);
    }

    public async Task<ConversationAdmin?> GetActiveAdminRecordAsync(Guid conversationId, Guid userId, CancellationToken cancellationToken = default)
    {
        return await _db.ConversationAdmins
            .FirstOrDefaultAsync(ca => ca.ConversationId == conversationId && ca.UserId == userId && ca.IsActive, cancellationToken);
    }

    public async Task AddParticipantAsync(ConversationParticipant participant, CancellationToken cancellationToken = default)
    {
        await _db.ConversationParticipants.AddAsync(participant, cancellationToken);
    }

    public async Task AddParticipantsRangeAsync(IEnumerable<ConversationParticipant> participants, CancellationToken cancellationToken = default)
    {
        await _db.ConversationParticipants.AddRangeAsync(participants, cancellationToken);
    }

    public async Task AddConversationAdminAsync(ConversationAdmin admin, CancellationToken cancellationToken = default)
    {
        await _db.ConversationAdmins.AddAsync(admin, cancellationToken);
    }

    public void UpdateParticipant(ConversationParticipant participant)
    {
        participant.UpdatedAt = DateTime.UtcNow;
        _db.ConversationParticipants.Update(participant);
    }

    public void UpdateConversationAdmin(ConversationAdmin admin)
    {
        _db.ConversationAdmins.Update(admin);
    }

    public async Task<User?> GetUserBasicAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
    }

    public async Task<(List<User> Items, int TotalRecords)> GetEligibleUsersPagedAsync(
        IEnumerable<Guid> eligibleUserIds, 
        string? searchTerm, 
        int pageNumber, 
        int pageSize, 
        CancellationToken cancellationToken = default)
    {
        var ids = eligibleUserIds.ToList();
        if (!ids.Any())
        {
            return (new List<User>(), 0);
        }

        var usersQuery = _db.Users
            .AsNoTracking()
            .Where(u => ids.Contains(u.Id) && !u.IsDeleted && u.IsActive);

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var term = searchTerm.Trim().ToLower();
            usersQuery = usersQuery.Where(u =>
                u.FirstName.ToLower().Contains(term) ||
                u.LastName.ToLower().Contains(term) ||
                (u.Email != null && u.Email.ToLower().Contains(term)) ||
                (u.Mobile != null && u.Mobile.Contains(term)));
        }

        var totalRecords = await usersQuery.CountAsync(cancellationToken);

        var page = pageNumber <= 0 ? 1 : pageNumber;
        var size = pageSize <= 0 ? 20 : pageSize;

        var users = await usersQuery
            .OrderBy(u => u.FirstName)
            .ThenBy(u => u.LastName)
            .Skip((page - 1) * size)
            .Take(size)
            .ToListAsync(cancellationToken);

        return (users, totalRecords);
    }
    #endregion

    #region Message Operations
    public async Task<Message?> GetMessageByIdAsync(Guid messageId, CancellationToken cancellationToken = default)
    {
        return await _db.Messages
            .AsNoTracking()
            .Include(m => m.Sender)
            .Include(m => m.Reads)
            .Include(m => m.Reactions)
            .Include(m => m.Attachments.Where(a => !a.IsDeleted))
            .Include(m => m.ReplyToMessage)
                .ThenInclude(r => r!.Sender)
            .FirstOrDefaultAsync(m => m.Id == messageId && !m.IsDeleted, cancellationToken);
    }

    public async Task<Message?> GetMessageWithAttachmentsAsync(Guid messageId, CancellationToken cancellationToken = default)
    {
        return await _db.Messages
            .Include(m => m.Attachments.Where(a => !a.IsDeleted))
            .FirstOrDefaultAsync(m => m.Id == messageId && !m.IsDeleted, cancellationToken);
    }

    public async Task<(List<Message> Items, int TotalRecords)> GetMessagesPagedAsync(
        Guid conversationId, 
        PagedRequest request, 
        CancellationToken cancellationToken = default)
    {
        var query = _db.Messages
            .AsNoTracking()
            .Include(m => m.Sender)
            .Include(m => m.Reads)
            .Include(m => m.Reactions)
            .Include(m => m.Attachments.Where(a => !a.IsDeleted))
            .Include(m => m.ReplyToMessage)
                .ThenInclude(r => r!.Sender)
            .Where(m => m.ConversationId == conversationId && !m.IsDeleted);

        return await query.ToPagedResultAsync(
            request,
            sortMappings: MessageSortMappings,
            defaultSortExpression: m => m.SentAt,
            defaultAscending: false,
            cancellationToken: cancellationToken);
    }

    public async Task AddMessageAsync(Message message, CancellationToken cancellationToken = default)
    {
        await _db.Messages.AddAsync(message, cancellationToken);
    }

    public void UpdateMessage(Message message)
    {
        message.EditedAt = DateTime.UtcNow;
        _db.Messages.Update(message);
    }

    public void SoftDeleteMessage(Message message, Guid deletedBy)
    {
        message.IsDeleted = true;
        message.DeletedBy = deletedBy;
        message.DeletedAt = DateTime.UtcNow;
        _db.Messages.Update(message);
    }
    #endregion

    #region Read Tracking
    public async Task MarkMessagesAsReadAsync(Guid conversationId, Guid userId, Guid latestMessageId, CancellationToken cancellationToken = default)
    {
        var participant = await _db.ConversationParticipants
            .FirstOrDefaultAsync(p => p.ConversationId == conversationId && p.UserId == userId && !p.IsDeleted, cancellationToken);

        if (participant != null)
        {
            participant.LastReadAt = DateTime.UtcNow;
            participant.LastReadMessageId = latestMessageId;
            participant.UpdatedAt = DateTime.UtcNow;
        }

        // Get latest message sent time if available
        var latestMsg = await _db.Messages
            .AsNoTracking()
            .Where(m => m.Id == latestMessageId)
            .Select(m => new { m.Id, m.SentAt })
            .FirstOrDefaultAsync(cancellationToken);

        var cutoffTime = latestMsg?.SentAt ?? DateTime.UtcNow;

        // Find all unread messages up to latestMessageId/cutoffTime in this conversation not sent by this user
        var unreadMessages = await _db.Messages
            .AsNoTracking()
            .Where(m => m.ConversationId == conversationId
                     && m.SenderId != userId
                     && !m.IsDeleted
                     && m.SentAt <= cutoffTime
                     && !_db.MessageReads.Any(r => r.MessageId == m.Id && r.UserId == userId))
            .Select(m => new { m.Id, m.OrganizationId })
            .ToListAsync(cancellationToken);

        if (unreadMessages.Any())
        {
            var now = DateTime.UtcNow;
            var readsToAdd = unreadMessages.Select(m => new MessageRead
            {
                OrganizationId = m.OrganizationId,
                MessageId = m.Id,
                UserId = userId,
                ReadAt = now
            }).ToList();

            await _db.MessageReads.AddRangeAsync(readsToAdd, cancellationToken);
        }
    }

    public async Task<int> GetUnreadCountAsync(Guid conversationId, Guid userId, CancellationToken cancellationToken = default)
    {
        var participant = await _db.ConversationParticipants
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.ConversationId == conversationId && p.UserId == userId && !p.IsDeleted, cancellationToken);

        if (participant == null) return 0;

        var lastReadAt = participant.LastReadAt ?? participant.JoinedAt;

        return await _db.Messages
            .Where(m => m.ConversationId == conversationId && m.SenderId != userId && !m.IsDeleted && m.SentAt > lastReadAt)
            .CountAsync(cancellationToken);
    }
    #endregion

    #region Reaction Operations
    public async Task<(Dictionary<string, int> Reactions, string? ActiveEmoji)> ToggleReactionAsync(
        Guid messageId, 
        Guid userId, 
        Guid organizationId, 
        string emoji, 
        CancellationToken cancellationToken = default)
    {
        var existing = await _db.MessageReactions
            .FirstOrDefaultAsync(r => r.MessageId == messageId && r.UserId == userId, cancellationToken);

        string? activeEmoji = null;

        if (existing != null)
        {
            if (string.Equals(existing.Emoji, emoji, StringComparison.Ordinal))
            {
                // User clicked same emoji -> Remove reaction
                _db.MessageReactions.Remove(existing);
                activeEmoji = null;
            }
            else
            {
                // User clicked different emoji -> Swap reaction
                existing.Emoji = emoji;
                existing.ReactedAt = DateTime.UtcNow;
                activeEmoji = emoji;
            }
        }
        else
        {
            // Add new reaction
            var newReaction = new MessageReaction
            {
                Id = Guid.NewGuid(),
                OrganizationId = organizationId,
                MessageId = messageId,
                UserId = userId,
                Emoji = emoji,
                ReactedAt = DateTime.UtcNow
            };
            await _db.MessageReactions.AddAsync(newReaction, cancellationToken);
            activeEmoji = emoji;
        }

        await _db.SaveChangesAsync(cancellationToken);

        // Fetch current aggregated reaction counts for this message
        var reactions = await _db.MessageReactions
            .Where(r => r.MessageId == messageId)
            .GroupBy(r => r.Emoji)
            .Select(g => new { Emoji = g.Key, Count = g.Count() })
            .ToDictionaryAsync(g => g.Emoji, g => g.Count, cancellationToken);

        return (reactions, activeEmoji);
    }
    #endregion

    #region Posting Permissions
    public async Task<bool> CanUserPostAsync(Guid conversationId, Guid userId, UserRole userRole, CancellationToken cancellationToken = default)
    {
        // SuperAdmin and Institute OrgAdmin can always post in their institute
        if (userRole == UserRole.SuperAdmin || userRole == UserRole.OrganizationAdmin)
        {
            return true;
        }

        var conversation = await _db.Conversations
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == conversationId && !c.IsDeleted && c.IsActive, cancellationToken);

        if (conversation == null) return false;

        // In Direct chats: both participants can post
        if (conversation.Type == ConversationType.Direct)
        {
            return await _db.ConversationParticipants
                .AnyAsync(p => p.ConversationId == conversationId && p.UserId == userId && !p.IsDeleted && p.IsActive, cancellationToken);
        }

        // Creator of the conversation can always post
        if (conversation.CreatedBy == userId) return true;

        // Check if user is conversation admin
        var isConvAdmin = await _db.ConversationParticipants
            .AnyAsync(p => p.ConversationId == conversationId && p.UserId == userId && p.IsAdmin && !p.IsDeleted && p.IsActive, cancellationToken)
            || await _db.ConversationAdmins
            .AnyAsync(a => a.ConversationId == conversationId && a.UserId == userId && !a.IsDeleted && a.IsActive, cancellationToken);

        if (isConvAdmin) return true;

        // If it's an Announcement channel: check specific role permissions
        if (conversation.IsAnnouncement)
        {
            var rolePermission = await _db.AnnouncementReplyPermissions
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.ConversationId == conversationId && p.Role == userRole && !p.IsDeleted, cancellationToken);

            return rolePermission?.CanReply ?? false;
        }

        // Standard Group: check WhoCanReply policy
        return conversation.WhoCanReply switch
        {
            WhoCanReply.Everyone => await _db.ConversationParticipants
                .AnyAsync(p => p.ConversationId == conversationId && p.UserId == userId && !p.IsDeleted && p.IsActive, cancellationToken),
            WhoCanReply.AdminsOnly => isConvAdmin,
            WhoCanReply.TeachersAndAdmins => isConvAdmin || userRole == UserRole.Teacher || userRole == UserRole.BranchAdmin,
            _ => false
        };
    }
    #endregion

    #region Persistence
    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _db.SaveChangesAsync(cancellationToken);
    }
    #endregion
}
