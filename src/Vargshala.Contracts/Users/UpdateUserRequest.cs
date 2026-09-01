using System.ComponentModel.DataAnnotations;

namespace Vargshala.Contracts.Users;

public class UpdateUserRequest
{
    [MaxLength(100)]
    public string? FirstName { get; set; }

    [MaxLength(100)]
    public string? LastName { get; set; }

    [EmailAddress]
    [MaxLength(150)]
    public string? Email { get; set; }

    [MaxLength(20)]
    public string? Mobile { get; set; }

    public string? Role { get; set; }

    public bool? IsActive { get; set; }
}
