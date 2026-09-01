using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Vargshala.Domain.Entities;

namespace Vargshala.Infrastructure.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");

        builder.HasKey(u => u.Id);

        builder.Property(u => u.FirstName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(u => u.LastName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(u => u.Email)
            .HasMaxLength(150);

        builder.Property(u => u.Mobile)
            .HasMaxLength(20);

        builder.Property(u => u.PasswordHash)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(u => u.Role)
            .IsRequired();

        builder.Property(u => u.RefreshToken)
            .HasMaxLength(500);

        // Index: unique email within an organization (filtered by non-deleted)
        builder.HasIndex(u => new { u.Email, u.OrganizationId })
            .IsUnique()
            .HasFilter("\"Email\" IS NOT NULL AND \"IsDeleted\" = false");

        // FK to Organization (nullable for SuperAdmin)
        builder.HasOne(u => u.Organization)
            .WithMany(o => o.Users)
            .HasForeignKey(u => u.OrganizationId)
            .OnDelete(DeleteBehavior.Restrict);

        // Self-referencing audit FKs — NoAction to avoid circular cascade
        // CreatedBy, UpdatedBy, DeletedBy reference Users.Id
        // These are intentionally NOT configured as FK navigations to avoid
        // circular dependency issues. They are Guid columns only.
    }
}
