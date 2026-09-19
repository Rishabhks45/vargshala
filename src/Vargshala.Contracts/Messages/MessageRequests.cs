using Vargshala.Contracts.Common;
using Vargshala.SharedKernel.Enums;

namespace Vargshala.Contracts.Messages;

public class CreateConversationRequest
{
    public ConversationType Type { get; set; } = ConversationType.Direct;
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? GroupPhotoUrl { get; set; }

    public Guid? BranchId { get; set; }
    public Guid? BatchId { get; set; }
    public Guid? TargetUserId { get; set; } // For Direct 1-on-1 chat

    public bool IsAnnouncement { get; set; } = false;
    public bool AllowReplies { get; set; } = true;
    public WhoCanReply WhoCanReply { get; set; } = WhoCanReply.Everyone;

    public List<Guid> ParticipantUserIds { get; set; } = new();
}

public class SendMessageRequest
{
    public Guid ConversationId { get; set; }
    public Guid? ReplyToMessageId { get; set; }

    public MessageType MessageType { get; set; } = MessageType.Text;
    public string? MessageText { get; set; }

    public List<MessageAttachmentRequest> Attachments { get; set; } = new();
}

public class MessageAttachmentRequest
{
    public string FileName { get; set; } = string.Empty;
    public string? ContentType { get; set; }
    public long? FileSize { get; set; }
    public string FileUrl { get; set; } = string.Empty;
    public string? ThumbnailUrl { get; set; }
    public string? StorageKey { get; set; }
}

public class AddConversationParticipantRequest
{
    public Guid ConversationId { get; set; }
    public Guid UserId { get; set; }
    public ConversationParticipantRole Role { get; set; } = ConversationParticipantRole.Member;
}

public class UpdateParticipantRoleRequest
{
    public Guid ConversationId { get; set; }
    public Guid UserId { get; set; }
    public ConversationParticipantRole NewRole { get; set; } = ConversationParticipantRole.Member;
}

public class GetConversationsPagedRequest : PagedRequest
{
    public ConversationType? Type { get; set; }
    public Guid? BranchId { get; set; }
    public Guid? BatchId { get; set; }
    public bool? IsAnnouncement { get; set; }
}

public class GetMessagesPagedRequest : PagedRequest
{
    public Guid ConversationId { get; set; }
    public DateTime? BeforeSentAt { get; set; } // Cursor pagination support for smooth infinite scroll
    public MessageType? MessageType { get; set; }
}

public class PromoteGroupAdminRequest
{
    public Guid ConversationId { get; set; }
    public Guid TargetUserId { get; set; }
}

public class RemoveGroupMemberRequest
{
    public Guid ConversationId { get; set; }
    public Guid TargetUserId { get; set; }
}

public class ChangeGroupPhotoRequest
{
    public Guid ConversationId { get; set; }
    public string GroupPhotoUrl { get; set; } = string.Empty;
}

public class GetEligibleRecipientsRequest : PagedRequest
{
    public string? SearchTerm { get; set; }
}

public class EligibleUserDto
{
    public Guid UserId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string? Role { get; set; }
    public string? Email { get; set; }
    public string? AvatarUrl { get; set; }
    public string? Subtitle { get; set; }
}

public class ToggleMessageReactionRequest
{
    public string Emoji { get; set; } = string.Empty;
}


