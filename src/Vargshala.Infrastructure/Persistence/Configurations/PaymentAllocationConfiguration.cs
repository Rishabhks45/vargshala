using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Vargshala.Domain.Entities;

namespace Vargshala.Infrastructure.Persistence.Configurations;

public class PaymentAllocationConfiguration : IEntityTypeConfiguration<PaymentAllocation>
{
    public void Configure(EntityTypeBuilder<PaymentAllocation> builder)
    {
        builder.ToTable("PaymentAllocations");

        builder.HasKey(pa => pa.Id);

        builder.Property(pa => pa.PaymentId).IsRequired();
        builder.Property(pa => pa.FeeInstallmentId).IsRequired();

        builder.Property(pa => pa.AllocatedAmount)
            .HasPrecision(12, 2)
            .IsRequired();

        builder.Property(pa => pa.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(pa => pa.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("NOW()");

        builder.Property(pa => pa.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);

        // Foreign keys
        builder.HasOne(pa => pa.Payment)
            .WithMany(p => p.Allocations)
            .HasForeignKey(pa => pa.PaymentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(pa => pa.FeeInstallment)
            .WithMany(fi => fi.PaymentAllocations)
            .HasForeignKey(pa => pa.FeeInstallmentId)
            .OnDelete(DeleteBehavior.Restrict);

        // Unique & Indexes
        builder.HasIndex(pa => new { pa.PaymentId, pa.FeeInstallmentId })
            .IsUnique()
            .HasDatabaseName("UQ_PaymentAllocations_PaymentId_FeeInstallmentId");

        builder.HasIndex(pa => pa.PaymentId).HasDatabaseName("IX_PaymentAllocations_PaymentId");
        builder.HasIndex(pa => pa.FeeInstallmentId).HasDatabaseName("IX_PaymentAllocations_FeeInstallmentId");
    }
}
