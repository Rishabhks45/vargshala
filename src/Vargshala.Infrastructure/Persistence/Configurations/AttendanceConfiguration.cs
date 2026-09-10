using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Vargshala.Domain.Entities;

namespace Vargshala.Infrastructure.Persistence.Configurations;

public class AttendanceConfiguration : IEntityTypeConfiguration<Attendance>
{
    public void Configure(EntityTypeBuilder<Attendance> builder)
    {
        builder.ToTable("Attendances");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.ClassSessionId)
            .IsRequired();

        builder.Property(a => a.StudentId)
            .IsRequired();

        builder.Property(a => a.Status)
            .IsRequired()
            .HasMaxLength(20)
            .HasDefaultValue("Present");

        builder.Property(a => a.MarkedAt);

        builder.Property(a => a.Remarks);

        builder.Property(a => a.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("NOW()");

        builder.Property(a => a.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);

        // Foreign Keys
        builder.HasOne(a => a.ClassSession)
            .WithMany(s => s.Attendances)
            .HasForeignKey(a => a.ClassSessionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(a => a.Student)
            .WithMany(s => s.Attendances)
            .HasForeignKey(a => a.StudentId)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes
        builder.HasIndex(a => a.ClassSessionId)
            .HasDatabaseName("IX_Attendances_ClassSessionId");

        builder.HasIndex(a => a.StudentId)
            .HasDatabaseName("IX_Attendances_StudentId");

        // Unique Constraint: UNIQUE(ClassSessionId, StudentId)
        builder.HasIndex(a => new { a.ClassSessionId, a.StudentId })
            .IsUnique()
            .HasDatabaseName("UQ_Attendances_ClassSessionId_StudentId");
    }
}
