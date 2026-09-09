using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Vargshala.Domain.Entities;

namespace Vargshala.Infrastructure.Persistence.Configurations;

public class BatchTeacherConfiguration : IEntityTypeConfiguration<BatchTeacher>
{
    public void Configure(EntityTypeBuilder<BatchTeacher> builder)
    {
        builder.ToTable("BatchTeachers");

        builder.HasKey(bt => bt.Id);

        builder.Property(bt => bt.BatchId)
            .IsRequired();

        builder.Property(bt => bt.TeacherId)
            .IsRequired();

        builder.Property(bt => bt.AssignedAt)
            .IsRequired()
            .HasDefaultValueSql("NOW()");

        builder.Property(bt => bt.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(bt => bt.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("NOW()");

        // Relationships
        builder.HasOne(bt => bt.Batch)
            .WithMany(b => b.BatchTeachers)
            .HasForeignKey(bt => bt.BatchId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(bt => bt.Teacher)
            .WithMany(t => t.BatchTeachers)
            .HasForeignKey(bt => bt.TeacherId)
            .OnDelete(DeleteBehavior.Restrict);

        // Unique constraint: (BatchId, TeacherId)
        builder.HasIndex(bt => new { bt.BatchId, bt.TeacherId })
            .IsUnique()
            .HasDatabaseName("UQ_BatchTeachers_BatchId_TeacherId");

        builder.HasIndex(bt => bt.BatchId)
            .HasDatabaseName("IX_BatchTeachers_BatchId");

        builder.HasIndex(bt => bt.TeacherId)
            .HasDatabaseName("IX_BatchTeachers_TeacherId");
    }
}
