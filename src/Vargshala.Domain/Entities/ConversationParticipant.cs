using Vargshala.Domain.Common;
using Vargshala.SharedKernel.Enums;

namespace Vargshala.Domain.Entities;

public class ConversationParticipant : BaseEntity
{
    public Guid OrganizationId { get; set; }
    public Guid ConversationId { get; set; }
    public Guid UserId { get; set; }

    public ConversationParticipantRole Role { get; set; } = ConversationParticipantRole.Member;
    public bool IsAdmin { get; set; } = false;

    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LeftAt { get; set; }

    // Read watermark & unread tracking
    public DateTime? LastReadAt { get; set; }
    public Guid? LastReadMessageId { get; set; }

    // User preferences
    public bool IsMuted { get; set; } = false;
    public bool IsPinned { get; set; } = false;

    // Navigation
    public Organization Organization { get; set; } = null!;
    public Conversation Conversation { get; set; } = null!;
    public User User { get; set; } = null!;
}
