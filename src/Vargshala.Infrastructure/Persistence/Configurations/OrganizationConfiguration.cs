using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Vargshala.Domain.Entities;

namespace Vargshala.Infrastructure.Persistence.Configurations;

public class OrganizationConfiguration : IEntityTypeConfiguration<Organization>
{
    public void Configure(EntityTypeBuilder<Organization> builder)
    {
        builder.ToTable("Organizations");

        builder.HasKey(o => o.Id);

        builder.Property(o => o.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(o => o.Code)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(o => o.LogoUrl)
            .HasMaxLength(500);

        builder.Property(o => o.Email)
            .HasMaxLength(150);

        builder.Property(o => o.Mobile)
            .HasMaxLength(20);

        builder.Property(o => o.Address)
            .HasMaxLength(500);

        builder.Property(o => o.City)
            .HasMaxLength(100);

        builder.Property(o => o.State)
            .HasMaxLength(100);

        builder.Property(o => o.Pincode)
            .HasMaxLength(10);

        builder.Property(o => o.AcademicSession)
            .HasMaxLength(20);

        // Unique constraint on Code
        builder.HasIndex(o => o.Code)
            .IsUnique()
            .HasFilter("\"IsDeleted\" = false");

        // Navigation
        builder.HasMany(o => o.Users)
            .WithOne(u => u.Organization)
            .HasForeignKey(u => u.OrganizationId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
