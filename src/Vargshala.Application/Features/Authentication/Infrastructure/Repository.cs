using Microsoft.EntityFrameworkCore;
using Vargshala.Application.Abstractions.Persistence;
using Vargshala.Domain.Entities;

namespace Vargshala.Application.Features.Authentication.Infrastructure;

public interface IAuthRepository
{
    Task<User?> GetUserByEmailWithOrgAsync(string email, CancellationToken cancellationToken = default);
    Task<User?> GetUserByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Organization?> GetOrganizationByCodeAsync(string code, CancellationToken cancellationToken = default);
    Task<bool> OrganizationCodeExistsAsync(string code, CancellationToken cancellationToken = default);
    Task<bool> UserEmailExistsAsync(string email, CancellationToken cancellationToken = default);
    Task<bool> UserEmailExistsInOrgAsync(string email, Guid organizationId, CancellationToken cancellationToken = default);
    Task AddOrganizationAsync(Organization organization, CancellationToken cancellationToken = default);
    Task AddUserAsync(User user, CancellationToken cancellationToken = default);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

public class AuthRepository : IAuthRepository
{
    private readonly IVargshalaDbContext _db;

    public AuthRepository(IVargshalaDbContext db)
    {
        _db = db;
    }

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
}
