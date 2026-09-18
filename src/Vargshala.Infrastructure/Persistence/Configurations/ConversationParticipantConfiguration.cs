using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Vargshala.Domain.Entities;

namespace Vargshala.Infrastructure.Persistence.Configurations;

public class ConversationParticipantConfiguration : IEntityTypeConfiguration<ConversationParticipant>
{
    public void Configure(EntityTypeBuilder<ConversationParticipant> builder)
    {
        builder.ToTable("ConversationParticipants");

        builder.HasKey(cp => cp.Id);

        builder.Property(cp => cp.OrganizationId).IsRequired();
        builder.Property(cp => cp.ConversationId).IsRequired();
        builder.Property(cp => cp.UserId).IsRequired();

        builder.Property(cp => cp.Role)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20)
            .HasDefaultValue(ConversationParticipantRole.Member);

        builder.Property(cp => cp.IsAdmin)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(cp => cp.IsMuted)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(cp => cp.IsPinned)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(cp => cp.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(cp => cp.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(cp => cp.JoinedAt)
            .IsRequired()
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        // Indexes
        builder.HasIndex(cp => cp.OrganizationId)
            .HasDatabaseName("IX_ConversationParticipants_OrganizationId");

        builder.HasIndex(cp => cp.ConversationId)
            .HasDatabaseName("IX_ConversationParticipants_ConversationId");

        builder.HasIndex(cp => cp.UserId)
            .HasDatabaseName("IX_ConversationParticipants_UserId");

        builder.HasIndex(cp => new { cp.UserId, cp.IsActive })
            .HasDatabaseName("IX_ConversationParticipants_User_Active")
            .HasFilter("\"IsDeleted\" = false");

        builder.HasIndex(cp => new { cp.ConversationId, cp.UserId })
            .HasDatabaseName("UQ_ConversationParticipants_Active")
            .IsUnique()
            .HasFilter("\"IsDeleted\" = false");

        // Foreign keys
        builder.HasOne(cp => cp.Organization)
            .WithMany()
            .HasForeignKey(cp => cp.OrganizationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(cp => cp.Conversation)
            .WithMany(c => c.Participants)
            .HasForeignKey(cp => cp.ConversationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(cp => cp.User)
            .WithMany()
            .HasForeignKey(cp => cp.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
