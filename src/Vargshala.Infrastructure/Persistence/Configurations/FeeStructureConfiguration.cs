using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Vargshala.Domain.Entities;

namespace Vargshala.Infrastructure.Persistence.Configurations;

public class FeeStructureConfiguration : IEntityTypeConfiguration<FeeStructure>
{
    public void Configure(EntityTypeBuilder<FeeStructure> builder)
    {
        builder.ToTable("FeeStructures");

        builder.HasKey(fs => fs.Id);

        builder.Property(fs => fs.OrganizationId).IsRequired();
        builder.Property(fs => fs.BranchId).IsRequired();
        builder.Property(fs => fs.ClassId);

        builder.Property(fs => fs.Name)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(fs => fs.Description);

        builder.Property(fs => fs.TotalAmount)
            .HasPrecision(12, 2)
            .IsRequired()
            .HasDefaultValue(0.00m);

        builder.Property(fs => fs.AcademicSession)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(fs => fs.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(fs => fs.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("NOW()");

        builder.Property(fs => fs.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);

        // Foreign keys
        builder.HasOne(fs => fs.Organization)
            .WithMany()
            .HasForeignKey(fs => fs.OrganizationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(fs => fs.Branch)
            .WithMany()
            .HasForeignKey(fs => fs.BranchId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(fs => fs.Class)
            .WithMany()
            .HasForeignKey(fs => fs.ClassId)
            .OnDelete(DeleteBehavior.SetNull);

        // Indexes
        builder.HasIndex(fs => fs.OrganizationId).HasDatabaseName("IX_FeeStructures_OrganizationId");
        builder.HasIndex(fs => fs.BranchId).HasDatabaseName("IX_FeeStructures_BranchId");
        builder.HasIndex(fs => fs.ClassId).HasDatabaseName("IX_FeeStructures_ClassId");
        builder.HasIndex(fs => new { fs.OrganizationId, fs.BranchId }).HasDatabaseName("IX_FeeStructures_Org_Branch");
        builder.HasIndex(fs => new { fs.AcademicSession, fs.IsActive }).HasDatabaseName("IX_FeeStructures_Session_IsActive");
    }
}
