using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Vargshala.Domain.Entities;

namespace Vargshala.Infrastructure.Persistence.Configurations;

public class BranchConfiguration : IEntityTypeConfiguration<Branch>
{
    public void Configure(EntityTypeBuilder<Branch> builder)
    {
        builder.ToTable("Branches");

        builder.HasKey(b => b.Id);

        builder.Property(b => b.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(b => b.Code)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(b => b.LogoUrl);

        builder.Property(b => b.Email)
            .HasMaxLength(150);

        builder.Property(b => b.Mobile)
            .HasMaxLength(20);

        builder.Property(b => b.AlternateMobile)
            .HasMaxLength(20);

        builder.Property(b => b.Address)
            .HasMaxLength(500);

        builder.Property(b => b.City)
            .HasMaxLength(100);

        builder.Property(b => b.State)
            .HasMaxLength(100);

        builder.Property(b => b.Pincode)
            .HasMaxLength(10);

        builder.Property(b => b.Country)
            .HasMaxLength(100);

        builder.Property(b => b.IsMainBranch)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(b => b.UseBranchName)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(b => b.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        // Audit properties
        builder.Property(b => b.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("NOW()");

        builder.Property(b => b.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);

        // Foreign key to Organization
        builder.HasOne(b => b.Organization)
            .WithMany(o => o.Branches)
            .HasForeignKey(b => b.OrganizationId)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes & Constraints
        builder.HasIndex(b => new { b.OrganizationId, b.Code })
            .IsUnique()
            .HasDatabaseName("UQ_Branches_OrganizationId_Code");

        builder.HasIndex(b => b.OrganizationId)
            .HasDatabaseName("IX_Branches_OrganizationId");

        builder.HasIndex(b => b.City)
            .HasDatabaseName("IX_Branches_City");
    }
}
