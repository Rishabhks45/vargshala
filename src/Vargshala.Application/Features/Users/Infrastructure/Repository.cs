using Microsoft.EntityFrameworkCore;
using Vargshala.Application.Abstractions.Persistence;
using Vargshala.Domain.Entities;

namespace Vargshala.Application.Features.Users.Infrastructure;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<User?> GetByEmailAndOrgAsync(string email, Guid organizationId, CancellationToken cancellationToken = default);
    Task<bool> ExistsByEmailAndOrgAsync(string email, Guid organizationId, CancellationToken cancellationToken = default);
    Task<(List<User> Items, int TotalCount)> GetPagedByOrgAsync(Guid organizationId, int page, int pageSize, CancellationToken cancellationToken = default);
    Task AddAsync(User user, CancellationToken cancellationToken = default);
    void Update(User user);
    void Delete(User user);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

public class UserRepository : IUserRepository
{
    private readonly IVargshalaDbContext _db;

    public UserRepository(IVargshalaDbContext db)
    {
        _db = db;
    }

    public async Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _db.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == id && !u.IsDeleted, cancellationToken);
    }

    public async Task<User?> GetByEmailAndOrgAsync(string email, Guid organizationId, CancellationToken cancellationToken = default)
    {
        return await _db.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Email == email && u.OrganizationId == organizationId && !u.IsDeleted, cancellationToken);
    }

    public async Task<bool> ExistsByEmailAndOrgAsync(string email, Guid organizationId, CancellationToken cancellationToken = default)
    {
        return await _db.Users
            .AnyAsync(u => u.Email == email && u.OrganizationId == organizationId && !u.IsDeleted, cancellationToken);
    }

    public async Task<(List<User> Items, int TotalCount)> GetPagedByOrgAsync(Guid organizationId, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = _db.Users
            .AsNoTracking()
            .Where(u => u.OrganizationId == organizationId && !u.IsDeleted)
            .OrderBy(u => u.FirstName)
            .ThenBy(u => u.LastName);

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task AddAsync(User user, CancellationToken cancellationToken = default)
    {
        await _db.Users.AddAsync(user, cancellationToken);
    }

    public void Update(User user)
    {
        _db.Users.Update(user);
    }

    public void Delete(User user)
    {
        user.IsDeleted = true;
        user.DeletedAt = DateTime.UtcNow;
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _db.SaveChangesAsync(cancellationToken);
    }
}
