using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Vargshala.Domain.Entities;

namespace Vargshala.Infrastructure.Persistence.Configurations;

public class ConversationAdminConfiguration : IEntityTypeConfiguration<ConversationAdmin>
{
    public void Configure(EntityTypeBuilder<ConversationAdmin> builder)
    {
        builder.ToTable("ConversationAdmins");

        builder.HasKey(ca => ca.Id);

        builder.Property(ca => ca.OrganizationId).IsRequired();
        builder.Property(ca => ca.ConversationId).IsRequired();
        builder.Property(ca => ca.UserId).IsRequired();

        builder.Property(ca => ca.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(ca => ca.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(ca => ca.AssignedAt)
            .IsRequired()
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        // Indexes
        builder.HasIndex(ca => ca.OrganizationId)
            .HasDatabaseName("IX_ConversationAdmins_OrganizationId");

        builder.HasIndex(ca => ca.ConversationId)
            .HasDatabaseName("IX_ConversationAdmins_ConversationId");

        builder.HasIndex(ca => ca.UserId)
            .HasDatabaseName("IX_ConversationAdmins_UserId");

        builder.HasIndex(ca => new { ca.ConversationId, ca.UserId })
            .HasDatabaseName("UQ_ConversationAdmins_Active")
            .IsUnique()
            .HasFilter("\"IsDeleted\" = false");

        // Foreign keys
        builder.HasOne(ca => ca.Organization)
            .WithMany()
            .HasForeignKey(ca => ca.OrganizationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(ca => ca.Conversation)
            .WithMany(c => c.Admins)
            .HasForeignKey(ca => ca.ConversationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(ca => ca.User)
            .WithMany()
            .HasForeignKey(ca => ca.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(ca => ca.AssignedByUser)
            .WithMany()
            .HasForeignKey(ca => ca.AssignedBy)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
