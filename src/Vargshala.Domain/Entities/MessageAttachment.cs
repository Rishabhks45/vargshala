using Vargshala.Domain.Common;

namespace Vargshala.Domain.Entities;

public class MessageAttachment : BaseEntity
{
    public Guid OrganizationId { get; set; }
    public Guid MessageId { get; set; }

    public string FileName { get; set; } = string.Empty;
    public string? ContentType { get; set; }
    public long? FileSize { get; set; }
    public string FileUrl { get; set; } = string.Empty;
    public string? ThumbnailUrl { get; set; }
    public string? StorageKey { get; set; }

    // Navigation
    public Organization Organization { get; set; } = null!;
    public Message Message { get; set; } = null!;
}
