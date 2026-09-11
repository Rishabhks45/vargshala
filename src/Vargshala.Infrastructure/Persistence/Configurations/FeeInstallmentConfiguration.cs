using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Vargshala.Domain.Entities;

namespace Vargshala.Infrastructure.Persistence.Configurations;

public class FeeInstallmentConfiguration : IEntityTypeConfiguration<FeeInstallment>
{
    public void Configure(EntityTypeBuilder<FeeInstallment> builder)
    {
        builder.ToTable("FeeInstallments");

        builder.HasKey(fi => fi.Id);

        builder.Property(fi => fi.OrganizationId).IsRequired();
        builder.Property(fi => fi.StudentFeeId).IsRequired();
        builder.Property(fi => fi.InstallmentNumber).IsRequired();

        builder.Property(fi => fi.Amount)
            .HasPrecision(12, 2)
            .IsRequired();

        builder.Property(fi => fi.PaidAmount)
            .HasPrecision(12, 2)
            .IsRequired()
            .HasDefaultValue(0.00m);

        builder.Property(fi => fi.DueDate)
            .IsRequired();

        builder.Property(fi => fi.Status)
            .IsRequired()
            .HasMaxLength(30)
            .HasDefaultValue("Pending");

        builder.Property(fi => fi.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("NOW()");

        // Foreign keys
        builder.HasOne(fi => fi.Organization)
            .WithMany()
            .HasForeignKey(fi => fi.OrganizationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(fi => fi.StudentFee)
            .WithMany(sf => sf.Installments)
            .HasForeignKey(fi => fi.StudentFeeId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(fi => fi.PaymentAllocations)
            .WithOne(pa => pa.FeeInstallment)
            .HasForeignKey(pa => pa.FeeInstallmentId)
            .OnDelete(DeleteBehavior.Restrict);

        // Unique & Indexes
        builder.HasIndex(fi => new { fi.StudentFeeId, fi.InstallmentNumber })
            .IsUnique()
            .HasDatabaseName("UQ_FeeInstallments_StudentFeeId_InstallmentNumber");

        builder.HasIndex(fi => fi.OrganizationId).HasDatabaseName("IX_FeeInstallments_OrganizationId");
        builder.HasIndex(fi => fi.StudentFeeId).HasDatabaseName("IX_FeeInstallments_StudentFeeId");
        builder.HasIndex(fi => fi.DueDate).HasDatabaseName("IX_FeeInstallments_DueDate");
        builder.HasIndex(fi => fi.Status).HasDatabaseName("IX_FeeInstallments_Status");
    }
}
