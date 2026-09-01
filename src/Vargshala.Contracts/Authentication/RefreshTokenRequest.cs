using System.ComponentModel.DataAnnotations;

namespace Vargshala.Contracts.Authentication;

public class RefreshTokenRequest
{
    [Required]
    public string AccessToken { get; set; } = string.Empty;

    [Required]
    public string RefreshToken { get; set; } = string.Empty;
}
