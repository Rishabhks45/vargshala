using Vargshala.Contracts.Common;
using Vargshala.Domain.Common;

namespace Vargshala.Domain.Entities;

public class AnnouncementReplyPermission : BaseEntity
{
    public Guid OrganizationId { get; set; }
    public Guid ConversationId { get; set; }

    public UserRole Role { get; set; }
    public bool CanReply { get; set; } = false;

    // Navigation
    public Organization Organization { get; set; } = null!;
    public Conversation Conversation { get; set; } = null!;
}
