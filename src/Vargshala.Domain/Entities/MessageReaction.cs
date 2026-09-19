using Vargshala.Domain.Common;

namespace Vargshala.Domain.Entities;

public class MessageReaction : BaseEntity
{
    public Guid OrganizationId { get; set; }
    public Guid MessageId { get; set; }
    public Guid UserId { get; set; }
    public string Emoji { get; set; } = string.Empty;

    public DateTime ReactedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public Organization Organization { get; set; } = null!;
    public Message Message { get; set; } = null!;
    public User User { get; set; } = null!;
}
