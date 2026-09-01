namespace Vargshala.Application.Abstractions.CurrentUser;

public interface ICurrentUser
{
    Guid UserId { get; }
    Guid? OrganizationId { get; }
    string Role { get; }
    bool IsAuthenticated { get; }
    bool IsSuperAdmin { get; }
}
