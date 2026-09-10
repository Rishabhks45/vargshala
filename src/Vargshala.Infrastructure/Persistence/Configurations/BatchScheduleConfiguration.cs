using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Vargshala.Domain.Entities;

namespace Vargshala.Infrastructure.Persistence.Configurations;

public class BatchScheduleConfiguration : IEntityTypeConfiguration<BatchSchedule>
{
    public void Configure(EntityTypeBuilder<BatchSchedule> builder)
    {
        builder.ToTable("BatchSchedules");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.BatchId)
            .IsRequired();

        builder.Property(s => s.DayOfWeek)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(s => s.StartTime)
            .IsRequired();

        builder.Property(s => s.EndTime)
            .IsRequired();

        builder.Property(s => s.RoomOrLocation)
            .HasMaxLength(150);

        builder.Property(s => s.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(s => s.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("NOW()");

        builder.Property(s => s.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);

        // Foreign Key
        builder.HasOne(s => s.Batch)
            .WithMany(b => b.BatchSchedules)
            .HasForeignKey(s => s.BatchId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(s => s.BatchId)
            .HasDatabaseName("IX_BatchSchedules_BatchId");

        builder.HasIndex(s => s.DayOfWeek)
            .HasDatabaseName("IX_BatchSchedules_DayOfWeek");
    }
}
