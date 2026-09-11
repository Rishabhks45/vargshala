using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Vargshala.Domain.Entities;

namespace Vargshala.Infrastructure.Persistence.Configurations;

public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.ToTable("Payments");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.OrganizationId).IsRequired();
        builder.Property(p => p.BranchId);
        builder.Property(p => p.StudentId).IsRequired();

        builder.Property(p => p.ReceiptNumber)
            .HasMaxLength(50);

        builder.Property(p => p.Amount)
            .HasPrecision(12, 2)
            .IsRequired();

        builder.Property(p => p.PaymentDate)
            .IsRequired()
            .HasDefaultValueSql("NOW()");

        builder.Property(p => p.PaymentMethod)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(p => p.TransactionReference)
            .HasMaxLength(100);

        builder.Property(p => p.Status)
            .IsRequired()
            .HasMaxLength(30)
            .HasDefaultValue("Completed");

        builder.Property(p => p.Remarks);

        builder.Property(p => p.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(p => p.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("NOW()");

        builder.Property(p => p.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);

        // Foreign keys
        builder.HasOne(p => p.Organization)
            .WithMany()
            .HasForeignKey(p => p.OrganizationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(p => p.Branch)
            .WithMany()
            .HasForeignKey(p => p.BranchId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(p => p.Student)
            .WithMany(s => s.Payments)
            .HasForeignKey(p => p.StudentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(p => p.CreatedByUser)
            .WithMany()
            .HasForeignKey(p => p.CreatedBy)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(p => p.Allocations)
            .WithOne(pa => pa.Payment)
            .HasForeignKey(pa => pa.PaymentId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(p => new { p.OrganizationId, p.ReceiptNumber })
            .IsUnique()
            .HasDatabaseName("UQ_Payments_OrganizationId_ReceiptNumber");

        builder.HasIndex(p => p.OrganizationId).HasDatabaseName("IX_Payments_OrganizationId");
        builder.HasIndex(p => p.BranchId).HasDatabaseName("IX_Payments_BranchId");
        builder.HasIndex(p => p.StudentId).HasDatabaseName("IX_Payments_StudentId");
        builder.HasIndex(p => p.ReceiptNumber).HasDatabaseName("IX_Payments_ReceiptNumber");
        builder.HasIndex(p => p.PaymentDate).HasDatabaseName("IX_Payments_PaymentDate");
        builder.HasIndex(p => p.TransactionReference).HasDatabaseName("IX_Payments_TransactionReference");
        builder.HasIndex(p => new { p.OrganizationId, p.PaymentDate }).HasDatabaseName("IX_Payments_Org_Date");
    }
}
