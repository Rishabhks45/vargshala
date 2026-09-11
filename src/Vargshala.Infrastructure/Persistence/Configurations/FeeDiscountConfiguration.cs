using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Vargshala.Domain.Entities;

namespace Vargshala.Infrastructure.Persistence.Configurations;

public class FeeDiscountConfiguration : IEntityTypeConfiguration<FeeDiscount>
{
    public void Configure(EntityTypeBuilder<FeeDiscount> builder)
    {
        builder.ToTable("FeeDiscounts");

        builder.HasKey(fd => fd.Id);

        builder.Property(fd => fd.OrganizationId).IsRequired();
        builder.Property(fd => fd.StudentFeeId).IsRequired();

        builder.Property(fd => fd.DiscountType)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(fd => fd.Value)
            .HasPrecision(12, 2)
            .IsRequired();

        builder.Property(fd => fd.Amount)
            .HasPrecision(12, 2)
            .IsRequired();

        builder.Property(fd => fd.Reason)
            .HasMaxLength(255);

        builder.Property(fd => fd.ApprovedAt)
            .IsRequired()
            .HasDefaultValueSql("NOW()");

        builder.Property(fd => fd.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("NOW()");

        // Foreign keys
        builder.HasOne(fd => fd.Organization)
            .WithMany()
            .HasForeignKey(fd => fd.OrganizationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(fd => fd.StudentFee)
            .WithMany(sf => sf.Discounts)
            .HasForeignKey(fd => fd.StudentFeeId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(fd => fd.ApprovedByUser)
            .WithMany()
            .HasForeignKey(fd => fd.ApprovedBy)
            .OnDelete(DeleteBehavior.SetNull);

        // Indexes
        builder.HasIndex(fd => fd.OrganizationId).HasDatabaseName("IX_FeeDiscounts_OrganizationId");
        builder.HasIndex(fd => fd.StudentFeeId).HasDatabaseName("IX_FeeDiscounts_StudentFeeId");
        builder.HasIndex(fd => fd.ApprovedBy).HasDatabaseName("IX_FeeDiscounts_ApprovedBy");
    }
}
