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

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
