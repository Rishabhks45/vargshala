using Vargshala.Domain.Entities;

namespace Vargshala.Application.Features.Authentication.Infrastructure;

#region Interface
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
#endregion
