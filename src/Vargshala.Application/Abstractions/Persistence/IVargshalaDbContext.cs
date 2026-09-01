using Microsoft.EntityFrameworkCore;
using Vargshala.Domain.Entities;

namespace Vargshala.Application.Abstractions.Persistence;

public interface IVargshalaDbContext
{
    DbSet<Organization> Organizations { get; }
    DbSet<User> Users { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
