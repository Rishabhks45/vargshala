using Vargshala.Domain.Common;
using Vargshala.Contracts.Common;

namespace Vargshala.Domain.Entities;

public class User : BaseEntity
{
    public Guid? OrganizationId { get; set; }

    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public string? Email { get; set; }

    public string? Mobile { get; set; }

    public string PasswordHash { get; set; } = string.Empty;

    public UserRole Role { get; set; }

    public bool EmailVerified { get; set; }

    public bool MobileVerified { get; set; }

    public DateTime? LastLoginAt { get; set; }

    // Refresh Token
    public string? RefreshToken { get; set; }

    public DateTime? RefreshTokenExpiryTime { get; set; }

    // Navigation
    public Organization? Organization { get; set; }
}
