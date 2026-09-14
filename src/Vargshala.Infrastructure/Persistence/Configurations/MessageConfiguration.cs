using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Vargshala.Domain.Entities;

namespace Vargshala.Infrastructure.Persistence.Configurations;

public class MessageConfiguration : IEntityTypeConfiguration<Message>
{
    public void Configure(EntityTypeBuilder<Message> builder)
    {
        builder.ToTable("Messages");

        builder.HasKey(m => m.Id);

        builder.Property(m => m.OrganizationId).IsRequired();
        builder.Property(m => m.ConversationId).IsRequired();
        builder.Property(m => m.SenderId).IsRequired();

        builder.Property(m => m.MessageType)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(30)
            .HasDefaultValue(Contracts.Messages.Enums.MessageType.Text);

        builder.Property(m => m.SystemEventType)
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(m => m.IsPinned)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(m => m.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(m => m.SentAt)
            .IsRequired()
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        // Ignore BaseEntity properties not present in Messages table
        builder.Ignore(m => m.IsActive);
        builder.Ignore(m => m.CreatedBy);
        builder.Ignore(m => m.CreatedAt);
        builder.Ignore(m => m.UpdatedBy);
        builder.Ignore(m => m.UpdatedAt);

        // Indexes
        builder.HasIndex(m => m.OrganizationId)
            .HasDatabaseName("IX_Messages_OrganizationId");

        builder.HasIndex(m => new { m.ConversationId, m.SentAt })
            .HasDatabaseName("IX_Messages_ConversationId_SentAt")
            .HasFilter("\"IsDeleted\" = false");

        builder.HasIndex(m => m.SenderId)
            .HasDatabaseName("IX_Messages_SenderId");

        builder.HasIndex(m => m.ReplyToMessageId)
            .HasDatabaseName("IX_Messages_ReplyToMessageId");

        builder.HasIndex(m => m.SystemEventUserId)
            .HasDatabaseName("IX_Messages_SystemEventUserId");

        builder.HasIndex(m => m.TargetUserId)
            .HasDatabaseName("IX_Messages_TargetUserId");

        // Foreign keys
        builder.HasOne(m => m.Organization)
            .WithMany()
            .HasForeignKey(m => m.OrganizationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(m => m.Conversation)
            .WithMany(c => c.Messages)
            .HasForeignKey(m => m.ConversationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(m => m.Sender)
            .WithMany()
            .HasForeignKey(m => m.SenderId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(m => m.ReplyToMessage)
            .WithMany(m => m.Replies)
            .HasForeignKey(m => m.ReplyToMessageId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(m => m.PinnedByUser)
            .WithMany()
            .HasForeignKey(m => m.PinnedBy)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(m => m.SystemEventUser)
            .WithMany()
            .HasForeignKey(m => m.SystemEventUserId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(m => m.TargetUser)
            .WithMany()
            .HasForeignKey(m => m.TargetUserId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(m => m.Attachments)
            .WithOne(a => a.Message)
            .HasForeignKey(a => a.MessageId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(m => m.Reads)
            .WithOne(r => r.Message)
            .HasForeignKey(r => r.MessageId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
