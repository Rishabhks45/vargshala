using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Vargshala.Domain.Entities;

namespace Vargshala.Infrastructure.Persistence.Configurations;

public class StudentConfiguration : IEntityTypeConfiguration<Student>
{
    public void Configure(EntityTypeBuilder<Student> builder)
    {
        builder.ToTable("Students");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.Gender).HasMaxLength(20);
        builder.Property(s => s.BloodGroup).HasMaxLength(10);
        builder.Property(s => s.Nationality).HasMaxLength(50);

        builder.Property(s => s.StudentCode).HasMaxLength(50);
        builder.Property(s => s.ClassName).HasMaxLength(100);
        builder.Property(s => s.Section).HasMaxLength(50);
        builder.Property(s => s.RollNumber).HasMaxLength(50);

        builder.Property(s => s.FatherName).HasMaxLength(150);
        builder.Property(s => s.FatherMobile).HasMaxLength(20);
        builder.Property(s => s.FatherAlternateMobile).HasMaxLength(20);
        builder.Property(s => s.MotherName).HasMaxLength(150);

        builder.Property(s => s.City).HasMaxLength(100);
        builder.Property(s => s.State).HasMaxLength(100);
        builder.Property(s => s.PostalCode).HasMaxLength(20);
        builder.Property(s => s.Country).HasMaxLength(100);

        builder.Property(s => s.EmergencyContactName).HasMaxLength(150);
        builder.Property(s => s.EmergencyContactMobile).HasMaxLength(20);
        builder.Property(s => s.EmergencyContactRelation).HasMaxLength(50);

        builder.Property(s => s.AadharNumber).HasMaxLength(20);
        builder.Property(s => s.PreviousInstitute).HasMaxLength(200);

        // One-to-one relationship with User
        builder.HasOne(s => s.User)
            .WithOne()
            .HasForeignKey<Student>(s => s.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(s => s.UserId).IsUnique();
        builder.HasIndex(s => s.StudentCode).IsUnique();
        builder.HasIndex(s => new { s.ClassName, s.Section });
    }
}
