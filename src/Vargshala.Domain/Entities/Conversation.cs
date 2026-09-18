using Vargshala.Domain.Common;
using Vargshala.SharedKernel.Enums;

namespace Vargshala.Domain.Entities;

public class Conversation : BaseEntity
{
    public Guid OrganizationId { get; set; }
    public Guid? BranchId { get; set; }
    public Guid? BatchId { get; set; }

    public ConversationType Type { get; set; } = ConversationType.Direct;
    public string? Name { get; set; }
    public string? GroupPhotoUrl { get; set; }
    public string? Description { get; set; }

    // Direct 1-on-1 canonical lookup (User1Id < User2Id)
    public Guid? DirectUser1Id { get; set; }
    public Guid? DirectUser2Id { get; set; }

    public bool IsAnnouncement { get; set; } = false;
    public bool AllowReplies { get; set; } = true;
    public WhoCanReply WhoCanReply { get; set; } = WhoCanReply.Everyone;

    // Last message denormalized cache for sub-millisecond inbox sorting
    public Guid? LastMessageId { get; set; }
    public DateTime? LastMessageAt { get; set; }
    public string? LastMessageText { get; set; }
    public Guid? LastMessageSenderId { get; set; }

    // Navigation
    public Organization Organization { get; set; } = null!;
    public Branch? Branch { get; set; }
    public Batch? Batch { get; set; }
    public User? CreatedByUser { get; set; }
    public User? DirectUser1 { get; set; }
    public User? DirectUser2 { get; set; }
    public User? LastMessageSender { get; set; }

    public ICollection<ConversationParticipant> Participants { get; set; } = new List<ConversationParticipant>();
    public ICollection<ConversationAdmin> Admins { get; set; } = new List<ConversationAdmin>();
    public ICollection<Message> Messages { get; set; } = new List<Message>();
    public ICollection<AnnouncementReplyPermission> ReplyPermissions { get; set; } = new List<AnnouncementReplyPermission>();
}
