using Vargshala.Domain.Common;

namespace Vargshala.Domain.Entities;

public class ConversationAdmin : BaseEntity
{
    public Guid OrganizationId { get; set; }
    public Guid ConversationId { get; set; }
    public Guid UserId { get; set; }

    public Guid? AssignedBy { get; set; }
    public DateTime AssignedAt { get; set; } = DateTime.UtcNow;
    public DateTime? RemovedAt { get; set; }

    // Navigation
    public Organization Organization { get; set; } = null!;
    public Conversation Conversation { get; set; } = null!;
    public User User { get; set; } = null!;
    public User? AssignedByUser { get; set; }
}
