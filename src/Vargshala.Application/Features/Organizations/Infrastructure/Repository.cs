using Microsoft.EntityFrameworkCore;
using Vargshala.Application.Abstractions.Persistence;
using Vargshala.Domain.Entities;

namespace Vargshala.Application.Features.Organizations.Infrastructure;

public interface IOrganizationRepository
{
    Task<Organization?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Organization?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);
    Task<bool> ExistsByCodeAsync(string code, CancellationToken cancellationToken = default);
    Task AddAsync(Organization organization, CancellationToken cancellationToken = default);
    void Update(Organization organization);
    void Delete(Organization organization);
    Task<List<Organization>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Organization?> GetByIdForUpdateAsync(Guid id, CancellationToken cancellationToken = default);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

public class OrganizationRepository : IOrganizationRepository
{
    private readonly IVargshalaDbContext _db;

    public OrganizationRepository(IVargshalaDbContext db)
    {
        _db = db;
    }

    public async Task<List<Organization>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _db.Organizations
            .AsNoTracking()
            .Where(o => !o.IsDeleted)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<Organization?> GetByIdForUpdateAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _db.Organizations
            .FirstOrDefaultAsync(o => o.Id == id && !o.IsDeleted, cancellationToken);
    }

    public async Task<Organization?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _db.Organizations
            .AsNoTracking()
            .FirstOrDefaultAsync(o => o.Id == id && !o.IsDeleted, cancellationToken);
    }

    public async Task<Organization?> GetByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        return await _db.Organizations
            .AsNoTracking()
            .FirstOrDefaultAsync(o => o.Code == code && !o.IsDeleted, cancellationToken);
    }

    public async Task<bool> ExistsByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        return await _db.Organizations
            .AnyAsync(o => o.Code == code && !o.IsDeleted, cancellationToken);
    }

    public async Task AddAsync(Organization organization, CancellationToken cancellationToken = default)
    {
        await _db.Organizations.AddAsync(organization, cancellationToken);
    }

    public void Update(Organization organization)
    {
        _db.Organizations.Update(organization);
    }

    public void Delete(Organization organization)
    {
        organization.IsDeleted = true;
        organization.DeletedAt = DateTime.UtcNow;
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _db.SaveChangesAsync(cancellationToken);
    }
}
