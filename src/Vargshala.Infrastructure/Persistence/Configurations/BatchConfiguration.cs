using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Vargshala.Domain.Entities;

namespace Vargshala.Infrastructure.Persistence.Configurations;

public class BatchConfiguration : IEntityTypeConfiguration<Batch>
{
    public void Configure(EntityTypeBuilder<Batch> builder)
    {
        builder.ToTable("Batches");

        builder.HasKey(b => b.Id);

        builder.Property(b => b.ClassId)
            .IsRequired();

        builder.Property(b => b.SubjectId)
            .IsRequired();

        builder.Property(b => b.Name)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(b => b.Code)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(b => b.StartTime);
        builder.Property(b => b.EndTime);

        builder.Property(b => b.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(b => b.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("NOW()");

        builder.Property(b => b.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);

        // Foreign Keys
        builder.HasOne(b => b.Class)
            .WithMany(c => c.Batches)
            .HasForeignKey(b => b.ClassId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(b => b.Subject)
            .WithMany(s => s.Batches)
            .HasForeignKey(b => b.SubjectId)
            .OnDelete(DeleteBehavior.Restrict);

        // Unique constraint: (ClassId, Code)
        builder.HasIndex(b => new { b.ClassId, b.Code })
            .IsUnique()
            .HasDatabaseName("UQ_Batches_ClassId_Code");

        builder.HasIndex(b => b.ClassId)
            .HasDatabaseName("IX_Batches_ClassId");

        builder.HasIndex(b => b.SubjectId)
            .HasDatabaseName("IX_Batches_SubjectId");
    }
}
