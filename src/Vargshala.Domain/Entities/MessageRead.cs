using Vargshala.Domain.Common;

namespace Vargshala.Domain.Entities;

public class MessageRead : BaseEntity
{
    public Guid OrganizationId { get; set; }
    public Guid MessageId { get; set; }
    public Guid UserId { get; set; }

    public DateTime ReadAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public Organization Organization { get; set; } = null!;
    public Message Message { get; set; } = null!;
    public User User { get; set; } = null!;
}
