using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Vargshala.Domain.Entities;

namespace Vargshala.Infrastructure.Persistence.Configurations;

public class MessageAttachmentConfiguration : IEntityTypeConfiguration<MessageAttachment>
{
    public void Configure(EntityTypeBuilder<MessageAttachment> builder)
    {
        builder.ToTable("MessageAttachments");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.OrganizationId).IsRequired();
        builder.Property(a => a.MessageId).IsRequired();

        builder.Property(a => a.FileName)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(a => a.ContentType)
            .HasMaxLength(150);

        builder.Property(a => a.FileUrl)
            .IsRequired();

        builder.Property(a => a.StorageKey)
            .HasMaxLength(500);

        builder.Property(a => a.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(a => a.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        // Ignore BaseEntity properties not present in table
        builder.Ignore(a => a.IsActive);
        builder.Ignore(a => a.UpdatedBy);
        builder.Ignore(a => a.UpdatedAt);

        // Indexes
        builder.HasIndex(a => a.OrganizationId)
            .HasDatabaseName("IX_MessageAttachments_OrganizationId");

        builder.HasIndex(a => a.MessageId)
            .HasDatabaseName("IX_MessageAttachments_MessageId");

        // Foreign keys
        builder.HasOne(a => a.Organization)
            .WithMany()
            .HasForeignKey(a => a.OrganizationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(a => a.Message)
            .WithMany(m => m.Attachments)
            .HasForeignKey(a => a.MessageId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
