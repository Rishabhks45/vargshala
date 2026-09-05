using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Vargshala.Domain.Entities;

namespace Vargshala.Infrastructure.Persistence.Configurations;

public class TeacherConfiguration : IEntityTypeConfiguration<Teacher>
{
    public void Configure(EntityTypeBuilder<Teacher> builder)
    {
        builder.ToTable("Teachers");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.EmployeeCode).HasMaxLength(50);
        builder.Property(t => t.Department).HasMaxLength(100);
        builder.Property(t => t.Designation).HasMaxLength(100);

        builder.Property(t => t.HighestQualification).HasMaxLength(150);
        builder.Property(t => t.Specialization).HasMaxLength(150);
        builder.Property(t => t.TeachingExperienceYears).HasPrecision(5, 2);

        builder.Property(t => t.City).HasMaxLength(100);
        builder.Property(t => t.State).HasMaxLength(100);
        builder.Property(t => t.PostalCode).HasMaxLength(20);
        builder.Property(t => t.Country).HasMaxLength(100);

        builder.Property(t => t.AadharNumber).HasMaxLength(20);
        builder.Property(t => t.PreviousInstitute).HasMaxLength(200);

        // One-to-one relationship with User
        builder.HasOne(t => t.User)
            .WithOne()
            .HasForeignKey<Teacher>(t => t.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(t => t.UserId).IsUnique();
        builder.HasIndex(t => t.EmployeeCode).IsUnique();
        builder.HasIndex(t => t.Department);
        builder.HasIndex(t => t.Designation);
    }
}
