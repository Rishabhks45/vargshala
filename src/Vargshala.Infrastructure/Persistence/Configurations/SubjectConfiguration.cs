using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Vargshala.Domain.Entities;

namespace Vargshala.Infrastructure.Persistence.Configurations;

public class SubjectConfiguration : IEntityTypeConfiguration<Subject>
{
    public void Configure(EntityTypeBuilder<Subject> builder)
    {
        builder.ToTable("Subjects");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.Id)
            .HasDefaultValueSql("gen_random_uuid()");

        builder.Property(s => s.OrganizationId)
            .IsRequired();

        builder.Property(s => s.Name)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(s => s.Code)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(s => s.Description);

        builder.Property(s => s.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        // Audit properties
        builder.Property(s => s.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("NOW()");

        builder.Property(s => s.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);

        // Foreign key to Organization
        builder.HasOne(s => s.Organization)
            .WithMany(o => o.Subjects)
            .HasForeignKey(s => s.OrganizationId)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes & Constraints
        builder.HasIndex(s => new { s.OrganizationId, s.Code })
            .IsUnique()
            .HasDatabaseName("UQ_Subjects_OrganizationId_Code");

        builder.HasIndex(s => s.OrganizationId)
            .HasDatabaseName("IX_Subjects_OrganizationId");
    }
}
