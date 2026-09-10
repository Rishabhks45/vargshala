using Microsoft.EntityFrameworkCore;
using Vargshala.Domain.Entities;

namespace Vargshala.Application.Abstractions.Persistence;

public interface IVargshalaDbContext
{
    DbSet<Organization> Organizations { get; }
    DbSet<User> Users { get; }
    DbSet<EmailTemplate> EmailTemplates { get; }
    DbSet<Coupon> Coupons { get; }
    DbSet<Student> Students { get; }
    DbSet<Teacher> Teachers { get; }
    DbSet<Branch> Branches { get; }
    DbSet<UserBranchAccess> UserBranchAccesses { get; }
    DbSet<Subject> Subjects { get; }
    DbSet<Class> Classes { get; }
    DbSet<Batch> Batches { get; }
    DbSet<BatchTeacher> BatchTeachers { get; }
    DbSet<BatchStudent> BatchStudents { get; }
    DbSet<BatchSchedule> BatchSchedules { get; }
    DbSet<ClassSession> ClassSessions { get; }
    DbSet<Attendance> Attendances { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
