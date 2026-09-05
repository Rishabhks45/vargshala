using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Vargshala.Domain.Entities;

namespace Vargshala.Infrastructure.Persistence.Configurations;

public class UserBranchAccessConfiguration : IEntityTypeConfiguration<UserBranchAccess>
{
    public void Configure(EntityTypeBuilder<UserBranchAccess> builder)
    {
        builder.ToTable("UserBranchAccess");

        builder.HasKey(uba => uba.Id);

        builder.Property(uba => uba.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(uba => uba.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("NOW()");

        // Relationships
        builder.HasOne(uba => uba.User)
            .WithMany(u => u.UserBranchAccesses)
            .HasForeignKey(uba => uba.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(uba => uba.Branch)
            .WithMany(b => b.UserBranchAccesses)
            .HasForeignKey(uba => uba.BranchId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes & Constraints
        builder.HasIndex(uba => new { uba.UserId, uba.BranchId })
            .IsUnique()
            .HasDatabaseName("UQ_UserBranchAccess_UserId_BranchId");

        builder.HasIndex(uba => uba.UserId)
            .HasDatabaseName("IX_UserBranchAccess_UserId");

        builder.HasIndex(uba => uba.BranchId)
            .HasDatabaseName("IX_UserBranchAccess_BranchId");
    }
}
