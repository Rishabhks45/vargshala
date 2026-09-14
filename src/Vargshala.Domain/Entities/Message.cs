using Vargshala.Contracts.Messages.Enums;
using Vargshala.Domain.Common;

namespace Vargshala.Domain.Entities;

public class Message : BaseEntity
{
    public Guid OrganizationId { get; set; }
    public Guid ConversationId { get; set; }
    public Guid SenderId { get; set; }
    public Guid? ReplyToMessageId { get; set; }

    public MessageType MessageType { get; set; } = MessageType.Text;
    public string? MessageText { get; set; }

    // Pinning feature
    public bool IsPinned { get; set; } = false;
    public DateTime? PinnedAt { get; set; }
    public Guid? PinnedBy { get; set; }

    // System event info
    public SystemEventType? SystemEventType { get; set; }
    public Guid? SystemEventUserId { get; set; }
    public Guid? TargetUserId { get; set; }

    public DateTime SentAt { get; set; } = DateTime.UtcNow;
    public DateTime? EditedAt { get; set; }

    // Navigation
    public Organization Organization { get; set; } = null!;
    public Conversation Conversation { get; set; } = null!;
    public User Sender { get; set; } = null!;
    public Message? ReplyToMessage { get; set; }
    public User? PinnedByUser { get; set; }
    public User? SystemEventUser { get; set; }
    public User? TargetUser { get; set; }

    public ICollection<MessageAttachment> Attachments { get; set; } = new List<MessageAttachment>();
    public ICollection<MessageRead> Reads { get; set; } = new List<MessageRead>();
    public ICollection<Message> Replies { get; set; } = new List<Message>();
}
