using Vargshala.Contracts.Common;
using Vargshala.Domain.Entities;

namespace Vargshala.Application.Features.Users.Infrastructure;

#region Interface
public interface IUserRepository
{
    Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<User?> GetByIdWithOrgAsync(Guid id, CancellationToken cancellationToken = default);
    Task<User?> GetByIdForUpdateAsync(Guid id, CancellationToken cancellationToken = default);
    Task<User?> GetByEmailAndOrgAsync(string email, Guid organizationId, CancellationToken cancellationToken = default);
    Task<bool> ExistsByEmailAndOrgAsync(string email, Guid organizationId, CancellationToken cancellationToken = default);
    Task<(List<User> Items, int TotalRecords)> GetPagedByOrgAsync(Guid organizationId, PagedRequest request, CancellationToken cancellationToken = default);
    Task<(List<User> Items, int TotalRecords)> GetControlPanelUsersPagedAsync(PagedRequest request, UserRole? role = null, bool? isActive = null, CancellationToken cancellationToken = default);
    Task AddAsync(User user, CancellationToken cancellationToken = default);
    void Update(User user);
    void Delete(User user);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
#endregion
