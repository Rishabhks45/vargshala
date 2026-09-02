using System.ComponentModel.DataAnnotations;

namespace Vargshala.Contracts.Authentication;

public class ForgotPasswordRequest
{
    [Required(ErrorMessage = "Email address is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    public string Email { get; set; } = string.Empty;
}
