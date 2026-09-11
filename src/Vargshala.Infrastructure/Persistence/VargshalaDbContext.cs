using Microsoft.EntityFrameworkCore;
using Vargshala.Application.Abstractions.Persistence;
using Vargshala.Domain.Entities;

namespace Vargshala.Infrastructure.Persistence;

public class VargshalaDbContext : DbContext, IVargshalaDbContext
{
    public VargshalaDbContext(DbContextOptions<VargshalaDbContext> options) : base(options)
    {
    }

    public DbSet<Organization> Organizations => Set<Organization>();
    public DbSet<User> Users => Set<User>();
    public DbSet<EmailTemplate> EmailTemplates => Set<EmailTemplate>();
    public DbSet<Coupon> Coupons => Set<Coupon>();
    public DbSet<Student> Students => Set<Student>();
    public DbSet<Teacher> Teachers => Set<Teacher>();
    public DbSet<Branch> Branches => Set<Branch>();
    public DbSet<UserBranchAccess> UserBranchAccesses => Set<UserBranchAccess>();
    public DbSet<Subject> Subjects => Set<Subject>();
    public DbSet<Class> Classes => Set<Class>();
    public DbSet<Batch> Batches => Set<Batch>();
    public DbSet<BatchTeacher> BatchTeachers => Set<BatchTeacher>();
    public DbSet<BatchStudent> BatchStudents => Set<BatchStudent>();
    public DbSet<BatchSchedule> BatchSchedules => Set<BatchSchedule>();
    public DbSet<ClassSession> ClassSessions => Set<ClassSession>();
    public DbSet<Attendance> Attendances => Set<Attendance>();
    public DbSet<FeeStructure> FeeStructures => Set<FeeStructure>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(VargshalaDbContext).Assembly);

        // Global query filter for soft delete
        modelBuilder.Entity<Organization>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<User>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<EmailTemplate>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Coupon>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Student>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Teacher>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Branch>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Subject>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Class>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Batch>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<BatchSchedule>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<ClassSession>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<FeeStructure>().HasQueryFilter(e => !e.IsDeleted);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        foreach (var entry in ChangeTracker.Entries<Domain.Common.BaseEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedAt = DateTime.UtcNow;
                    if (entry.Entity.Id == Guid.Empty)
                    {
                        entry.Entity.Id = Guid.NewGuid();
                    }
                    break;

                case EntityState.Modified:
                    entry.Entity.UpdatedAt = DateTime.UtcNow;
                    break;
            }
        }

        // Ensure all DateTime / DateTime? properties with Kind=Unspecified are converted to Utc for Npgsql timestamptz
        foreach (var entry in ChangeTracker.Entries())
        {
            if (entry.State == EntityState.Added || entry.State == EntityState.Modified)
            {
                foreach (var property in entry.Properties)
                {
                    if (property.Metadata.ClrType == typeof(DateTime) && property.CurrentValue is DateTime dt && dt.Kind == DateTimeKind.Unspecified)
                    {
                        property.CurrentValue = DateTime.SpecifyKind(dt, DateTimeKind.Utc);
                    }
                    else if (property.Metadata.ClrType == typeof(DateTime?) && property.CurrentValue is DateTime dtNullable && dtNullable.Kind == DateTimeKind.Unspecified)
                    {
                        property.CurrentValue = DateTime.SpecifyKind(dtNullable, DateTimeKind.Utc);
                    }
                }
            }
        }

        return await base.SaveChangesAsync(cancellationToken);
    }
}
