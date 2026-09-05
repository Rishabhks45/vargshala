using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Vargshala.Domain.Entities;

namespace Vargshala.Infrastructure.Persistence.Configurations;

public class CouponConfiguration : IEntityTypeConfiguration<Coupon>
{
    public void Configure(EntityTypeBuilder<Coupon> builder)
    {
        builder.ToTable("Coupons");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Code)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(c => c.Description)
            .HasMaxLength(500);

        builder.Property(c => c.DiscountValue)
            .HasPrecision(18, 2);

        builder.Property(c => c.MinOrderAmount)
            .HasPrecision(18, 2);

        builder.Property(c => c.MaxDiscountAmount)
            .HasPrecision(18, 2);

        // Soft delete filtered unique index on Code
        builder.HasIndex(c => c.Code)
            .IsUnique()
            .HasFilter("\"IsDeleted\" = false");

        // Filter index on Organization and Status
        builder.HasIndex(c => new { c.OrganizationId, c.IsActive })
            .HasFilter("\"IsDeleted\" = false");

        // Navigation
        builder.HasOne(c => c.Organization)
            .WithMany()
            .HasForeignKey(c => c.OrganizationId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
