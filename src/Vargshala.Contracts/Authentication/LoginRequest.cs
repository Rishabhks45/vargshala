using System.ComponentModel.DataAnnotations;

namespace Vargshala.Contracts.Authentication;

public class LoginRequest
{
    [Required]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;
}
