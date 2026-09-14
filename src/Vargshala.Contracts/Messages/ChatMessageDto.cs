using Vargshala.Contracts.Common;
using Vargshala.Contracts.Messages.Enums;

namespace Vargshala.Contracts.Messages;

public class ChatMessageDto
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ConversationId { get; set; }
    public string SenderId { get; set; } = string.Empty;
    public string SenderName { get; set; } = string.Empty;
    public string SenderRole { get; set; } = "Student"; // "Teacher", "Student", "Admin", "Parent"
    public UserRole? SenderUserRole { get; set; }
    public string SenderInitials { get; set; } = string.Empty;
    public string SenderAvatarColor { get; set; } = "teal";
    public MessageType MessageType { get; set; } = MessageType.Text;
    public SystemEventType? SystemEventType { get; set; }
    public string Content { get; set; } = string.Empty;
    public DateTime SentAt { get; set; } = DateTime.UtcNow;
    public bool IsOutgoing { get; set; } = false;
    public bool IsRead { get; set; } = false;
    public DateTime? ReadAt { get; set; }
    public bool IsAnnouncement { get; set; } = false;
    public bool IsPinned { get; set; } = false;
    public string? AnnouncementTitle { get; set; }
    public string? AttachmentName { get; set; }
    public string? AttachmentSize { get; set; }
    public string? AttachmentType { get; set; } // "PDF", "Image", "Code", "Archive"
    public string? AttachmentUrl { get; set; }
    public Dictionary<string, int> Reactions { get; set; } = new();
}

public class ChatConversationDto
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid OrganizationId { get; set; }
    public Guid? BranchId { get; set; }
    public Guid? BatchId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Subtitle { get; set; } = string.Empty;
    public ConversationType Type { get; set; } = ConversationType.Direct;
    public bool IsAnnouncement { get; set; } = false;
    public string? GroupPhotoUrl { get; set; }
    public string BranchName { get; set; } = "Main Campus — Patna";
    public string BatchName { get; set; } = string.Empty;
    public string Initials { get; set; } = string.Empty;
    public string AvatarColor { get; set; } = "teal";
    public int MemberCount { get; set; } = 1;
    public bool IsOnline { get; set; } = false;
    public string LastSeenText { get; set; } = "Offline";
    public string LastMessage { get; set; } = string.Empty;
    public string LastMessageTime { get; set; } = string.Empty;
    public DateTime? LastMessageAt { get; set; }
    public int UnreadCount { get; set; } = 0;
    public bool IsPinned { get; set; } = false;
    public bool IsMuted { get; set; } = false;
    
    // Permissions & Administration
    public ChannelPostingPermission WhoCanPost { get; set; } = ChannelPostingPermission.AllMembers;
    public WhoCanReply WhoCanReply { get; set; } = WhoCanReply.Everyone;
    public List<string> Admins { get; set; } = new();
    public bool IsAdmin { get; set; } = false;
    public string? AvatarUrl { get => GroupPhotoUrl; set => GroupPhotoUrl = value; }
    public string CreatedByRole { get; set; } = "Admin"; // "Admin", "Teacher", "Student"
    public UserRole? CreatedByUserRole { get; set; }
    
    public List<ChatMemberDto> Members { get; set; } = new();
    public List<SharedFileDto> SharedFiles { get; set; } = new();
}

public class ChatMemberDto
{
    public Guid UserId { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string Role { get; set; } = "Student"; // "Teacher", "Student", "Admin", "Parent"
    public UserRole? UserRole { get; set; }
    public string Initials { get; set; } = string.Empty;
    public bool IsOnline { get; set; } = false;
    public bool IsAdmin { get; set; } = false;
    public ConversationParticipantRole ConversationParticipantRole { get; set; } = ConversationParticipantRole.Member;
    public bool CanPost { get; set; } = true;
}

public class SharedFileDto
{
    public string FileName { get; set; } = string.Empty;
    public string FileSize { get; set; } = string.Empty;
    public string UploadedBy { get; set; } = string.Empty;
    public string UploadDate { get; set; } = string.Empty;
    public string Extension { get; set; } = "pdf";
}

public class MessagesReadNotification
{
    public Guid ConversationId { get; set; }
    public Guid ReadByUserId { get; set; }
    public Guid LatestMessageId { get; set; }
    public DateTime ReadAt { get; set; } = DateTime.UtcNow;
}

