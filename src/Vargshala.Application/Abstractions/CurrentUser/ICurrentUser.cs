using Vargshala.Contracts.Common;

namespace Vargshala.Application.Abstractions.CurrentUser;

public interface ICurrentUser
{
    Guid UserId { get; }
    Guid? OrganizationId { get; }
    string Role { get; }
    UserRole? UserRole { get; }
    bool IsAuthenticated { get; }
    bool IsSuperAdmin { get; }
}
