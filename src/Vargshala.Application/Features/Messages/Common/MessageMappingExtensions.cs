using Vargshala.Contracts.Common;
using Vargshala.Contracts.Messages;
using Vargshala.SharedKernel.Enums;
using Vargshala.Domain.Entities;

namespace Vargshala.Application.Features.Messages.Common;

public static class MessageMappingExtensions
{
    private static readonly string[] AvatarColors = { "teal", "indigo", "emerald", "blue", "violet", "amber" };

    public static string GetInitials(string? name)
    {
        if (string.IsNullOrWhiteSpace(name)) return "U";
        var parts = name.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 1) return parts[0][0].ToString().ToUpperInvariant();
        return $"{parts[0][0]}{parts[^1][0]}".ToUpperInvariant();
    }

    public static string GetAvatarColor(Guid id)
    {
        var hash = Math.Abs(id.GetHashCode());
        return AvatarColors[hash % AvatarColors.Length];
    }

    public static string FormatFileSize(long? bytes)
    {
        if (bytes == null || bytes == 0) return string.Empty;
        var b = bytes.Value;
        if (b >= 1024 * 1024) return $"{b / (1024.0 * 1024.0):0.#} MB";
        if (b >= 1024) return $"{b / 1024.0:0.#} KB";
        return $"{b} B";
    }

    public static string FormatMessageTime(DateTime? time)
    {
        if (!time.HasValue) return string.Empty;
        var dt = time.Value.ToLocalTime();
        var now = DateTime.Now;

        if (dt.Date == now.Date)
        {
            return dt.ToString("hh:mm tt");
        }
        if (dt.Date == now.Date.AddDays(-1))
        {
            return "Yesterday";
        }
        if (dt.Year == now.Year)
        {
            return dt.ToString("MMM dd");
        }
        return dt.ToString("MMM dd, yyyy");
    }

    public static ChatConversationDto ToDto(this Conversation c, Guid currentUserId)
    {
        string name = c.Name ?? string.Empty;
        string subtitle = c.Description ?? string.Empty;
        string initials = "G";

        if (c.Type == ConversationType.Direct)
        {
            var counterpart = c.DirectUser1Id == currentUserId ? c.DirectUser2 : c.DirectUser1;
            if (counterpart == null)
            {
                counterpart = c.Participants.FirstOrDefault(p => p.UserId != currentUserId)?.User;
            }

            if (counterpart != null)
            {
                name = $"{counterpart.FirstName} {counterpart.LastName}".Trim();
                subtitle = counterpart.Role.ToString();
                initials = GetInitials(name);
            }
            else
            {
                name = "Direct Chat";
                subtitle = "User";
                initials = "D";
            }
        }
        else
        {
            name = !string.IsNullOrWhiteSpace(c.Name) ? c.Name : (c.IsAnnouncement ? "Announcements" : "Group Chat");
            subtitle = !string.IsNullOrWhiteSpace(c.Description) 
                ? c.Description 
                : $"{c.Participants.Count(p => !p.IsDeleted && p.IsActive)} members";
            initials = GetInitials(name);
        }

        var dto = new ChatConversationDto
        {
            Id = c.Id,
            OrganizationId = c.OrganizationId,
            BranchId = c.BranchId,
            BatchId = c.BatchId,
            Name = name,
            Subtitle = subtitle,
            Type = c.Type,
            IsAnnouncement = c.IsAnnouncement,
            GroupPhotoUrl = c.GroupPhotoUrl,
            Initials = initials,
            AvatarColor = GetAvatarColor(c.Id),
            MemberCount = c.Participants.Count(p => !p.IsDeleted && p.IsActive),
            LastMessage = c.LastMessageText ?? string.Empty,
            LastMessageAt = c.LastMessageAt,
            LastMessageTime = FormatMessageTime(c.LastMessageAt ?? c.CreatedAt),
            WhoCanReply = c.WhoCanReply,
            WhoCanPost = c.WhoCanReply switch
            {
                WhoCanReply.AdminsOnly => ChannelPostingPermission.AdminsOnly,
                WhoCanReply.TeachersAndAdmins => ChannelPostingPermission.AdminsAndTeachersOnly,
                _ => ChannelPostingPermission.AllMembers
            },
            Members = c.Participants
                .Where(p => !p.IsDeleted && p.IsActive)
                .Select(p => new ChatMemberDto
                {
                    UserId = p.UserId,
                    Name = p.User != null ? $"{p.User.FirstName} {p.User.LastName}".Trim() : "Member",
                    Role = p.User?.Role.ToString() ?? "Student",
                    UserRole = p.User?.Role,
                    Initials = p.User != null ? GetInitials($"{p.User.FirstName} {p.User.LastName}") : "M",
                    IsAdmin = p.IsAdmin,
                    ConversationParticipantRole = p.Role,
                    CanPost = true
                }).ToList(),
            IsAdmin = c.Participants.Any(p => p.UserId == currentUserId && (p.IsAdmin || p.Role == ConversationParticipantRole.Admin) && !p.IsDeleted && p.IsActive)
        };

        return dto;
    }

    public static ChatMessageDto ToDto(this Message m, Guid currentUserId)
    {
        var senderName = m.Sender != null 
            ? $"{m.Sender.FirstName} {m.Sender.LastName}".Trim() 
            : "User";

        var att = m.Attachments.FirstOrDefault(a => !a.IsDeleted);
        var isOutgoing = m.SenderId == currentUserId;
        var readReceipt = m.Reads.FirstOrDefault(r => r.UserId != m.SenderId);
        var isRead = isOutgoing && (readReceipt != null || m.Reads.Any(r => r.UserId != currentUserId));

        return new ChatMessageDto
        {
            Id = m.Id,
            ConversationId = m.ConversationId,
            SenderId = m.SenderId.ToString(),
            SenderName = senderName,
            SenderRole = m.Sender?.Role.ToString() ?? "Student",
            SenderUserRole = m.Sender?.Role,
            SenderInitials = GetInitials(senderName),
            SenderAvatarColor = GetAvatarColor(m.SenderId),
            MessageType = m.MessageType,
            SystemEventType = m.SystemEventType,
            Content = m.MessageText ?? string.Empty,
            SentAt = m.SentAt,
            IsOutgoing = isOutgoing,
            IsRead = isRead,
            ReadAt = readReceipt?.ReadAt,
            IsPinned = m.IsPinned,
            AttachmentName = att?.FileName,
            AttachmentSize = FormatFileSize(att?.FileSize),
            AttachmentType = att?.ContentType,
            AttachmentUrl = att?.FileUrl
        };
    }
}
