using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Vargshala.Domain.Entities;

namespace Vargshala.Infrastructure.Persistence.Configurations;

public class ConversationConfiguration : IEntityTypeConfiguration<Conversation>
{
    public void Configure(EntityTypeBuilder<Conversation> builder)
    {
        builder.ToTable("Conversations");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.OrganizationId).IsRequired();
        builder.Property(c => c.CreatedBy).IsRequired();

        builder.Property(c => c.Type)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(30);

        builder.Property(c => c.Name)
            .HasMaxLength(200);

        builder.Property(c => c.WhoCanReply)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(30)
            .HasDefaultValue(WhoCanReply.Everyone);

        builder.Property(c => c.IsAnnouncement)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(c => c.AllowReplies)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(c => c.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(c => c.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(c => c.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        // Indexes
        builder.HasIndex(c => c.OrganizationId)
            .HasDatabaseName("IX_Conversations_OrganizationId");

        builder.HasIndex(c => new { c.OrganizationId, c.LastMessageAt })
            .HasDatabaseName("IX_Conversations_Org_LastMessageAt");

        builder.HasIndex(c => c.BranchId)
            .HasDatabaseName("IX_Conversations_BranchId");

        builder.HasIndex(c => c.BatchId)
            .HasDatabaseName("IX_Conversations_BatchId");

        builder.HasIndex(c => c.CreatedBy)
            .HasDatabaseName("IX_Conversations_CreatedBy");

        // Unique Direct Chat index
        builder.HasIndex(c => new { c.OrganizationId, c.DirectUser1Id, c.DirectUser2Id })
            .HasDatabaseName("UQ_Conversations_DirectUsers")
            .IsUnique()
            .HasFilter("\"Type\" = 'Direct' AND \"IsDeleted\" = false");

        // Foreign keys
        builder.HasOne(c => c.Organization)
            .WithMany()
            .HasForeignKey(c => c.OrganizationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(c => c.Branch)
            .WithMany()
            .HasForeignKey(c => c.BranchId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(c => c.Batch)
            .WithMany()
            .HasForeignKey(c => c.BatchId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(c => c.CreatedByUser)
            .WithMany()
            .HasForeignKey(c => c.CreatedBy)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(c => c.DirectUser1)
            .WithMany()
            .HasForeignKey(c => c.DirectUser1Id)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(c => c.DirectUser2)
            .WithMany()
            .HasForeignKey(c => c.DirectUser2Id)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(c => c.LastMessageSender)
            .WithMany()
            .HasForeignKey(c => c.LastMessageSenderId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(c => c.Participants)
            .WithOne(cp => cp.Conversation)
            .HasForeignKey(cp => cp.ConversationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(c => c.Admins)
            .WithOne(ca => ca.Conversation)
            .HasForeignKey(ca => ca.ConversationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(c => c.Messages)
            .WithOne(m => m.Conversation)
            .HasForeignKey(m => m.ConversationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(c => c.ReplyPermissions)
            .WithOne(rp => rp.Conversation)
            .HasForeignKey(rp => rp.ConversationId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
