using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Vargshala.Domain.Entities;

namespace Vargshala.Infrastructure.Persistence.Configurations;

public class StudentFeeConfiguration : IEntityTypeConfiguration<StudentFee>
{
    public void Configure(EntityTypeBuilder<StudentFee> builder)
    {
        builder.ToTable("StudentFees");

        builder.HasKey(sf => sf.Id);

        builder.Property(sf => sf.OrganizationId).IsRequired();
        builder.Property(sf => sf.StudentId).IsRequired();
        builder.Property(sf => sf.FeeStructureId).IsRequired();

        builder.Property(sf => sf.OriginalAmount)
            .HasPrecision(12, 2)
            .IsRequired();

        builder.Property(sf => sf.DiscountAmount)
            .HasPrecision(12, 2)
            .IsRequired()
            .HasDefaultValue(0.00m);

        builder.Property(sf => sf.FinalAmount)
            .HasPrecision(12, 2)
            .IsRequired();

        builder.Property(sf => sf.PaidAmount)
            .HasPrecision(12, 2)
            .IsRequired()
            .HasDefaultValue(0.00m);

        builder.Property(sf => sf.Status)
            .IsRequired()
            .HasMaxLength(30)
            .HasDefaultValue("Pending");

        builder.Property(sf => sf.AssignedAt)
            .IsRequired()
            .HasDefaultValueSql("NOW()");

        builder.Property(sf => sf.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(sf => sf.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("NOW()");

        builder.Property(sf => sf.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);

        // Foreign keys
        builder.HasOne(sf => sf.Organization)
            .WithMany()
            .HasForeignKey(sf => sf.OrganizationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(sf => sf.Student)
            .WithMany(s => s.StudentFees)
            .HasForeignKey(sf => sf.StudentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(sf => sf.FeeStructure)
            .WithMany()
            .HasForeignKey(sf => sf.FeeStructureId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(sf => sf.Discounts)
            .WithOne(d => d.StudentFee)
            .HasForeignKey(d => d.StudentFeeId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(sf => sf.Installments)
            .WithOne(i => i.StudentFee)
            .HasForeignKey(i => i.StudentFeeId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(sf => sf.OrganizationId).HasDatabaseName("IX_StudentFees_OrganizationId");
        builder.HasIndex(sf => sf.StudentId).HasDatabaseName("IX_StudentFees_StudentId");
        builder.HasIndex(sf => sf.FeeStructureId).HasDatabaseName("IX_StudentFees_FeeStructureId");
        builder.HasIndex(sf => new { sf.OrganizationId, sf.Status }).HasDatabaseName("IX_StudentFees_Org_Status");
    }
}
