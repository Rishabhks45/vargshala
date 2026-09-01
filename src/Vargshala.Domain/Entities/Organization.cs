using Vargshala.Domain.Common;

namespace Vargshala.Domain.Entities;

public class Organization : BaseEntity
{
    public string Name { get; set; } = string.Empty;

    public string Code { get; set; } = string.Empty;

    public string? LogoUrl { get; set; }

    public string? Email { get; set; }

    public string? Mobile { get; set; }

    public string? Address { get; set; }

    public string? City { get; set; }

    public string? State { get; set; }

    public string? Pincode { get; set; }

    public string? AcademicSession { get; set; }

    // Navigation
    public ICollection<User> Users { get; set; } = new List<User>();
}
