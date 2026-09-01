using System.ComponentModel.DataAnnotations;

namespace Vargshala.Contracts.Authentication;

public class RegisterOrganizationRequest
{
    [Required]
    [MaxLength(200)]
    public string OrganizationName { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    public string OrganizationCode { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string AdminFirstName { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string AdminLastName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [MaxLength(150)]
    public string AdminEmail { get; set; } = string.Empty;

    [MaxLength(20)]
    public string? AdminMobile { get; set; }

    [Required]
    [MinLength(8)]
    public string Password { get; set; } = string.Empty;
}
