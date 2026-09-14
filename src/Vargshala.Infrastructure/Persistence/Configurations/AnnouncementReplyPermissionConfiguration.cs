using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Vargshala.Domain.Entities;

namespace Vargshala.Infrastructure.Persistence.Configurations;

public class AnnouncementReplyPermissionConfiguration : IEntityTypeConfiguration<AnnouncementReplyPermission>
{
    public void Configure(EntityTypeBuilder<AnnouncementReplyPermission> builder)
    {
        builder.ToTable("AnnouncementReplyPermissions");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.OrganizationId).IsRequired();
        builder.Property(p => p.ConversationId).IsRequired();

        builder.Property(p => p.Role)
            .IsRequired();

        builder.Property(p => p.CanReply)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(p => p.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(p => p.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);

        // Ignore BaseEntity properties not present in table
        builder.Ignore(p => p.IsActive);

        // Unique constraint: one permission record per conversation & role
        builder.HasIndex(p => new { p.ConversationId, p.Role })
            .IsUnique()
            .HasDatabaseName("UQ_AnnouncementReplyPermissions_ConversationId_Role");

        // Indexes
        builder.HasIndex(p => p.OrganizationId)
            .HasDatabaseName("IX_AnnouncementReplyPermissions_OrganizationId");

        builder.HasIndex(p => p.ConversationId)
            .HasDatabaseName("IX_AnnouncementReplyPermissions_ConversationId");

        // Foreign keys
        builder.HasOne(p => p.Organization)
            .WithMany()
            .HasForeignKey(p => p.OrganizationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.Conversation)
            .WithMany(c => c.ReplyPermissions)
            .HasForeignKey(p => p.ConversationId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
