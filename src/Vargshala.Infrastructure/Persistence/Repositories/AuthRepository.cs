using Microsoft.EntityFrameworkCore;
using Vargshala.Application.Abstractions.Persistence;
using Vargshala.Application.Features.Authentication.Infrastructure;
using Vargshala.Domain.Entities;

namespace Vargshala.Infrastructure.Persistence.Repositories;

public class AuthRepository : IAuthRepository
{
    #region Fields & Constructor
    private readonly IVargshalaDbContext _db;

    public AuthRepository(IVargshalaDbContext db)
    {
        _db = db;
    }
    #endregion

    #region Query Methods
    public async Task<User?> GetUserByEmailWithOrgAsync(string email, CancellationToken cancellationToken = default)
    {
        return await _db.Users
            .Include(u => u.Organization)
            .FirstOrDefaultAsync(u => u.Email == email && !u.IsDeleted, cancellationToken);
    }

    public async Task<User?> GetUserByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _db.Users
            .FirstOrDefaultAsync(u => u.Id == id && !u.IsDeleted, cancellationToken);
    }

    public async Task<Organization?> GetOrganizationByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        return await _db.Organizations
            .FirstOrDefaultAsync(o => o.Code == code && !o.IsDeleted, cancellationToken);
    }

    public async Task<bool> OrganizationCodeExistsAsync(string code, CancellationToken cancellationToken = default)
    {
        return await _db.Organizations
            .AnyAsync(o => o.Code == code && !o.IsDeleted, cancellationToken);
    }

    public async Task<bool> UserEmailExistsAsync(string email, CancellationToken cancellationToken = default)
    {
        return await _db.Users
            .AnyAsync(u => u.Email == email && !u.IsDeleted, cancellationToken);
    }

    public async Task<bool> UserEmailExistsInOrgAsync(string email, Guid organizationId, CancellationToken cancellationToken = default)
    {
        return await _db.Users
            .AnyAsync(u => u.Email == email && u.OrganizationId == organizationId && !u.IsDeleted, cancellationToken);
    }
    #endregion

    #region Command Methods
    public async Task AddOrganizationAsync(Organization organization, CancellationToken cancellationToken = default)
    {
        await _db.Organizations.AddAsync(organization, cancellationToken);
    }

    public async Task AddUserAsync(User user, CancellationToken cancellationToken = default)
    {
        await _db.Users.AddAsync(user, cancellationToken);
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _db.SaveChangesAsync(cancellationToken);
    }
    #endregion
}
