using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Vargshala.Domain.Entities;

namespace Vargshala.Infrastructure.Persistence.Configurations;

public class ClassSessionConfiguration : IEntityTypeConfiguration<ClassSession>
{
    public void Configure(EntityTypeBuilder<ClassSession> builder)
    {
        builder.ToTable("ClassSessions");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.BatchId)
            .IsRequired();

        builder.Property(c => c.BatchScheduleId);

        builder.Property(c => c.TeacherId);

        builder.Property(c => c.SessionDate)
            .IsRequired();

        builder.Property(c => c.StartTime)
            .IsRequired();

        builder.Property(c => c.EndTime)
            .IsRequired();

        builder.Property(c => c.Topic)
            .HasMaxLength(250);

        builder.Property(c => c.Notes);

        builder.Property(c => c.Status)
            .IsRequired()
            .HasMaxLength(30)
            .HasDefaultValue("Scheduled");

        builder.Property(c => c.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(c => c.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("NOW()");

        builder.Property(c => c.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);

        // Foreign Keys
        builder.HasOne(c => c.Batch)
            .WithMany(b => b.ClassSessions)
            .HasForeignKey(c => c.BatchId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(c => c.BatchSchedule)
            .WithMany(s => s.ClassSessions)
            .HasForeignKey(c => c.BatchScheduleId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(c => c.Teacher)
            .WithMany(t => t.ClassSessions)
            .HasForeignKey(c => c.TeacherId)
            .OnDelete(DeleteBehavior.SetNull);

        // Indexes
        builder.HasIndex(c => c.BatchId)
            .HasDatabaseName("IX_ClassSessions_BatchId");

        builder.HasIndex(c => c.SessionDate)
            .HasDatabaseName("IX_ClassSessions_SessionDate");

        builder.HasIndex(c => c.TeacherId)
            .HasDatabaseName("IX_ClassSessions_TeacherId");

        builder.HasIndex(c => c.BatchScheduleId)
            .HasDatabaseName("IX_ClassSessions_BatchScheduleId");

        builder.HasIndex(c => new { c.BatchId, c.SessionDate })
            .HasDatabaseName("IX_ClassSessions_BatchId_SessionDate");
    }
}
