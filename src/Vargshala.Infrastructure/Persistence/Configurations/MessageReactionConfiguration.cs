using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Vargshala.Domain.Entities;

namespace Vargshala.Infrastructure.Persistence.Configurations;

public class MessageReactionConfiguration : IEntityTypeConfiguration<MessageReaction>
{
    public void Configure(EntityTypeBuilder<MessageReaction> builder)
    {
        builder.ToTable("MessageReactions");

        builder.HasKey(mr => mr.Id);

        builder.Property(mr => mr.OrganizationId).IsRequired();
        builder.Property(mr => mr.MessageId).IsRequired();
        builder.Property(mr => mr.UserId).IsRequired();
        builder.Property(mr => mr.Emoji).IsRequired().HasMaxLength(32);

        builder.Property(mr => mr.ReactedAt)
            .IsRequired()
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        // Ignore BaseEntity audit/soft-delete fields not present in MessageReactions table
        builder.Ignore(mr => mr.IsActive);
        builder.Ignore(mr => mr.CreatedBy);
        builder.Ignore(mr => mr.CreatedAt);
        builder.Ignore(mr => mr.UpdatedBy);
        builder.Ignore(mr => mr.UpdatedAt);
        builder.Ignore(mr => mr.IsDeleted);
        builder.Ignore(mr => mr.DeletedBy);
        builder.Ignore(mr => mr.DeletedAt);

        // Unique constraint: A user has at most one reaction on a given message
        builder.HasIndex(mr => new { mr.MessageId, mr.UserId })
            .IsUnique()
            .HasDatabaseName("UQ_MessageReactions_MessageId_UserId");

        // Indexes
        builder.HasIndex(mr => mr.OrganizationId)
            .HasDatabaseName("IX_MessageReactions_OrganizationId");

        builder.HasIndex(mr => mr.MessageId)
            .HasDatabaseName("IX_MessageReactions_MessageId");

        builder.HasIndex(mr => mr.UserId)
            .HasDatabaseName("IX_MessageReactions_UserId");

        // Foreign keys
        builder.HasOne(mr => mr.Organization)
            .WithMany()
            .HasForeignKey(mr => mr.OrganizationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(mr => mr.Message)
            .WithMany(m => m.Reactions)
            .HasForeignKey(mr => mr.MessageId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(mr => mr.User)
            .WithMany()
            .HasForeignKey(mr => mr.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
