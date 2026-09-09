using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Vargshala.Domain.Entities;

namespace Vargshala.Infrastructure.Persistence.Configurations;

public class BatchStudentConfiguration : IEntityTypeConfiguration<BatchStudent>
{
    public void Configure(EntityTypeBuilder<BatchStudent> builder)
    {
        builder.ToTable("BatchStudents");

        builder.HasKey(bs => bs.Id);

        builder.Property(bs => bs.BatchId)
            .IsRequired();

        builder.Property(bs => bs.StudentId)
            .IsRequired();

        builder.Property(bs => bs.IsPrimary)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(bs => bs.EnrollmentType)
            .IsRequired()
            .HasMaxLength(50)
            .HasDefaultValue("Regular");

        builder.Property(bs => bs.JoinedAt)
            .IsRequired()
            .HasDefaultValueSql("NOW()");

        builder.Property(bs => bs.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(bs => bs.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("NOW()");

        // Relationships
        builder.HasOne(bs => bs.Batch)
            .WithMany(b => b.BatchStudents)
            .HasForeignKey(bs => bs.BatchId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(bs => bs.Student)
            .WithMany(s => s.BatchStudents)
            .HasForeignKey(bs => bs.StudentId)
            .OnDelete(DeleteBehavior.Restrict);

        // Unique constraint: (BatchId, StudentId)
        builder.HasIndex(bs => new { bs.BatchId, bs.StudentId })
            .IsUnique()
            .HasDatabaseName("UQ_BatchStudents_BatchId_StudentId");

        builder.HasIndex(bs => bs.BatchId)
            .HasDatabaseName("IX_BatchStudents_BatchId");

        builder.HasIndex(bs => bs.StudentId)
            .HasDatabaseName("IX_BatchStudents_StudentId");
    }
}
