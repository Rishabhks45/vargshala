using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Vargshala.Domain.Entities;

namespace Vargshala.Infrastructure.Persistence.Configurations;

public class MessageReadConfiguration : IEntityTypeConfiguration<MessageRead>
{
    public void Configure(EntityTypeBuilder<MessageRead> builder)
    {
        builder.ToTable("MessageReads");

        builder.HasKey(mr => mr.Id);

        builder.Property(mr => mr.OrganizationId).IsRequired();
        builder.Property(mr => mr.MessageId).IsRequired();
        builder.Property(mr => mr.UserId).IsRequired();

        builder.Property(mr => mr.ReadAt)
            .IsRequired()
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        // Ignore BaseEntity audit/soft-delete fields not present in MessageReads table
        builder.Ignore(mr => mr.IsActive);
        builder.Ignore(mr => mr.CreatedBy);
        builder.Ignore(mr => mr.CreatedAt);
        builder.Ignore(mr => mr.UpdatedBy);
        builder.Ignore(mr => mr.UpdatedAt);
        builder.Ignore(mr => mr.IsDeleted);
        builder.Ignore(mr => mr.DeletedBy);
        builder.Ignore(mr => mr.DeletedAt);

        // Unique constraint: A user reads a message only once
        builder.HasIndex(mr => new { mr.MessageId, mr.UserId })
            .IsUnique()
            .HasDatabaseName("UQ_MessageReads_MessageId_UserId");

        // Indexes
        builder.HasIndex(mr => mr.OrganizationId)
            .HasDatabaseName("IX_MessageReads_OrganizationId");

        builder.HasIndex(mr => mr.MessageId)
            .HasDatabaseName("IX_MessageReads_MessageId");

        builder.HasIndex(mr => mr.UserId)
            .HasDatabaseName("IX_MessageReads_UserId");

        // Foreign keys
        builder.HasOne(mr => mr.Organization)
            .WithMany()
            .HasForeignKey(mr => mr.OrganizationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(mr => mr.Message)
            .WithMany(m => m.Reads)
            .HasForeignKey(mr => mr.MessageId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(mr => mr.User)
            .WithMany()
            .HasForeignKey(mr => mr.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
